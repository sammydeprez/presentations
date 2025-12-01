// ============================================================================
// Swarm Workflow Sample - Microsoft Agent Framework (.NET)
// 
// Demonstrates: Swarm pattern where all agents can talk to each other
// Pattern: Any Agent <-> Any Other Agent (no central supervisor)
// 
// Based on the Python LangGraph swarm pattern - agents form a peer-to-peer
// network and can hand off to any other agent based on the conversation needs.
// ============================================================================

using System.ComponentModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

// ============================================================================
// Configuration
// ============================================================================

var openAiEndpoint = new Uri("##ENDPOINT##");
var openAiKey = "##API-KEY##";

var openAIClient = new AzureOpenAIClient(openAiEndpoint, new Azure.AzureKeyCredential(openAiKey));
var deploymentName = "gpt-4.1-mini";

IChatClient chatClient = openAIClient
    .GetChatClient(deploymentName)
    .AsIChatClient();

// ============================================================================
// Define Tools for each specialist agent
// ============================================================================

// Hotel Booking Tools
[Description("Book a hotel for a given destination and number of nights")]
static string BookHotel(string destination, int nights, DateTime startDate)
{
    return $"✅ Hotel booked in {destination} for {nights} nights starting from {startDate:yyyy-MM-dd}. Booking Code: HLT{Random.Shared.Next(100, 999)}";
}

[Description("Get available hotels for a given destination and start date")]
static string[] GetHotels(string destination, DateTime startDate)
{
    return ["Holiday Inn", "Marriott", "Hilton", "Best Western", "Hyatt"];
}

// Flight Booking Tools
[Description("Get available flights from origin to destination on a specific date")]
static string[] GetFlightsAvailable(string origin, string destination, DateTime date)
{
    return [$"Flight QF32 ({origin} → {destination})", 
            $"Flight QF4 ({origin} → {destination})", 
            $"Flight QF128 ({origin} → {destination})"];
}

[Description("Book a flight for a specific flight number and date")]
static string BookFlight(string flightNo, DateTime date)
{
    return $"✅ Flight {flightNo} booked on {date:yyyy-MM-dd}. Booking Code: FLY{Random.Shared.Next(100, 999)}";
}

// Event Booking Tools
[Description("Book an event ticket for a specific event on a specific date")]
static string BookEvent(string eventName, DateTime date)
{
    return $"✅ Ticket booked for {eventName} on {date:yyyy-MM-dd}. Ticket Code: EVT{Random.Shared.Next(100, 999)}";
}

[Description("Get available events in a city on a specific date")]
static string[] GetEvents(string city, DateTime date)
{
    return ["Cirque du Soleil - Corteo", "Hamilton Musical", "Local Concert", "Sports Game"];
}

// ============================================================================
// Create Swarm Agents - Each can hand off to any other agent
// ============================================================================

// Hotel Booking Agent - can delegate to flight or event agents
var hotelBookingAgent = new ChatClientAgent(
    chatClient,
    name: "HotelBookingAgent",
    description: "Handles hotel searches and bookings. Can coordinate with flight and event agents for complete trip planning.",
    instructions: """
        You are a hotel booking specialist in a collaborative travel agent swarm.
        
        Your capabilities:
        - Search for available hotels in any destination
        - Book hotels for specified dates and number of nights
        
        SWARM BEHAVIOR:
        You can hand off to other agents when the user needs additional services:
        - Hand off to FlightBookingAgent if the user mentions needing flights
        - Hand off to EventBookingAgent if the user mentions events, shows, or entertainment
        
        After completing your hotel task, check if the user mentioned other travel needs
        and proactively hand off to the appropriate agent.
        
        Be collaborative - you're part of a team helping plan complete trips!
        """,
    tools: [
        AIFunctionFactory.Create(BookHotel),
        AIFunctionFactory.Create(GetHotels)
    ]
);

// Flight Booking Agent - can delegate to hotel or event agents
var flightBookingAgent = new ChatClientAgent(
    chatClient,
    name: "FlightBookingAgent",
    description: "Handles flight searches and bookings. Can coordinate with hotel and event agents for complete trip planning.",
    instructions: """
        You are a flight booking specialist in a collaborative travel agent swarm.
        
        Your capabilities:
        - Search for available flights between cities
        - Book flights for specified dates
        
        SWARM BEHAVIOR:
        You can hand off to other agents when the user needs additional services:
        - Hand off to HotelBookingAgent if the user mentions needing accommodation
        - Hand off to EventBookingAgent if the user mentions events, shows, or entertainment
        
        After completing your flight task, check if the user mentioned other travel needs
        and proactively hand off to the appropriate agent.
        
        Be collaborative - you're part of a team helping plan complete trips!
        """,
    tools: [
        AIFunctionFactory.Create(GetFlightsAvailable),
        AIFunctionFactory.Create(BookFlight)
    ]
);

// Event Booking Agent - can delegate to hotel or flight agents
var eventBookingAgent = new ChatClientAgent(
    chatClient,
    name: "EventBookingAgent",
    description: "Handles event ticket searches and bookings. Can coordinate with hotel and flight agents for complete trip planning.",
    instructions: """
        You are an event booking specialist in a collaborative travel agent swarm.
        
        Your capabilities:
        - Search for available events in any city
        - Book tickets for events on specified dates
        
        SWARM BEHAVIOR:
        You can hand off to other agents when the user needs additional services:
        - Hand off to HotelBookingAgent if the user mentions needing accommodation
        - Hand off to FlightBookingAgent if the user mentions needing flights
        
        After completing your event booking task, check if the user mentioned other travel needs
        and proactively hand off to the appropriate agent.
        
        Be collaborative - you're part of a team helping plan complete trips!
        """,
    tools: [
        AIFunctionFactory.Create(BookEvent),
        AIFunctionFactory.Create(GetEvents)
    ]
);

// ============================================================================
// Build Swarm Workflow - Every agent can talk to every other agent
// ============================================================================

// In a swarm, we still need one agent to be the "entry point"
// But every agent can hand off to any other agent
var allAgents = new[] { hotelBookingAgent, flightBookingAgent, eventBookingAgent };

var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(hotelBookingAgent)
    // Hotel agent can hand off to flight and event agents
    .WithHandoffs(hotelBookingAgent, [flightBookingAgent, eventBookingAgent])
    // Flight agent can hand off to hotel and event agents
    .WithHandoffs(flightBookingAgent, [hotelBookingAgent, eventBookingAgent])
    // Event agent can hand off to hotel and flight agents
    .WithHandoffs(eventBookingAgent, [hotelBookingAgent, flightBookingAgent])
    .Build();

// ============================================================================
// Workflow Visualization
// ============================================================================

Console.WriteLine("\n" + new string('=', 80));
Console.WriteLine("🐝 SWARM WORKFLOW - Collaborative Travel Agents");
Console.WriteLine(new string('=', 80));
Console.WriteLine();
Console.WriteLine("Agents in this swarm (each can talk to any other):");
Console.WriteLine("  🏨 HotelBookingAgent  ←→  ✈️ FlightBookingAgent");
Console.WriteLine("       ↕                         ↕");
Console.WriteLine("  🎭 EventBookingAgent  ←→  (connected to all)");
Console.WriteLine();
Console.WriteLine("Key Difference from Supervisor:");
Console.WriteLine("  • No central coordinator - agents collaborate directly");
Console.WriteLine("  • Any agent can hand off to any other agent");
Console.WriteLine("  • Agents proactively route based on conversation context");
Console.WriteLine();
Console.WriteLine("Workflow Structure:");
Console.WriteLine(workflow.ToMermaidString());
Console.WriteLine();
Console.WriteLine(new string('=', 80));
Console.WriteLine();

// ============================================================================
// Interactive Chat Loop
// ============================================================================

Console.WriteLine("💬 Chat with the Travel Agent Swarm (type 'exit' to quit)");
Console.WriteLine();
Console.WriteLine("Example requests:");
Console.WriteLine("  • I need a hotel in Helsinki for 5 nights, plus a flight from London");
Console.WriteLine("  • Book me a flight to Dubai and find events happening there");
Console.WriteLine("  • I want to see Cirque du Soleil and need a hotel nearby");
Console.WriteLine("  • Plan my complete trip: hotel, flight, and show in Helsinki");
Console.WriteLine();

List<ChatMessage> conversationHistory = [];

while (true)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("You: ");
    Console.ResetColor();
    
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n👋 Thank you for using the Travel Agent Swarm! Goodbye!\n");
        break;
    }
    
    // Add user message to history
    conversationHistory.Add(new ChatMessage(ChatRole.User, userInput));
    
    Console.WriteLine();
    
    try
    {
        // Stream the workflow execution
        string? lastExecutorId = null;
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, conversationHistory);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        
        await foreach (var evt in run.WatchStreamAsync())
        {
            if (evt is AgentRunUpdateEvent updateEvent)
            {
                // Show executor name when it changes (shows the swarm handoffs)
                if (updateEvent.ExecutorId != lastExecutorId)
                {
                    lastExecutorId = updateEvent.ExecutorId;
                    var emoji = updateEvent.ExecutorId switch
                    {
                        var id when id.Contains("HotelBookingAgent") => "🏨",
                        var id when id.Contains("FlightBookingAgent") => "✈️",
                        var id when id.Contains("EventBookingAgent") => "🎭",
                        _ => "🐝"
                    };
                    Console.ForegroundColor = updateEvent.ExecutorId switch
                    {
                        var id when id.Contains("HotelBookingAgent") => ConsoleColor.Green,
                        var id when id.Contains("FlightBookingAgent") => ConsoleColor.Blue,
                        var id when id.Contains("EventBookingAgent") => ConsoleColor.Magenta,
                        _ => ConsoleColor.White
                    };
                    Console.WriteLine();
                    Console.Write($"{emoji} [{updateEvent.ExecutorId}]: ");
                    Console.ResetColor();
                }
                
                // Print text content
                Console.Write(updateEvent.Update.Text);
                
                // Show function calls and handoffs
                if (updateEvent.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is FunctionCallContent call)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    if (call.Name.StartsWith("handoff_to_"))
                    {
                        Console.WriteLine($"   🔄 Handoff: {call.Name.Replace("handoff_to_", "→ ")}");
                    }
                    else
                    {
                        Console.WriteLine($"   🔧 Calling: {call.Name}");
                    }
                    Console.ResetColor();
                }
            }
            else if (evt is WorkflowOutputEvent outputEvent)
            {
                // Add the output messages to conversation history
                if (outputEvent.As<List<ChatMessage>>() is List<ChatMessage> outputMessages)
                {
                    conversationHistory.AddRange(outputMessages);
                }
                Console.WriteLine();
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
    
    Console.WriteLine();
}

// ============================================================================
// Summary
// ============================================================================

Console.WriteLine(new string('=', 80));
Console.WriteLine("✅ Sample Complete: Swarm Pattern");
Console.WriteLine(new string('=', 80));
Console.WriteLine();
Console.WriteLine("Key Concepts Demonstrated:");
Console.WriteLine("  ✓ Swarm pattern - agents as peers without central supervisor");
Console.WriteLine("  ✓ Bidirectional handoffs between all agents");
Console.WriteLine("  ✓ Collaborative task completion across multiple agents");
Console.WriteLine("  ✓ Agents proactively route based on context");
Console.WriteLine("  ✓ Visible handoff tracking in streaming output");
Console.WriteLine();
Console.WriteLine("Swarm vs Supervisor:");
Console.WriteLine("  Supervisor: Central agent routes to specialists");
Console.WriteLine("  Swarm:      Agents collaborate as equals, any can route to any");
Console.WriteLine();
