// ============================================================================
// Parallel Travel Advice Workflow with Microsoft Agent Framework
// ============================================================================
// This application demonstrates the fan-out/fan-in parallelism pattern where:
// 1. A City Generator agent returns N random city names for a chosen country
// 2. Multiple Travel Advisor agents process each city IN PARALLEL (fan-out)
// 3. An Aggregator collects all travel advice and returns a combined response (fan-in)
//
// Key Concepts:
// - Fan-out pattern: One executor broadcasts work to multiple parallel executors
// - Fan-in pattern: Multiple executor results are collected into a single aggregator
// - Dynamic parallel execution: Number of parallel branches based on city count
// - Shared state: City assignments tracked across executors
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

const int NumberOfCities = 3;
const string Country = "Italy";

// ============================================================================
// Executor Initialization
// ============================================================================

var cityGeneratorExecutor = new CityGeneratorExecutor(chatClient, NumberOfCities);

// Create N travel advisor executors - one for each city
var travelAdvisors = Enumerable
    .Range(0, NumberOfCities)
    .Select(i => new ParallelTravelAdvisorExecutor(chatClient, $"TravelAdvisor_{i}"))
    .ToArray();

var aggregatorExecutor = new TravelAggregatorExecutor(NumberOfCities);

// ============================================================================
// Workflow Construction
// ============================================================================
// Build workflow with fan-out/fan-in pattern:
// CityGenerator -> [TravelAdvisor_0, TravelAdvisor_1, TravelAdvisor_2] -> Aggregator

var workflowBuilder = new WorkflowBuilder(cityGeneratorExecutor)
    .AddFanOutEdge(cityGeneratorExecutor, [.. travelAdvisors])
    .AddFanInEdge([.. travelAdvisors], aggregatorExecutor)
    .WithOutputFrom(aggregatorExecutor);

var workflow = workflowBuilder.Build();

// ============================================================================
// Workflow Visualization & Execution
// ============================================================================

Console.WriteLine("\n=== Parallel Travel Advice Workflow ===\n");
Console.WriteLine($"Generating travel advice for {NumberOfCities} random cities in {Country}");
Console.WriteLine("Each city will be processed IN PARALLEL by separate advisors\n");

Console.WriteLine("Workflow Mermaid Representation:");
Console.WriteLine("=====================================");
Console.WriteLine(workflow.ToMermaidString());
Console.WriteLine("=====================================\n");

// Execute the workflow
Console.WriteLine(new string('=', 80));
Console.WriteLine($"QUERY: Get travel advice for {NumberOfCities} cities in {Country}");
Console.WriteLine(new string('=', 80) + "\n");

await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, Country);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is WorkflowOutputEvent outputEvent)
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ AGGREGATED TRAVEL GUIDE");
        Console.ResetColor();
        Console.WriteLine(new string('=', 80));
        Console.WriteLine();
        Console.WriteLine(outputEvent.Data);
        Console.WriteLine();
        Console.WriteLine(new string('=', 80));
    }
}

Console.WriteLine("\n✅ Sample Complete: Parallel Travel Advice demonstrates fan-out/fan-in pattern\n");
Console.WriteLine("Key Concepts Demonstrated:");
Console.WriteLine("  ✓ Fan-out pattern: Broadcast cities to parallel advisors");
Console.WriteLine("  ✓ Fan-in pattern: Aggregate all advice into single response");
Console.WriteLine($"  ✓ Parallel execution: {NumberOfCities} advisors running concurrently");
Console.WriteLine("  ✓ Dynamic city generation with structured output");
Console.WriteLine("  ✓ Shared state for city-to-advisor assignment\n");

// ============================================================================
// Shared State for City Assignment
// ============================================================================

internal static class CityStateShared
{
    public const string Scope = "CityStateScope";
    public const string CitiesKey = "cities";
}

// ============================================================================
// Data Transfer Objects
// ============================================================================

[Description("List of city names generated for travel advice")]
internal sealed class CityList
{
    [JsonPropertyName("cities")]
    [Description("Array of city names in the specified country")]
    public List<string> Cities { get; set; } = [];
}

internal sealed class CityAssignment
{
    public string City { get; set; } = "";
    public int AdvisorIndex { get; set; }
}

internal sealed class TravelAdviceResult
{
    public string City { get; set; } = "";
    public string Advice { get; set; } = "";
    public string AdvisorId { get; set; } = "";
}

// ============================================================================
// Custom Executors
// ============================================================================

/// <summary>
/// Executor that generates N random city names for a given country.
/// Broadcasts city assignments to all parallel travel advisors.
/// </summary>
internal sealed class CityGeneratorExecutor : Executor<string, List<CityAssignment>>
{
    private readonly AIAgent _agent;
    private readonly int _numberOfCities;

    public CityGeneratorExecutor(IChatClient chatClient, int numberOfCities) : base("CityGenerator")
    {
        _numberOfCities = numberOfCities;
        _agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "CityGenerator",
            Instructions = $"""
                You are a geography expert. When given a country name, generate exactly {numberOfCities} 
                different, interesting cities from that country that would be great travel destinations.
                
                Choose diverse cities - mix famous tourist destinations with hidden gems.
                Return ONLY the city names, no descriptions.
                """,
            ChatOptions = new()
            {
                ResponseFormat = ChatResponseFormat.ForJsonSchema<CityList>()
            }
        });
    }

    public override async ValueTask<List<CityAssignment>> HandleAsync(
        string country,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== City Generator ===\n");
        Console.ResetColor();
        Console.WriteLine($"Generating {_numberOfCities} cities in {country}...\n");

        var prompt = $"Generate {_numberOfCities} interesting cities to visit in {country}";
        var response = await _agent.RunAsync(new ChatMessage(ChatRole.User, prompt), cancellationToken: cancellationToken);
        
        var cityList = response.Deserialize<CityList>(JsonSerializerOptions.Web);
        
        Console.WriteLine("Generated cities:");
        for (int i = 0; i < cityList.Cities.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {cityList.Cities[i]} → TravelAdvisor_{i}");
        }
        Console.WriteLine();

        // Store cities in shared state so advisors can access their assigned city
        await context.QueueStateUpdateAsync(CityStateShared.CitiesKey, cityList.Cities, scopeName: CityStateShared.Scope);

        // Create assignments - each advisor gets one city
        var assignments = cityList.Cities
            .Select((city, index) => new CityAssignment { City = city, AdvisorIndex = index })
            .ToList();

        return assignments;
    }
}

/// <summary>
/// Executor that provides travel advice for a specific city.
/// Multiple instances run in parallel, each handling one city.
/// </summary>
internal sealed class ParallelTravelAdvisorExecutor : Executor<List<CityAssignment>, TravelAdviceResult>
{
    private readonly AIAgent _agent;
    private readonly int _advisorIndex;

    public ParallelTravelAdvisorExecutor(IChatClient chatClient, string id) : base(id)
    {
        // Extract index from ID (e.g., "TravelAdvisor_0" -> 0)
        _advisorIndex = int.Parse(id.Split('_').Last());
        
        _agent = new ChatClientAgent(
            chatClient,
            name: id,
            instructions: """
                You are an expert travel advisor. Provide concise but helpful travel advice for the given city.
                Include:
                - 2-3 must-see attractions
                - 1 local food recommendation
                - 1 practical tip for visitors
                
                Keep your response to about 100-150 words.
                """
        );
    }

    public override async ValueTask<TravelAdviceResult> HandleAsync(
        List<CityAssignment> assignments,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // Find the assignment for this advisor
        var myAssignment = assignments.FirstOrDefault(a => a.AdvisorIndex == _advisorIndex);
        
        if (myAssignment == null)
        {
            return new TravelAdviceResult 
            { 
                City = "Unknown", 
                Advice = "No city assigned", 
                AdvisorId = Id 
            };
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"=== {Id} - Processing: {myAssignment.City} ===\n");
        Console.ResetColor();

        var prompt = $"Provide travel advice for visiting {myAssignment.City}";
        
        StringBuilder sb = new();
        await foreach (var update in _agent.RunStreamingAsync(new ChatMessage(ChatRole.User, prompt), cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                sb.Append(update.Text);
                Console.Write(update.Text);
            }
        }
        Console.WriteLine("\n");

        return new TravelAdviceResult
        {
            City = myAssignment.City,
            Advice = sb.ToString(),
            AdvisorId = Id
        };
    }
}

/// <summary>
/// Executor that aggregates travel advice from all parallel advisors.
/// Receives results via fan-in - called once per advisor, accumulates until all received.
/// </summary>
internal sealed class TravelAggregatorExecutor : Executor<TravelAdviceResult>
{
    private readonly int _expectedResults;
    private readonly List<TravelAdviceResult> _results = [];

    public TravelAggregatorExecutor(int expectedResults) : base("TravelAggregator")
    {
        _expectedResults = expectedResults;
    }

    public override async ValueTask HandleAsync(
        TravelAdviceResult result,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        _results.Add(result);
        
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"[Aggregator] Received result {_results.Count}/{_expectedResults} from {result.AdvisorId} for {result.City}");
        Console.ResetColor();

        // Only produce output when all results are collected
        if (_results.Count >= _expectedResults)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n=== Travel Aggregator ===\n");
            Console.ResetColor();
            Console.WriteLine($"All {_results.Count} advisors have reported. Aggregating...\n");

            var sb = new StringBuilder();
            sb.AppendLine("🌍 COMPREHENSIVE TRAVEL GUIDE");
            sb.AppendLine(new string('─', 50));
            sb.AppendLine();

            foreach (var r in _results.OrderBy(r => r.City))
            {
                sb.AppendLine($"📍 {r.City.ToUpper()}");
                sb.AppendLine(new string('─', 30));
                sb.AppendLine(r.Advice);
                sb.AppendLine();
            }

            sb.AppendLine(new string('─', 50));
            sb.AppendLine($"✨ Generated advice for {_results.Count} cities in parallel!");

            await context.YieldOutputAsync(sb.ToString(), cancellationToken);
        }
    }
}
