using InsightSage.Infrastructure.Examples;
using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Custom;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace InsightSage.Application.Services.AI
{
    public class AIProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AIProcessingService> _logger;
        private readonly ConcurrentQueue<AIProcessingRequest> _processingQueue = new();

        public AIProcessingService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AIProcessingService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Queue an AI processing request
        /// </summary>
        public void QueueProcessing(AIProcessingRequest request)
        {
            _processingQueue.Enqueue(request);
            _logger.LogInformation("Queued AI processing for session {SessionId}, message {MessageId}",
                request.SessionId, request.UserMessageId);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Processing Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_processingQueue.TryDequeue(out var request))
                    {
                        await ProcessAIRequestAsync(request, stoppingToken);
                    }
                    else
                    {
                        // No items in queue, wait briefly
                        await Task.Delay(100, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing AI requests");
                    await Task.Delay(1000, stoppingToken); // Wait before retrying
                }
            }

            _logger.LogInformation("AI Processing Service stopped");
        }

        private async Task ProcessAIRequestAsync(AIProcessingRequest request, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            try
            {
                var signalRService = scope.ServiceProvider.GetRequiredService<ISignalRNotificationService>();
                var chatMessageService = scope.ServiceProvider
                    .GetRequiredService<IChatMessageService<ChatMessageResponse<List<ChatMessage>>, ChatMessageResponse<ChatMessage>, ChatMessage>>();
                var multiPlatformAIService = scope.ServiceProvider.GetRequiredService<MultiPlatformAIService>();

                _logger.LogInformation("Processing AI request for session {SessionId}", request.SessionId);

                // Notify client that AI is processing
                await signalRService.NotifyAIProcessingStartedAsync(
                    request.UserId,
                    request.SessionId,
                    request.UserMessageId);

                // Get AI response
                var messages = new[]
                {
                    new { role = "user", content = request.UserMessage }
                };

                var aiResponse = await multiPlatformAIService.GetChatCompletionAsync(
                    platformSlug: request.PlatformSlug ?? "openai",
                    model: request.Model ?? "gpt-3.5-turbo",
                    messages: messages,
                    maxTokens: request.MaxTokens ?? 1000,
                    temperature: request.Temperature ?? 0.7
                );

                if (aiResponse?.Choices?.Count > 0)
                {
                    var aiMessageContent = aiResponse.Choices[0].Message.Content;

                    // Save AI response to database
                    var aiMessage = new ChatMessage
                    {
                        ChatSessionId = request.SessionId,
                        Role = "assistant",
                        Content = aiMessageContent
                    };

                    var saveResult = await chatMessageService.AddUpdateAsync(aiMessage);
                    if (saveResult.Result > 0)
                    {
                        // Send AI response to client via SignalR
                        var usage = new
                        {
                            PromptTokens = aiResponse.Usage?.PromptTokens ?? 0,
                            CompletionTokens = aiResponse.Usage?.CompletionTokens ?? 0,
                            TotalTokens = aiResponse.Usage?.TotalTokens ?? 0
                        };

                        await signalRService.SendAIResponseAsync(
                            request.UserId,
                            request.SessionId,
                            saveResult.Result,
                            aiMessageContent,
                            usage);

                        // Notify processing completed
                        await signalRService.NotifyAIProcessingCompletedAsync(
                            request.UserId,
                            request.SessionId,
                            saveResult.Result);

                        _logger.LogInformation("Successfully processed AI request for session {SessionId}", request.SessionId);
                    }
                    else
                    {
                        throw new Exception($"Failed to save AI response: {string.Join(", ", saveResult.Errors)}");
                    }
                }
                else
                {
                    throw new Exception("No response received from AI service");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI request for session {SessionId}", request.SessionId);

                // Notify client of error via SignalR
                using var errorScope = _serviceScopeFactory.CreateScope();
                var signalRService = errorScope.ServiceProvider.GetRequiredService<ISignalRNotificationService>();

                await signalRService.SendAIProcessingErrorAsync(
                    request.UserId,
                    request.SessionId,
                    request.UserMessageId,
                    "Failed to process AI request. Please try again.");
            }
        }
    }
}
