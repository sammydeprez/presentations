// ============================================================================
// Travel Advice Reflection Workflow with Microsoft Agent Framework
// ============================================================================
// This application demonstrates a reflection pattern where:
// 1. A Travel Advisor agent provides travel recommendations
// 2. A Reviewer agent checks if the advice covers 3 topics: history, sports, culture
// 3. If not all topics are covered, the reviewer sends feedback to the advisor
// 4. The advisor revises the response until all 3 topics are discussed
//
// Key Concepts:
// - Reflection pattern: Content is iteratively improved based on feedback
// - Conditional routing: Switch routes based on reviewer's approval status
// - Shared state: Iteration tracking and conversation history across executors
// - Structured output: Reviewer returns typed decision with specific feedback
// ============================================================================

using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

// ============================================================================
// Azure OpenAI Configuration
// ============================================================================

var openAiEndpoint = new Uri("##ENDPOINT##");
var openAiKey = "##API-KEY##";

var openAIClient = new AzureOpenAIClient(openAiEndpoint, new Azure.AzureKeyCredential(openAiKey));
var chatClient = openAIClient.GetChatClient("gpt-4.1-mini").AsIChatClient();

// ============================================================================
// Configuration
// ============================================================================

const int MaxIterations = 5;

// Define the topics that must be covered in travel advice
string[] requiredTopics = ["history", "sports", "culture"];

// ============================================================================
// Executor Initialization
// ============================================================================

var travelAdvisorExecutor = new TravelAdvisorExecutor(chatClient);
var topicReviewerExecutor = new TopicReviewerExecutor(chatClient, requiredTopics, MaxIterations);
var summaryExecutor = new SummaryExecutor(chatClient, requiredTopics);

// ============================================================================
// Workflow Construction
// ============================================================================
// Build workflow with reflection loop:
// TravelAdvisor -> TopicReviewer -> (if approved) Summary
//                                -> (if not approved) TravelAdvisor (loop back)

var workflowBuilder = new WorkflowBuilder(travelAdvisorExecutor)
    .AddEdge(travelAdvisorExecutor, topicReviewerExecutor)
    .AddSwitch(topicReviewerExecutor, sw => sw
        .AddCase<ReviewDecision>(rd => rd.Approved == true, summaryExecutor)
        .AddCase<ReviewDecision>(rd => rd.Approved == false, travelAdvisorExecutor))
    .WithOutputFrom(summaryExecutor);

var workflow = workflowBuilder.Build();

// ============================================================================
// Workflow Visualization & Execution
// ============================================================================

Console.WriteLine("\n=== Travel Advice Reflection Workflow ===\n");
Console.WriteLine($"The Travel Advisor and Topic Reviewer will iterate up to {MaxIterations} times");
Console.WriteLine($"until all required topics are covered: {string.Join(", ", requiredTopics)}\n");

Console.WriteLine("Workflow Mermaid Representation:");
Console.WriteLine("=====================================");
Console.WriteLine(workflow.ToMermaidString());
Console.WriteLine("=====================================\n");

// Execute the workflow with a destination query
const string UserQuery = "Give me travel advice for visiting Rome, Italy.";

Console.WriteLine(new string('=', 80));
Console.WriteLine($"USER QUERY: {UserQuery}");
Console.WriteLine(new string('=', 80) + "\n");

await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, UserQuery);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is WorkflowOutputEvent outputEvent)
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ FINAL APPROVED TRAVEL ADVICE");
        Console.ResetColor();
        Console.WriteLine(new string('=', 80));
        Console.WriteLine();
        Console.WriteLine(outputEvent.Data);
        Console.WriteLine();
        Console.WriteLine(new string('=', 80));
    }
}

Console.WriteLine("\n✅ Sample Complete: Travel Advice Reflection demonstrates iterative content improvement\n");
Console.WriteLine("Key Concepts Demonstrated:");
Console.WriteLine("  ✓ Reflection pattern with feedback loops");
Console.WriteLine($"  ✓ Dynamic topic coverage validation ({string.Join(", ", requiredTopics)})");
Console.WriteLine($"  ✓ Max iteration cap ({MaxIterations}) for safety");
Console.WriteLine("  ✓ Multiple message handlers for initial and revision requests");
Console.WriteLine("  ✓ Structured output for reliable topic detection\n");

// ============================================================================
// Shared State for Iteration Tracking
// ============================================================================

internal sealed class FlowState
{
    public int Iteration { get; set; } = 1;
    public List<ChatMessage> History { get; } = [];
}

internal static class FlowStateShared
{
    public const string Scope = "FlowStateScope";
    public const string Key = "singleton";
}

internal static class FlowStateHelpers
{
    public static async Task<FlowState> ReadFlowStateAsync(IWorkflowContext context)
    {
        FlowState? state = await context.ReadStateAsync<FlowState>(FlowStateShared.Key, scopeName: FlowStateShared.Scope);
        return state ?? new FlowState();
    }

    public static ValueTask SaveFlowStateAsync(IWorkflowContext context, FlowState state)
        => context.QueueStateUpdateAsync(FlowStateShared.Key, state, scopeName: FlowStateShared.Scope);
}

// ============================================================================
// Data Transfer Objects
// ============================================================================

[Description("Topic review decision including approval status and missing topics feedback")]
internal sealed class ReviewDecision
{
    [JsonPropertyName("approved")]
    [Description("Whether all required topics are adequately covered")]
    public bool Approved { get; set; }

    [JsonPropertyName("coveredTopics")]
    [Description("List of topic names that are adequately covered in the response")]
    public List<string> CoveredTopics { get; set; } = [];

    [JsonPropertyName("missingTopics")]
    [Description("List of topic names that are missing or not adequately covered")]
    public List<string> MissingTopics { get; set; } = [];

    [JsonPropertyName("feedback")]
    [Description("Specific feedback about which topics are missing and suggestions for content")]
    public string Feedback { get; set; } = "";

    // Non-JSON properties for workflow use
    [JsonIgnore]
    public string Content { get; set; } = "";

    [JsonIgnore]
    public int Iteration { get; set; }
}

// ============================================================================
// Custom Executors
// ============================================================================

/// <summary>
/// Executor that provides travel advice based on user queries.
/// Handles both initial requests and revision requests with feedback.
/// </summary>
internal sealed class TravelAdvisorExecutor : Executor
{
    private readonly AIAgent _agent;

    public TravelAdvisorExecutor(IChatClient chatClient) : base("TravelAdvisor")
    {
        
        _agent = new ChatClientAgent(
            chatClient,
            name: "TravelAdvisor",
            instructions: $"""
                You are an expert travel advisor who provides comprehensive travel recommendations..
                Be thorough but concise. Make your advice engaging and practical.
                """
        );
    }

    protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder) =>
        routeBuilder
            .AddHandler<string, ChatMessage>(HandleInitialRequestAsync)
            .AddHandler<ReviewDecision, ChatMessage>(HandleRevisionRequestAsync);

    /// <summary>
    /// Handles the initial travel advice request from the user.
    /// </summary>
    private async ValueTask<ChatMessage> HandleInitialRequestAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        return await HandleAsyncCoreAsync(new ChatMessage(ChatRole.User, message), context, cancellationToken);
    }

    /// <summary>
    /// Handles revision requests from the reviewer with feedback about missing topics.
    /// </summary>
    private async ValueTask<ChatMessage> HandleRevisionRequestAsync(
        ReviewDecision decision,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        string prompt = $"""
            Please revise and expand your travel advice based on this feedback:

            FEEDBACK: {decision.Feedback}

            ORIGINAL RESPONSE:
            {decision.Content}

            Please provide an updated response that addresses the missing topics while 
            keeping the valuable information from your original advice.
            """;

        return await HandleAsyncCoreAsync(new ChatMessage(ChatRole.User, prompt), context, cancellationToken);
    }

    /// <summary>
    /// Core implementation for generating travel advice (initial or revised).
    /// </summary>
    private async Task<ChatMessage> HandleAsyncCoreAsync(
        ChatMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        FlowState state = await FlowStateHelpers.ReadFlowStateAsync(context);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n=== Travel Advisor (Iteration {state.Iteration}) ===\n");
        Console.ResetColor();

        StringBuilder sb = new();
        await foreach (AgentRunResponseUpdate update in _agent.RunStreamingAsync(message, cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                sb.Append(update.Text);
                Console.Write(update.Text);
            }
        }
        Console.WriteLine("\n");

        string text = sb.ToString();
        state.History.Add(new ChatMessage(ChatRole.Assistant, text));
        await FlowStateHelpers.SaveFlowStateAsync(context, state);

        return new ChatMessage(ChatRole.User, text);
    }
}

/// <summary>
/// Executor that reviews travel advice to ensure it covers all required topics.
/// Uses structured output for reliable detection.
/// </summary>
internal sealed class TopicReviewerExecutor : Executor<ChatMessage, ReviewDecision>
{
    private readonly AIAgent _agent;
    private readonly string[] _requiredTopics;
    private readonly int _maxIterations;

    public TopicReviewerExecutor(IChatClient chatClient, string[] requiredTopics, int maxIterations) : base("TopicReviewer")
    {
        _requiredTopics = requiredTopics;
        _maxIterations = maxIterations;
        
        string topicsList = string.Join(", ", requiredTopics);
        string numberedTopics = string.Join("\n", requiredTopics.Select((t, i) => $"{i + 1}. {t.ToUpper()}: Content related to {t}"));
        
        _agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "TopicReviewer",
            Instructions = $"""
                You are a travel content reviewer. Your job is to analyze travel advice and determine
                if it adequately covers ALL of these required topics: {topicsList}
                
                Topics to check:
                {numberedTopics}
                
                Analyze the content carefully. A topic is considered "covered" if there's meaningful
                discussion about it, not just a brief mention.
                
                In your response:
                - List all topics that ARE adequately covered in "coveredTopics"
                - List all topics that are MISSING or inadequate in "missingTopics"
                - Set approved=true ONLY if all required topics ({topicsList}) are covered
                - If any topic is missing, set approved=false and provide specific feedback
                
                The topic names in coveredTopics and missingTopics should match exactly: {topicsList}
                """,
            ChatOptions = new()
            {
                ResponseFormat = ChatResponseFormat.ForJsonSchema<ReviewDecision>()
            }
        });
    }

    public override async ValueTask<ReviewDecision> HandleAsync(
        ChatMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        FlowState state = await FlowStateHelpers.ReadFlowStateAsync(context);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"=== Topic Reviewer (Iteration {state.Iteration}) ===\n");
        Console.ResetColor();

        // Use RunStreamingAsync to get streaming updates, then deserialize at the end
        IAsyncEnumerable<AgentRunResponseUpdate> updates = _agent.RunStreamingAsync(message, cancellationToken: cancellationToken);

        // Stream the output in real-time (for any rationale/explanation)
        await foreach (AgentRunResponseUpdate update in updates)
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                Console.Write(update.Text);
            }
        }
        Console.WriteLine("\n");

        // Convert the stream to a response and deserialize the structured output
        AgentRunResponse response = await updates.ToAgentRunResponseAsync(cancellationToken);
        ReviewDecision decision = response.Deserialize<ReviewDecision>(JsonSerializerOptions.Web);

        // Display the review results
        Console.WriteLine("Topic Coverage:");
        foreach (var topic in _requiredTopics)
        {
            bool isCovered = decision.CoveredTopics.Contains(topic, StringComparer.OrdinalIgnoreCase);
            Console.WriteLine($"  • {topic}: {(isCovered ? "✅" : "❌")}");
        }
        Console.WriteLine();
        Console.WriteLine($"Decision: {(decision.Approved ? "✅ APPROVED - All topics covered!" : "❌ NEEDS REVISION")}");
        
        if (!string.IsNullOrEmpty(decision.Feedback))
        {
            Console.WriteLine($"Feedback: {decision.Feedback}");
        }
        Console.WriteLine();

        // Safety: approve if max iterations reached
        if (!decision.Approved && state.Iteration >= _maxIterations)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️ Max iterations ({_maxIterations}) reached - auto-approving");
            Console.ResetColor();
            decision.Approved = true;
            decision.Feedback = "";
        }

        // Increment iteration ONLY if rejecting (will loop back to TravelAdvisor)
        if (!decision.Approved)
        {
            state.Iteration++;
        }

        // Store the decision in history
        state.History.Add(new ChatMessage(ChatRole.Assistant,
            $"[Review: {(decision.Approved ? "Approved" : "Needs Revision")}] {decision.Feedback}"));
        await FlowStateHelpers.SaveFlowStateAsync(context, state);

        // Populate workflow-specific fields
        decision.Content = message.Text ?? "";
        decision.Iteration = state.Iteration;

        return decision;
    }
}

/// <summary>
/// Executor that presents the final approved travel advice to the user.
/// </summary>
internal sealed class SummaryExecutor : Executor<ReviewDecision, ChatMessage>
{
    private readonly AIAgent _agent;

    public SummaryExecutor(IChatClient chatClient, string[] requiredTopics) : base("Summary")
    {
        string topicsList = string.Join(", ", requiredTopics.Select(t => char.ToUpper(t[0]) + t[1..]));
        
        _agent = new ChatClientAgent(
            chatClient,
            name: "Summary",
            instructions: $"""
                You present the final approved travel advice to the user.
                Format the content nicely with clear sections for each topic: {topicsList}.
                Add a brief welcoming introduction and a closing statement encouraging the user to visit.
                Keep the original content but polish it for presentation.
                """
        );
    }

    public override async ValueTask<ChatMessage> HandleAsync(
        ReviewDecision message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== Summary - Preparing Final Output ===\n");
        Console.ResetColor();

        string prompt = $"Polish and present this approved travel advice:\n\n{message.Content}";

        StringBuilder sb = new();
        await foreach (AgentRunResponseUpdate update in _agent.RunStreamingAsync(new ChatMessage(ChatRole.User, prompt), cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                sb.Append(update.Text);
            }
        }

        ChatMessage result = new(ChatRole.Assistant, sb.ToString());
        await context.YieldOutputAsync(result, cancellationToken);
        return result;
    }
}
