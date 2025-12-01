// ============================================================================
// Supervisor Workflow Sample - Microsoft Agent Framework (.NET)
// 
// Demonstrates: Supervisor pattern with handoffs between specialist agents
// Pattern: Supervisor -> [HotelBookingAgent | FlightBookingAgent | EventBookingAgent]
// 
// Based on the Python LangGraph supervisor pattern - the supervisor routes
// user requests to the appropriate specialist agent based on the task.
// ============================================================================

using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
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
// Create Specialist Agents
// ============================================================================

// Hotel Booking Agent
var hotelBookingAgent = new ChatClientAgent(
    chatClient,
    name: "HotelBookingAgent",
    description: "Handles hotel searches and bookings. Use this agent for any hotel-related requests.",
    instructions: """
        You are a hotel booking specialist. You can:
        - Search for available hotels in any destination
        - Book hotels for specified dates and number of nights
        
        Always confirm the destination, dates, and number of nights before booking.
        Provide helpful information about the hotels available.
        """,
    tools: [
        AIFunctionFactory.Create(BookHotel),
        AIFunctionFactory.Create(GetHotels)
    ]
);

// Flight Booking Agent
var flightBookingAgent = new ChatClientAgent(
    chatClient,
    name: "FlightBookingAgent",
    description: "Handles flight searches and bookings. Use this agent for any flight-related requests.",
    instructions: """
        You are a flight booking specialist. You can:
        - Search for available flights between cities
        - Book flights for specified dates
        
        Always confirm the origin, destination, and travel date before booking.
        Provide flight options and details to help the user choose.
        """,
    tools: [
        AIFunctionFactory.Create(GetFlightsAvailable),
        AIFunctionFactory.Create(BookFlight)
    ]
);

// Event Booking Agent
var eventBookingAgent = new ChatClientAgent(
    chatClient,
    name: "EventBookingAgent",
    description: "Handles event ticket searches and bookings. Use this agent for any event or entertainment bookings.",
    instructions: """
        You are an event booking specialist. You can:
        - Search for available events in any city
        - Book tickets for events on specified dates
        
        Always confirm the event name and date before booking.
        Provide information about available events to help the user choose.
        """,
    tools: [
        AIFunctionFactory.Create(BookEvent),
        AIFunctionFactory.Create(GetEvents)
    ]
);

// ============================================================================
// Create Supervisor Agent
// ============================================================================

var supervisorAgent = new ChatClientAgent(
    chatClient,
    name: "TravelSupervisor",
    description: "Travel agency supervisor that routes requests to appropriate specialists",
    instructions: """
        You are a travel agency supervisor managing a team of specialist agents.
        Your job is to analyze the user's request and delegate to the appropriate specialist:
        
        - For HOTEL bookings or searches → handoff to HotelBookingAgent
        - For FLIGHT bookings or searches → handoff to FlightBookingAgent  
        - For EVENT tickets or entertainment → handoff to EventBookingAgent
        
        When a user has multiple needs (e.g., hotel + flight + event), handle them one at a time.
        Always handoff to a specialist - do not try to handle requests yourself.
        After a specialist completes their task, ask if there's anything else needed.
        """
);

// ============================================================================
// Build Supervisor Workflow using HandoffsWorkflowBuilder
// ============================================================================

var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(supervisorAgent)
    // Supervisor can handoff to any specialist
    .WithHandoffs(supervisorAgent, [hotelBookingAgent, flightBookingAgent, eventBookingAgent])
    // Specialists can hand back to supervisor when done
    .WithHandoffs([hotelBookingAgent, flightBookingAgent, eventBookingAgent], supervisorAgent)
    .Build();

// ============================================================================
// Workflow Visualization
// ============================================================================

Console.WriteLine("\n" + new string('=', 80));
Console.WriteLine("🎯 SUPERVISOR WORKFLOW - Travel Agency");
Console.WriteLine(new string('=', 80));
Console.WriteLine();
Console.WriteLine("Agents in this workflow:");
Console.WriteLine("  👔 TravelSupervisor - Routes requests to specialists");
Console.WriteLine("  🏨 HotelBookingAgent - Handles hotel searches and bookings");
Console.WriteLine("  ✈️  FlightBookingAgent - Handles flight searches and bookings");
Console.WriteLine("  🎭 EventBookingAgent - Handles event ticket bookings");
Console.WriteLine();
Console.WriteLine("Workflow Structure:");
Console.WriteLine(workflow.ToMermaidString());
Console.WriteLine();
Console.WriteLine(new string('=', 80));
Console.WriteLine();

// ============================================================================
// Interactive Chat Loop
// ============================================================================

Console.WriteLine("💬 Chat with the Travel Agency (type 'exit' to quit)");
Console.WriteLine();
Console.WriteLine("Example requests:");
Console.WriteLine("  • I need a hotel in Helsinki for 14 nights starting December 1st");
Console.WriteLine("  • Book me a flight from London to Dubai on December 1st 2025");
Console.WriteLine("  • I want tickets for Cirque du Soleil on December 10th");
Console.WriteLine("  • I need help planning a trip to Helsinki with hotel, flight, and show tickets");
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
        Console.WriteLine("\n👋 Thank you for using Travel Agency! Goodbye!\n");
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
                // Show executor name when it changes
                if (updateEvent.ExecutorId != lastExecutorId)
                {
                    lastExecutorId = updateEvent.ExecutorId;
                    var emoji = updateEvent.ExecutorId switch
                    {
                        var id when id.Contains("TravelSupervisor") => "👔",
                        var id when id.Contains("HotelBookingAgent") => "🏨",
                        var id when id.Contains("FlightBookingAgent") => "✈️",
                        var id when id.Contains("EventBookingAgent") => "🎭",
                        _ => "🤖"
                    };
                    Console.ForegroundColor = updateEvent.ExecutorId switch
                    {
                        var id when id.Contains("TravelSupervisor") => ConsoleColor.Yellow,
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
                
                // Show function calls
                if (updateEvent.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is FunctionCallContent call)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"   🔧 Calling: {call.Name}");
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
Console.WriteLine("✅ Sample Complete: Supervisor Pattern with Handoffs");
Console.WriteLine(new string('=', 80));
Console.WriteLine();
Console.WriteLine("Key Concepts Demonstrated:");
Console.WriteLine("  ✓ HandoffsWorkflowBuilder for supervisor-agent patterns");
Console.WriteLine("  ✓ Supervisor agent that routes to specialists");
Console.WriteLine("  ✓ Multiple specialist agents with domain-specific tools");
Console.WriteLine("  ✓ Bidirectional handoffs (supervisor ↔ specialists)");
Console.WriteLine("  ✓ Streaming responses with agent identification");
Console.WriteLine("  ✓ Interactive chat loop with conversation history");
Console.WriteLine();
