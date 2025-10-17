using System;
using System.ClientModel;
using System.Text.Json;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;

namespace TrainDashboard.Services;

/// <summary>
/// AI Provider type
/// </summary>
public enum AIProvider
{
    Groq,
    OpenRouter
}

/// <summary>
/// Service for parsing natural language queries into structured journey information
/// Supports OpenAI, Groq (free!), and OpenRouter
/// </summary>
public class NaturalLanguageQueryService
{
    private readonly string? _apiKey;
    private readonly ChatClient? _chatClient;
    private readonly bool _isEnabled;
    private readonly AIProvider _provider;
    private readonly string _modelName = "gpt-4o-mini"; // Default model

    public NaturalLanguageQueryService(string? apiKey, string? providerName = null)
    {
        _apiKey = apiKey;
        _isEnabled = !string.IsNullOrEmpty(apiKey);
        
        // Determine provider from environment or default to OpenAI
        _provider = DetermineProvider(providerName);
        
        if (_isEnabled && !string.IsNullOrEmpty(_apiKey))
        {
            try
            {
                // Configure based on provider
                switch (_provider)
                {
                    case AIProvider.Groq:
                        // Groq: Fast and FREE! https://console.groq.com
                        // Using llama-3.3-70b-versatile (latest stable free model)
                        _modelName = "llama-3.3-70b-versatile";
                        var groqClient = new OpenAIClient(new ApiKeyCredential(_apiKey!), 
                            new OpenAIClientOptions { Endpoint = new Uri("https://api.groq.com/openai/v1") });
                        _chatClient = groqClient.GetChatClient(_modelName);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\nUsing Groq AI ({_modelName})");
                        Console.ResetColor();
                        break;
                        
                    case AIProvider.OpenRouter:
                        // OpenRouter: Many free models! https://openrouter.ai
                        // Using llama 3.1 8B (free tier, stable)
                        _modelName = "meta-llama/llama-3.1-8b-instruct:free";
                        var openRouterClient = new OpenAIClient(new ApiKeyCredential(_apiKey!),
                            new OpenAIClientOptions { Endpoint = new Uri("https://openrouter.ai/api/v1") });
                        _chatClient = openRouterClient.GetChatClient(_modelName);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nUsing OpenRouter ({_modelName})");
                        Console.ResetColor();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not initialize AI client: {ex.Message}");
                _isEnabled = false;
            }
        }
    }
    
    private AIProvider DetermineProvider(string? providerName)
    {
        if (string.IsNullOrEmpty(providerName))
            providerName = Environment.GetEnvironmentVariable("AI_PROVIDER");
            
        if (string.IsNullOrEmpty(providerName))
            return AIProvider.Groq; // Default to FREE Groq!
            
        return providerName.ToLower() switch
        {
            "groq" => AIProvider.Groq,
            "openrouter" => AIProvider.OpenRouter,
            _ => AIProvider.Groq // Default to Groq
        };
    }

    /// <summary>
    /// Checks if the AI service is available
    /// </summary>
    public bool IsEnabled => _isEnabled;

    /// <summary>
    /// Parses a natural language query into a structured JourneyQuery
    /// </summary>
    public async Task<JourneyQuery?> ParseQueryAsync(string userInput)
    {
        if (!_isEnabled || _chatClient == null)
        {
            return null;
        }

        try
        {
            var systemPrompt = @"You are a UK train journey query parser. Extract structured information from natural language queries.

IMPORTANT: Output ONLY valid JSON, no explanations or markdown.

Extract these fields when present:
- departureStation: Station name or code (required)
- destinationStation: Station name or code (optional)
- preferredDepartureTime: Time in HH:mm format (optional)
- preferredArrivalTime: Time in HH:mm format (optional)
- isDeparture: true for departures, false for arrivals (default: true)
- notes: Any additional context (optional)

Examples:
Input: ""I need to get from Paddington to Bristol around 3pm""
Output: {""departureStation"":""Paddington"",""destinationStation"":""Bristol"",""preferredDepartureTime"":""15:00"",""isDeparture"":true}

Input: ""Next train to Manchester""
Output: {""departureStation"":null,""destinationStation"":""Manchester"",""isDeparture"":true}

Input: ""Trains from Kings Cross to Edinburgh leaving after 2pm""
Output: {""departureStation"":""Kings Cross"",""destinationStation"":""Edinburgh"",""preferredDepartureTime"":""14:00"",""isDeparture"":true}

Input: ""Show me London Euston departures""
Output: {""departureStation"":""London Euston"",""isDeparture"":true}

Input: ""What trains arrive at Birmingham from London around 5pm""
Output: {""destinationStation"":""Birmingham"",""departureStation"":""London"",""preferredArrivalTime"":""17:00"",""isDeparture"":false}

Now parse this query and return ONLY the JSON:";

            var messages = new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userInput)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text.Trim();

            // Remove markdown code blocks if present
            if (content.StartsWith("```json"))
            {
                content = content.Substring(7);
            }
            if (content.StartsWith("```"))
            {
                content = content.Substring(3);
            }
            if (content.EndsWith("```"))
            {
                content = content.Substring(0, content.Length - 3);
            }
            content = content.Trim();

            // Parse the JSON response
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsedData = JsonSerializer.Deserialize<JsonElement>(content);
            
            var query = new JourneyQuery
            {
                Confidence = 0.9 // High confidence for AI parsing
            };

            // Extract departure station
            if (parsedData.TryGetProperty("departureStation", out var depStation))
            {
                var depValue = depStation.GetString();
                if (!string.IsNullOrEmpty(depValue))
                {
                    query.DepartureStation = depValue;
                }
            }

            // Extract destination station
            if (parsedData.TryGetProperty("destinationStation", out var destStation))
            {
                var destValue = destStation.GetString();
                if (!string.IsNullOrEmpty(destValue))
                {
                    query.DestinationStation = destValue;
                }
            }

            // Extract preferred departure time
            if (parsedData.TryGetProperty("preferredDepartureTime", out var depTime))
            {
                var timeStr = depTime.GetString();
                if (!string.IsNullOrEmpty(timeStr) && TimeSpan.TryParse(timeStr, out var parsedTime))
                {
                    query.PreferredDepartureTime = parsedTime;
                }
            }

            // Extract preferred arrival time
            if (parsedData.TryGetProperty("preferredArrivalTime", out var arrTime))
            {
                var timeStr = arrTime.GetString();
                if (!string.IsNullOrEmpty(timeStr) && TimeSpan.TryParse(timeStr, out var parsedTime))
                {
                    query.PreferredArrivalTime = parsedTime;
                }
            }

            // Extract isDeparture flag
            if (parsedData.TryGetProperty("isDeparture", out var isDep))
            {
                query.IsDeparture = isDep.GetBoolean();
            }

            // Extract notes
            if (parsedData.TryGetProperty("notes", out var notes))
            {
                query.Notes = notes.GetString();
            }

            return query;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing query with AI: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Provides a more conversational response to user queries
    /// </summary>
    public async Task<string?> GetConversationalResponseAsync(string userInput, JourneyQuery? parsedQuery)
    {
        if (!_isEnabled || _chatClient == null)
        {
            return null;
        }

        try
        {
            var context = parsedQuery != null 
                ? $"The user asked: '{userInput}'. We understood: From {parsedQuery.DepartureStation ?? "current location"} to {parsedQuery.DestinationStation ?? "anywhere"}."
                : $"The user asked: '{userInput}'. We couldn't parse this query.";

            var systemPrompt = $@"You are a helpful UK train journey assistant. Be brief and friendly.
{context}
Provide a short, natural confirmation (1 sentence max) of what you understood.";

            var messages = new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userInput)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            return response.Value.Content[0].Text.Trim();
        }
        catch
        {
            return null;
        }
    }
}
