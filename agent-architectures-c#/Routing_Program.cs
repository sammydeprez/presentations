// ============================================================================
// Multi-Agent Routing Workflow with Microsoft Agent Framework
// ============================================================================
// This application demonstrates a routing pattern where:
// 1. A router agent analyzes incoming user queries
// 2. Routes the query to the appropriate specialized agent (Weather, Travel, or Unknown)
// 3. The selected agent processes the query and returns a response
//
// Key Concepts:
// - Switch-based routing: Router returns an enum, switch routes to appropriate executor
// - Shared state: Messages are stored in a shared scope so downstream agents can access them
// - SuperSteps: State updates are queued and published between execution steps
// ============================================================================

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.ComponentModel;

// ============================================================================
// Azure OpenAI Configuration
// ============================================================================

var openAiEndpoint = new Uri("##ENDPOINT##");
var openAiKey = "##API-KEY##";

var openAIClient = new AzureOpenAIClient(openAiEndpoint, new Azure.AzureKeyCredential(openAiKey));
var chatClient = openAIClient.GetChatClient("gpt-4.1-mini").AsIChatClient();

// ============================================================================
// Executor Initialization
// ============================================================================
// Create instances of all executors that will participate in the workflow

// Test the router
var routerExecutor = new RouterExecutor(chatClient);
var testMessage = new Microsoft.Extensions.AI.ChatMessage(
    Microsoft.Extensions.AI.ChatRole.User,
    "What can I do in New York?"
);
// var result = await routerExecutor.HandleAsync(testMessage, null!);
// Console.WriteLine($"Router selected: {result.Agent}");

// Test the weather agent
var weatherAgentExecutor = new WeatherAgentExecutor(chatClient);
testMessage = new Microsoft.Extensions.AI.ChatMessage(
    Microsoft.Extensions.AI.ChatRole.User,
    "What is the weather in New York?"
);
// var weatherResult = await weatherAgentExecutor.HandleAsync(testMessage, null!);
// Console.WriteLine($"Weather agent response: {weatherResult.Content[0].Text}");

// Test the travel advisor agent
var travelAdvisorExecutor = new TravelAdvisorExecutor(chatClient);
testMessage = new Microsoft.Extensions.AI.ChatMessage(
    Microsoft.Extensions.AI.ChatRole.User,
    "What are some must-see attractions in Paris?"
);
// var travelResult = await travelAdvisorExecutor.HandleAsync(testMessage, null!);
// Console.WriteLine($"Travel advisor agent response: {travelResult.Content[0].Text}");

// Test the unknown agent
var unknownExecutor = new UnknownExecutor();
testMessage = new Microsoft.Extensions.AI.ChatMessage(
    Microsoft.Extensions.AI.ChatRole.User,
    "Can you help me with my math homework?"
);
// var unknownResult = await unknownExecutor.HandleAsync(testMessage, null!);
// Console.WriteLine($"Unknown agent response: {unknownResult.Content[0].Text}");

// create a workflow that uses the router to select the appropriate agent based on user input
var workflowBuilder = new WorkflowBuilder(routerExecutor);
workflowBuilder
    .AddSwitch(routerExecutor, routerResult => routerResult
        .AddCase<RouterResult>(r => r.Agent == AgentType.Weather, weatherAgentExecutor)
        .AddCase<RouterResult>(r => r.Agent == AgentType.TravelAdvisor, travelAdvisorExecutor)
        .AddCase<RouterResult>(r => r.Agent == AgentType.Unknown, unknownExecutor))
    .WithOutputFrom(weatherAgentExecutor, travelAdvisorExecutor, unknownExecutor);

var workflow = workflowBuilder.Build();

// ============================================================================
// Workflow Visualization & Execution
// ============================================================================

// Display workflow structure as Mermaid diagram
Console.WriteLine("Workflow Mermaid Representation:");
Console.WriteLine("=====================================");
Console.WriteLine(workflow.ToMermaidString());

// Execute the workflow with input data
// The workflow will:
// 1. Pass messages to RouterExecutor
// 2. Router stores messages in shared state and returns RouterResult
// 3. Switch evaluates RouterResult and routes to appropriate executor
// 4. Selected executor retrieves messages from shared state and processes them
var inputMessages = new List<Microsoft.Extensions.AI.ChatMessage> 
{ 
    new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "Wie is sammy deprez") 
};

await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, inputMessages);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is WorkflowOutputEvent outputEvent)
    {
        if (evt.Data is AssistantChatMessage assistantChatMessage)
            Console.WriteLine($"Final Output: {assistantChatMessage.Content[0].Text}");
    }
}

enum AgentType
{
    [Description("Agent that can give travel advice")]
    TravelAdvisor,
    [Description("Agent that can provide weather information")]
    Weather,
    [Description("Agent for questions not covered by other agents")]
    Unknown
}

class RouterResult
{
        public AgentType Agent { get; set; }
}

class RouterExecutor : Executor<List<Microsoft.Extensions.AI.ChatMessage>, RouterResult>
{
    private readonly AIAgent _agent;

    public RouterExecutor(IChatClient client) : base("RouterExecutor")
    {
        JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(RouterResult));
        var chatOptions = new ChatOptions
        {
            ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema(schema)
        };

        _agent = client.CreateAIAgent(new ChatClientAgentOptions()
        {
            Instructions = "You are a router that directs user queries to the appropriate agent based on the content of the message.",
            ChatOptions = chatOptions
        });
    }

    /// <summary>
    /// Handles incoming messages by:
    /// 1. Storing messages in shared state (available to downstream executors in next SuperStep)
    /// 2. Querying AI to determine which agent should handle the request
    /// 3. Returning RouterResult with the selected agent type
    /// </summary>
    public override async ValueTask<RouterResult> HandleAsync(List<Microsoft.Extensions.AI.ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Router");
        
        // CRITICAL: Store messages in a shared state scope so downstream executors can access them
        // The scopeName "SharedMessages" allows cross-executor state sharing
        // State updates are queued here and published after this SuperStep completes
        await context.QueueStateUpdateAsync("messages", messages, scopeName: "SharedMessages", cancellationToken: cancellationToken);
        
        var response = await _agent.RunAsync(messages, cancellationToken: cancellationToken);
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
        };
        
        var routerResult = JsonSerializer.Deserialize<RouterResult>(response.Text, options);
        
        return routerResult ?? new RouterResult { Agent = AgentType.Unknown };
    }
}

// ============================================================================
// WeatherAgentExecutor
// ============================================================================
// Specialized executor for weather-related queries
// - Receives RouterResult from the switch
// - Retrieves original messages from shared state
// - Processes weather queries using AI with tool access

/// <summary>
/// Weather-specialized agent that handles weather information requests.
/// Has access to a GetWeather tool function for demonstration purposes.
/// </summary>
class WeatherAgentExecutor : Executor<RouterResult, AssistantChatMessage>
{
    private readonly AIAgent _agent;

    public WeatherAgentExecutor(IChatClient client) : base("WeatherAgentExecutor")
    {
        _agent = client.CreateAIAgent(instructions: "You are a helpful assistant, assisting with weather information", tools: [AIFunctionFactory.Create(GetWeather)]);
    }

    /// <summary>
    /// Handles weather queries by:
    /// 1. Retrieving original user messages from shared state
    /// 2. Processing the query with AI (including tool access)
    /// 3. Returning the AI response as AssistantChatMessage
    /// </summary>
    public override async ValueTask<AssistantChatMessage> HandleAsync(RouterResult routerResult, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("WeatherAgent");
        
        // CRITICAL: Retrieve messages from the shared state scope
        // Must use the same scopeName ("SharedMessages") as the router
        // Messages are now available because state was published after router's SuperStep
        var messages = await context.ReadStateAsync<List<Microsoft.Extensions.AI.ChatMessage>>("messages", scopeName: "SharedMessages", cancellationToken: cancellationToken);
        
        if (messages == null)
            throw new InvalidOperationException("Messages not found in shared state");
        
        var response = await _agent.RunAsync(messages, cancellationToken: cancellationToken);
        return new AssistantChatMessage(response.Text);
    }

    [Description("Get the weather for a given location.")]
    static string GetWeather([Description("The location to get the weather for.")] string location){
        return $"The weather in {location} is cloudy with a high of 15°C.";
    }
}

// ============================================================================
// TravelAdvisorExecutor
// ============================================================================
// Specialized executor for travel-related queries
// - Receives RouterResult from the switch
// - Retrieves original messages from shared state
// - Provides travel destination suggestions

/// <summary>
/// Travel-specialized agent that handles travel planning and destination recommendation requests.
/// </summary>
class TravelAdvisorExecutor : Executor<RouterResult, AssistantChatMessage>
{
    private readonly AIAgent _agent;

    public TravelAdvisorExecutor(IChatClient client) : base("TravelAdvisorExecutor")
    {
        _agent = client.CreateAIAgent(instructions: "You are a travel expert. Based on the user's input, suggest 1 travel destination.");
    }

    /// <summary>
    /// Handles travel queries by:
    /// 1. Retrieving original user messages from shared state
    /// 2. Processing the query with travel-specialized AI
    /// 3. Returning travel suggestions as AssistantChatMessage
    /// </summary>
    public override async ValueTask<AssistantChatMessage> HandleAsync(RouterResult routerResult, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("TravelAdvisor");
        
        // Retrieve messages from the shared state scope (same as WeatherAgent)
        var messages = await context.ReadStateAsync<List<Microsoft.Extensions.AI.ChatMessage>>("messages", scopeName: "SharedMessages", cancellationToken: cancellationToken);
        
        if (messages == null)
            throw new InvalidOperationException("Messages not found in shared state");
        
        var response = await _agent.RunAsync(messages, cancellationToken: cancellationToken);
        return new AssistantChatMessage(response.Text);
    }
}

// ============================================================================
// UnknownExecutor
// ============================================================================
// Fallback executor for queries that don't match any specialized agent
// Returns a generic message without using AI

/// <summary>
/// Fallback executor that handles queries that don't match weather or travel categories.
/// Returns a simple static response without AI processing.
/// </summary>
class UnknownExecutor : Executor<RouterResult, AssistantChatMessage>
{
    public UnknownExecutor() : base("UnknownExecutor")
    {
    }
    
    /// <summary>
    /// Returns a generic message for unsupported query types.
    /// Note: Does not need to retrieve messages from state since it returns a static response.
    /// </summary>
    public override async ValueTask<AssistantChatMessage> HandleAsync(RouterResult routerResult, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("UnknownExecutor");
        return new AssistantChatMessage("I can not assist with that request. Could you please rephrase or ask something else? I can help you with travel advice or weather information.");
    }
}
