using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using BootstrapBlazor.Server.Exceptions;

namespace BootstrapBlazor.Server.Http
{
    public static class RequestClient
    {
        private readonly static HttpClient _client;

        //intergated service
        public static IConfiguration Config;
        private static CancellationTokenSource _tokenSource;
        private static ILocalStorageService _localStorage;
        private static long UploadLimit = 25214400;
        static RequestClient()
        {
            _client = new HttpClient();
            _tokenSource = new CancellationTokenSource();
        }

        public static void CancelToken()
        {
            //ko được sử dụng cancel ở đây exception IO
        }

        public static void Initialize(IConfiguration configuration)
        {
            Config = configuration;
            _client.BaseAddress = new Uri(Config["RemoteServices:BaseUrl"]);
        }

        public static void InjectServices(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public static void AttachToken(string Token = "")
        {
            if (!string.IsNullOrEmpty(Token))
            {
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }
        }


        public static async Task<T> GetAPIAsync<T>([Required] string URL)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                httpResponseMessage = await _client.GetAsync(URL, _tokenSource.Token);
                return await ReturnApiResponse<T>(httpResponseMessage);
            }
            catch (Exception ex)
            {
                return await ReturnApiResponse<T>(null);
            }
        }

        public static async Task<T> PostAPIAsync<T>([Required] string URL, dynamic input, bool notifyOk = true)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            StringContent content =
                new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");

            httpResponseMessage = await _client.PostAsync(URL, content, _tokenSource.Token);
            

            return await ReturnApiResponse<T>(httpResponseMessage);
        }

        public static async Task<byte[]> PostAPIBytesAsync([Required] string URL, dynamic input)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            StringContent content =
                new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");

            httpResponseMessage = await _client.PostAsync(URL, content, _tokenSource.Token);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return await httpResponseMessage.Content.ReadAsByteArrayAsync();
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _localStorage.RemoveItemAsync("my-access-token");
                await _localStorage.RemoveItemAsync("my-refresh-token");
                throw new UnauthorizedException("");
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new ServerErrorException("Server-Error");
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                string errorMessage = await httpResponseMessage.Content.ReadAsStringAsync();
                throw new BadRequestException(errorMessage);
            }

            throw new Exception($"API call failed with status code: {httpResponseMessage.StatusCode}");
        }

        public static async Task<T> PatchAPIAsync<T>([Required] string URL, dynamic input, bool notifyOk = true)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();


            StringContent content =
                new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");

            httpResponseMessage = await _client.PatchAsync(URL, content, _tokenSource.Token);
            return await ReturnApiResponse<T>(httpResponseMessage);
        }

        public static async Task<T> PostAPIWithFileAsync<T>([Required] string URL, IBrowserFile file)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            var streams = new List<MemoryStream>();

            using (var content = new MultipartFormDataContent())
            {
                var stream = file.OpenReadStream(UploadLimit);
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "file", file.Name);
                httpResponseMessage = await _client.PostAsync(URL, content, _tokenSource.Token);
                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                foreach (var item in streams)
                {
                    item.Close();
                }
                return await ReturnApiResponse<T>(httpResponseMessage);
            }
        }

        public static async Task<T> PostAPIWithMultipleFileAsync<T>([Required] string URL, List<IBrowserFile> files)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

            var streams = new List<MemoryStream>();

            using (var content = new MultipartFormDataContent())
            {
                foreach (var file in files)
                {
                    var ms = new MemoryStream();
                    await file.OpenReadStream(UploadLimit).CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    content.Add(new StreamContent(ms), $"files", file.Name);
                    streams.Add(ms);

                }

                httpResponseMessage = await _client.PostAsync(URL, content, _tokenSource.Token);

                var response = await httpResponseMessage.Content.ReadAsStringAsync();

                foreach (var item in streams)
                {
                    item.Close();
                }

                return await ReturnApiResponse<T>(httpResponseMessage);
            }
        }


        public static async Task<T> PutAPIAsync<T>([Required] string URL, dynamic input)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            StringContent content =
                new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
            httpResponseMessage = await _client.PutAsync(URL, content, _tokenSource.Token);
            return await ReturnApiResponse<T>(httpResponseMessage);
        }

        public static async Task<T> DeleteAPIAsync<T>([Required] string URL)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage = await _client.DeleteAsync(URL, _tokenSource.Token);
            return await ReturnApiResponse<T>(httpResponseMessage);
        }


        private static SemaphoreSlim Coordinator = new SemaphoreSlim(initialCount: 1, 1);
        private static bool IsWorking = true;
        private static async Task<T> ReturnApiResponse<T>(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string? jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync() ?? null;
                return JsonConvert.DeserializeObject<T>(jsonResponse);
            }


            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _localStorage.RemoveItemAsync("my-access-token");
                await _localStorage.RemoveItemAsync("my-refresh-token");
                throw new UnauthorizedException("");
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new ServerErrorException("Server-Error");
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new TooManyRequests("Too many request");
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Conflict)
            {
                string? jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync() ?? null;
                var response = JsonConvert.DeserializeObject<ResponseApi>(jsonResponse);

                throw new ConflictException(response.message);
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.BadGateway)
            {
                throw new DbConnectionException("connection-error");
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                string? jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync() ?? null;
                
                // Try to deserialize as ResponseApi
                try
                {
                    var response = JsonConvert.DeserializeObject<ResponseApi>(jsonResponse);
                    throw new BadRequestException(response?.message ?? jsonResponse ?? "Bad Request");
                }
                catch (JsonException)
                {
                    // If deserialization fails, throw the raw response
                    throw new BadRequestException(jsonResponse ?? "Bad Request - No details provided");
                }
            }

            return default;
        }
    }

    public class ResponseApi
    {
        public string message { get; set; }
        public int status { get; set; }
    }
}
