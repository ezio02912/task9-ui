using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace BootstrapBlazor.Server.Services.MindsDB
{
    /// <summary>
    /// MindsDB Chat Message model for streaming response
    /// </summary>
    public class MindsDBChatMessage
    {
        /// <summary>
        /// Content of the chat message
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// MindsDB API request model
    /// </summary>
    public class MindsDBChatRequest
    {
        /// <summary>
        /// List of messages in the conversation
        /// </summary>
        public List<MindsDBRequestMessage> Messages { get; set; } = [];
    }

    /// <summary>
    /// MindsDB request message model (simplified format)
    /// </summary>
    public class MindsDBRequestMessage
    {
        /// <summary>
        /// User question
        /// </summary>
        public string Question { get; set; } = string.Empty;
        
        /// <summary>
        /// AI answer (empty for request)
        /// </summary>
        public string Answer { get; set; } = string.Empty;
    }

    /// <summary>
    /// MindsDB response message model
    /// </summary>
    public class MindsDBMessage
    {
        /// <summary>
        /// Content of the message
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// MindsDB streaming response model
    /// </summary>
    public class MindsDBStreamResponse
    {
        /// <summary>
        /// Type of the response
        /// </summary>
        public string? Type { get; set; }
        
        /// <summary>
        /// Final output from AI
        /// </summary>
        public string? Output { get; set; }
        
        /// <summary>
        /// Content of the response
        /// </summary>
        public string? Content { get; set; }
        
        /// <summary>
        /// Actions being performed
        /// </summary>
        public List<MindsDBAction>? Actions { get; set; }
        
        /// <summary>
        /// Steps completed
        /// </summary>
        public List<MindsDBStep>? Steps { get; set; }
        
        /// <summary>
        /// Messages in the response
        /// </summary>
        public List<MindsDBMessage>? Messages { get; set; }
        
        /// <summary>
        /// Original prompt
        /// </summary>
        public string? Prompt { get; set; }
        
        /// <summary>
        /// Trace ID for debugging
        /// </summary>
        public string? Trace_Id { get; set; }
    }

    /// <summary>
    /// MindsDB action model
    /// </summary>
    public class MindsDBAction
    {
        /// <summary>
        /// Tool being used
        /// </summary>
        public string? Tool { get; set; }
        
        /// <summary>
        /// Input for the tool
        /// </summary>
        public string? Tool_Input { get; set; }
        
        /// <summary>
        /// Log message
        /// </summary>
        public string? Log { get; set; }
    }

    /// <summary>
    /// MindsDB step model
    /// </summary>
    public class MindsDBStep
    {
        /// <summary>
        /// Action performed in this step
        /// </summary>
        public MindsDBAction? Action { get; set; }
        
        /// <summary>
        /// Observation from the step
        /// </summary>
        public string? Observation { get; set; }
    }

    /// <summary>
    /// Interface for MindsDB service
    /// </summary>
    public interface IMindsDBService
    {
        /// <summary>
        /// Get chat completions streaming from MindsDB API
        /// </summary>
        /// <param name="prompt">User input prompt</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of chat messages</returns>
        IAsyncEnumerable<MindsDBChatMessage> GetChatCompletionsStreamingAsync(string prompt, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// MindsDB service implementation for streaming chat completions
    /// </summary>
    public class MindsDBService : IMindsDBService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MindsDBService> _logger;

        /// <summary>
        /// Constructor for MindsDBService
        /// </summary>
        /// <param name="httpClient">HTTP client for API calls</param>
        /// <param name="configuration">Configuration for API settings</param>
        /// <param name="logger">Logger for error handling</param>
        public MindsDBService(HttpClient httpClient, IConfiguration configuration, ILogger<MindsDBService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get chat completions streaming from MindsDB API
        /// </summary>
        /// <param name="prompt">User input prompt</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of chat messages</returns>
        public async IAsyncEnumerable<MindsDBChatMessage> GetChatCompletionsStreamingAsync(
            string prompt,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var apiUrl = _configuration["RemoteServices:MCPSeoURLApi"];
            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("MCPSeoURLApi configuration is missing");
                yield break;
            }

            var requestData = new MindsDBChatRequest
            {
                Messages = new List<MindsDBRequestMessage>
                {
                    new MindsDBRequestMessage
                    {
                        Question = prompt,
                        Answer = ""
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Use HttpCompletionOption.ResponseHeadersRead for true streaming
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = content
            };
            using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("MindsDB API request failed with status code: {StatusCode}", response.StatusCode);
                yield break;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, bufferSize: 128); // Very small buffer for faster streaming

            bool hasStarted = false;
            bool hasReceivedFinalOutput = false;

            string? line;
            while ((line = await reader.ReadLineAsync()) != null && !cancellationToken.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                    continue;

                var jsonData = line.Substring(6); // Remove "data: " prefix
                
                if (jsonData.Trim() == "[DONE]")
                    break;

                MindsDBStreamResponse? streamResponse = null;
                try
                {
                    streamResponse = JsonSerializer.Deserialize<MindsDBStreamResponse>(jsonData, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning("Failed to parse streaming response: {Error} - Data: {Data}", ex.Message, jsonData);
                    continue;
                }

                if (streamResponse != null)
                {
                    // Log for debugging streaming speed
                    _logger.LogInformation("Received stream data: Type={Type}, HasOutput={HasOutput}, HasActions={HasActions}, HasSteps={HasSteps}", 
                        streamResponse.Type, 
                        !string.IsNullOrEmpty(streamResponse.Output),
                        streamResponse.Actions?.Count > 0,
                        streamResponse.Steps?.Count > 0);

                    // Handle different message types based on MindsDB format
                    if (streamResponse.Type == "start")
                    {
                        hasStarted = true;
                        yield return new MindsDBChatMessage { Content = "🤖 Đang phân tích câu hỏi..." };
                    }
                    else if (streamResponse.Type == "end")
                    {
                        if (!hasReceivedFinalOutput)
                        {
                            yield return new MindsDBChatMessage { Content = "✅ Phân tích hoàn tất nhưng không có kết quả." };
                        }
                        break;
                    }
                    else if (!string.IsNullOrEmpty(streamResponse.Output))
                    {
                        // This is the final AI answer
                        hasReceivedFinalOutput = true;
                        yield return new MindsDBChatMessage { Content = "CLEAR_PREVIOUS" }; // Clear progress first
                        yield return new MindsDBChatMessage { Content = streamResponse.Output };
                    }
                    else if (streamResponse.Actions != null && streamResponse.Actions.Count > 0)
                    {
                        // AI is performing actions (SQL queries)
                        var action = streamResponse.Actions[0];
                        if (action.Tool == "sql_db_query")
                        {
                            yield return new MindsDBChatMessage { Content = "📊 Đang truy vấn cơ sở dữ liệu..." };
                        }
                        else
                        {
                            yield return new MindsDBChatMessage { Content = "⚙️ Đang xử lý dữ liệu..." };
                        }
                    }
                    else if (streamResponse.Steps != null && streamResponse.Steps.Count > 0)
                    {
                        // AI has received results from actions
                        yield return new MindsDBChatMessage { Content = "🧠 Đang phân tích kết quả..." };
                    }
                    else if (hasStarted && !hasReceivedFinalOutput)
                    {
                        // Default progress message for any other activity
                        yield return new MindsDBChatMessage { Content = "💭 Đang suy nghĩ..." };
                    }
                }
            }
        }
    }
}