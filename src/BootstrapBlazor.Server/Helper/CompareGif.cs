using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BootstrapBlazor.Server.Helper
{
    public class GifComparisonResult
    {
        public bool FrameCountMatch { get; set; }
        public (int, int) FrameCounts { get; set; }
        public bool SizeMatch { get; set; }
        public (SixLabors.ImageSharp.Size, SixLabors.ImageSharp.Size) Sizes { get; set; }
        public List<double> FrameSimilarities { get; set; } = new();
        public double OverallSimilarity { get; set; }
        public string Gif1PathLocal { get; set; }
    }

    public static class CompareGifHelper
    {
        public static async Task<GifComparisonResult> CompareGifs(string gifName, string gif2Url, string dimesion = "")
        {
            var results = new GifComparisonResult();
            var tempFilePath = Path.GetTempFileName();
            try
            {
                //download ảnh từ url
                gif2Url = gif2Url.Replace("\n", "");
                var gif1Image = await DownloadImageFromUrl(gifName);
                var gif2Image = await DownloadImageFromUrl(gif2Url);

                using var gif1 = await Image.LoadAsync<Rgba32>(gif1Image);
                using var gif2 = await Image.LoadAsync<Rgba32>(gif2Image);

                var frames1 = new List<Image<Rgba32>>();
                var frames2 = new List<Image<Rgba32>>();

                // Extract frames from first GIF
                for (int i = 0; i < gif1.Frames.Count; i++)
                {
                    var frame = gif1.Frames[i];
                    var newFrame = new Image<Rgba32>(frame.Width, frame.Height);

                    // Copy pixels directly
                    for (int y = 0; y < frame.Height; y++)
                    {
                        for (int x = 0; x < frame.Width; x++)
                        {
                            var pixel = frame[x, y];
                            newFrame[x, y] = new Rgba32(pixel.R, pixel.G, pixel.B, pixel.A);
                        }
                    }

                    frames1.Add(newFrame);
                }

                // Extract frames from second GIF
                for (int i = 0; i < gif2.Frames.Count; i++)
                {
                    var frame = gif2.Frames[i];
                    var newFrame = new Image<Rgba32>(frame.Width, frame.Height);

                    // Copy pixels directly
                    for (int y = 0; y < frame.Height; y++)
                    {
                        for (int x = 0; x < frame.Width; x++)
                        {
                            var pixel = frame[x, y];
                            newFrame[x, y] = new Rgba32(pixel.R, pixel.G, pixel.B, pixel.A);
                        }
                    }

                    frames2.Add(newFrame);
                }

                results.FrameCountMatch = frames1.Count == frames2.Count;
                results.FrameCounts = (frames1.Count, frames2.Count);

                SixLabors.ImageSharp.Size? newGif2Size = null;
                if(!string.IsNullOrEmpty(dimesion))
                {
                    int[] dimensionArray = dimesion.Split('x').Select(int.Parse).ToArray();
                    newGif2Size = new SixLabors.ImageSharp.Size(dimensionArray[0], dimensionArray[1]);
                }

                results.SizeMatch = newGif2Size != null ? newGif2Size == gif2.Size : gif1.Size == gif2.Size;
                results.Sizes = (gif1.Size, gif2.Size) ;

                // If sizes don't match, we can't compare frame contents
                if (!results.SizeMatch)
                {
                    return results;
                }

                // Compare frames
                var minFrames = Math.Min(frames1.Count, frames2.Count);
                var totalSimilarity = 0.0;

                for (int i = 0; i < minFrames; i++)
                {
                    var similarity = CalculateFrameSimilarity(frames1[i], frames2[i]);
                    results.FrameSimilarities.Add(similarity);
                    totalSimilarity += similarity;
                }

                // Calculate overall similarity
                results.OverallSimilarity = minFrames > 0 ? totalSimilarity / minFrames : 0.0;
                return results;
            }
            catch(Exception ex)
            {
                return new GifComparisonResult
                {
                    FrameCountMatch = false,
                    FrameCounts = (0, 0),
                    SizeMatch = false,
                    Sizes = (SixLabors.ImageSharp.Size.Empty, SixLabors.ImageSharp.Size.Empty),
                    OverallSimilarity = 0.0,
                    Gif1PathLocal = string.Empty
                };
            }
            finally
            {
                // Ensure temporary file is deleted even if an exception occurs
                if (File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch
                    {
                        // Ignore any errors during cleanup
                    }
                }
            }
        }

        private static double CalculateFrameSimilarity(Image<Rgba32> frame1, Image<Rgba32> frame2)
        {
            double mse = 0;
            var width = frame1.Width;
            var height = frame1.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel1 = frame1[x, y];
                    var pixel2 = frame2[x, y];

                    mse += Math.Pow(pixel1.R - pixel2.R, 2) +
                           Math.Pow(pixel1.G - pixel2.G, 2) +
                           Math.Pow(pixel1.B - pixel2.B, 2);
                }
            }

            mse /= (width * height * 3); // Divide by total number of pixels and channels
            return 100 * (1 - mse / (255 * 255)); // Convert MSE to similarity score (0-100%)
        }

        public static string GetComparisonConclusion(GifComparisonResult results)
        {


            if (!results.SizeMatch)
            {
                return "FAILED - Kích thước ảnh không khớp";
            }

            if (!results.FrameCountMatch)
            {
                return "FAILED - Số lượng frame không khớp";
            }

            var similarity = results.OverallSimilarity;
            if (similarity >= 95)
            {
                return "OK";
            }
            else
            {
                return $"FAILED - Ảnh khác nhau (độ tương đồng: {similarity:F2}%)";
            }


        }

        public static string PrintComparisonResults(GifComparisonResult results)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("\nTHÔNG TIN CHI TIẾT:");
            sb.AppendLine("-".PadRight(50, '-'));
            sb.AppendLine($"GIF 1: {results.FrameCounts.Item1} frames");
            sb.AppendLine($"GIF 2: {results.FrameCounts.Item2} frames");
            sb.AppendLine($"Frame Count Match: {(results.FrameCountMatch ? "Yes" : "No")}");
            sb.AppendLine($"Size Match: {(results.SizeMatch ? "Yes" : "No")}");
            sb.AppendLine($"Sizes: GIF1={results.Sizes.Item1}, GIF2={results.Sizes.Item2}");
            sb.AppendLine($"Overall Similarity: {results.OverallSimilarity:F2}%");

            if (results.FrameSimilarities.Any())
            {
                sb.AppendLine("\nĐộ tương đồng từng frame:");
                for (int i = 0; i < results.FrameSimilarities.Count; i++)
                {
                    sb.AppendLine($"Frame {i + 1}: {results.FrameSimilarities[i]:F2}%");
                }
            }

            return sb.ToString();
        }


        private static async Task<string> DownloadImageFromUrl(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var tempFilePath = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempFilePath, imageBytes);
            return tempFilePath;
        }
    }
}
