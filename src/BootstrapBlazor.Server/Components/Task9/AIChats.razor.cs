
using System.Collections.Concurrent;
using BootstrapBlazor.Server.Services.MindsDB;
using Microsoft.AspNetCore.Components.Authorization;
using BootstrapBlazor.Server.Identity;
using Microsoft.Extensions.DependencyInjection;
namespace BootstrapBlazor.Server.Components.Task9;


public partial class AIChats
{
    [Inject]
    [NotNull]
    private IAIChatService? AIChatService { get; set; }

    [Inject]
    [NotNull]
    private IMindsDBService? MindsDBService { get; set; }

    [Inject]
    [NotNull]
    private IBrowserFingerService? BrowserFingerService { get; set; }

    [Inject, NotNull]
    private IServiceProvider? ServiceProvider { get; set; }
   

    [Inject]
    [NotNull]
    private ILogger<AIChats>? _logger { get; set; }

    private string? Context { get; set; }

    private List<AIChatResponseDto> Messages { get; set; } = [];

    private static string? GetStackClass(AIChatResponseDto message) => CssBuilder.Default("msg-stack")
        .AddClass("msg-stack-assistant", message.answer != "question")
        .Build();

    /// <summary>
    /// Check if message content is a progress message (not final output)
    /// </summary>
    /// <param name="content">Message content</param>
    /// <returns>True if it's a progress message</returns>
    private static bool IsProgressMessage(string content)
    {
        return content.StartsWith("🤖") || 
               content.StartsWith("📊") || 
               content.StartsWith("🧠") || 
               content.StartsWith("💭") || 
               content.StartsWith("⚙️") ||
               content.StartsWith("✅") ||
               content.Contains("Đang suy nghĩ") ||
               content.Contains("Đang phân tích") ||
               content.Contains("Đang truy vấn") ||
               content.Contains("Đang xử lý");
    }

    private const int TotalCount = 50;
    private static readonly ConcurrentDictionary<string, int> Cache = new();
    private string? _code, _userId;
    private int _currentCount;
    private bool _isDisabled;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var provider = ServiceProvider.GetService<AuthenticationStateProvider>();
        _userId = await ((ApiAuthenticationStateProvider)provider!).GetCurrentUserId();
        _code = await GetFingerCodeAsync();
        _currentCount = Cache.GetOrAdd(_code, key => TotalCount);
        _isDisabled = _currentCount < 1;
        
        // Load old messages on initialization
        await GetOldMessagesAsync();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            // Don't auto-scroll when loading old messages on first render, just focus input
            await InvokeVoidAsync("scroll", Id, false);
        }
        // Don't interfere with scroll on subsequent renders
    }



    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override Task InvokeInitAsync() => InvokeVoidAsync("init", Id);

    private async Task GetOldMessagesAsync()
    {
        var result = await AIChatService.GetListAsync(new AIChatFilterDto()
        {
            UserId = int.Parse(_userId ?? "1")
        });
        if(result.IsSuccess && result.Data != null)
        {
            // Sort messages by creation time (oldest first)
            Messages = result.Data.OrderBy(m => m.createdAt).ToList();
            StateHasChanged();
        }
    }

    private async Task GetCompletionsAsync()
    {
        Context = Context?.TrimEnd('\n') ?? string.Empty;
        if (!string.IsNullOrEmpty(Context))
        {
            var context = Context;
            Context = string.Empty;

            // 1. Thêm câu hỏi của user trước
            var userMsg = new AIChatResponseDto()
            {
                id = Guid.NewGuid().ToString(),
                question = context,
                answer = "question", // Special marker for user message
                createdAt = DateTime.Now
            };
            Messages.Add(userMsg);
            
            // 2. Thêm message cho AI response
            var aiMsg = new AIChatResponseDto()
            {
                id = Guid.NewGuid().ToString(),
                question = context, 
                answer = "Đang suy nghĩ ...",
                createdAt = DateTime.Now
            };
            Messages.Add(aiMsg);
            StateHasChanged();
            
            // Chờ một chút để DOM render xong, sau đó scroll xuống cuối
            await Task.Delay(50);
            await InvokeVoidAsync("scrollToBottom", Id);

            bool first = true;
            bool isCompleted = false;
            var resultCreateFirst = await AIChatService.CreateAsync(new CreateAIChatDto()
            {
                UserId = int.Parse(_userId ?? "1"),
                Question = userMsg.question,
                Answer = "question"
            });
            if (resultCreateFirst.IsSuccess)
            {
                _logger?.LogInformation("Successfully saved chat conversation to database");
            }
            else
            {
                _logger?.LogError("Failed to save chat conversation to database");
            }
            await foreach (var chatMessage in MindsDBService.GetChatCompletionsStreamingAsync(context))
            {
                if (first)
                {
                    first = false;
                    aiMsg.answer = string.Empty;
                }

                // Remove delay for faster streaming
                // await Task.Delay(50);
                if (!string.IsNullOrEmpty(chatMessage.Content))
                {
                    // Handle special signals from MindsDB
                    if (chatMessage.Content == "CLEAR_PREVIOUS")
                    {
                        aiMsg.answer = string.Empty; // Clear previous content
                        StateHasChanged();
                    }
                    else
                    {
                        // For progress messages - replace content for real-time update
                        if (chatMessage.Content.StartsWith("🤖") || 
                            chatMessage.Content.StartsWith("📊") || 
                            chatMessage.Content.StartsWith("🧠") || 
                            chatMessage.Content.StartsWith("💭") || 
                            chatMessage.Content.StartsWith("⚙️") ||
                            chatMessage.Content.StartsWith("✅"))
                        {
                            aiMsg.answer = chatMessage.Content; // Replace progress message immediately
                            StateHasChanged();
                            
                            // Apply jumping effect to progress messages
                            await InvokeVoidAsync("applyJumpingEffect");
                            await InvokeVoidAsync("scrollToBottom", Id);
                        }
                        else
                        {
                            // This is the final output - append it
                            aiMsg.answer += chatMessage.Content;
                            isCompleted = true; // Mark as completed when we get final content
                            StateHasChanged();
                            await InvokeVoidAsync("scrollToBottom", Id);
                        }
                    }
                }
            }

            // Save the conversation to database after completion
            if (isCompleted && !string.IsNullOrWhiteSpace(aiMsg.answer) && 
                !aiMsg.answer.StartsWith("🤖") && !aiMsg.answer.StartsWith("📊") && 
                !aiMsg.answer.StartsWith("🧠") && !aiMsg.answer.StartsWith("💭") && 
                !aiMsg.answer.StartsWith("⚙️") && !aiMsg.answer.StartsWith("✅"))
            {
                try
                {
                    var createDto = new CreateAIChatDto
                    {
                        UserId = int.Parse(_userId ?? "1"),
                        Question = aiMsg.question,
                        Answer = aiMsg.answer
                    };

                    var result = await AIChatService.CreateAsync(createDto);
                    if (result.IsSuccess)
                    {
                        _logger?.LogInformation("Successfully saved chat conversation to database");
                    }
                    else
                    {
                        _logger?.LogError("Failed to save chat conversation to database");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to save chat conversation to database");
                    // Don't show error to user, just log it
                }
            }
           
            _ = Task.Run(async () =>
            {
                await Task.Delay(100);
                if (!string.IsNullOrEmpty(_code))
                {
                    _currentCount = Cache.AddOrUpdate(_code, key => TotalCount, (key, number) => number - 1);
                    _isDisabled = _currentCount < 1;
                }
                else
                {
                    _isDisabled = true;
                }
                await InvokeAsync(StateHasChanged);
            });
        }
    }

    private async Task<string> GetFingerCodeAsync()
    {
        var code = await BrowserFingerService.GetFingerCodeAsync();
        code ??= "BootstrapBlazor";
        return code;
    }

    private void CreateNewTopic()
    {
        Context = null;
        Messages.Clear();
        StateHasChanged();
    }

}
