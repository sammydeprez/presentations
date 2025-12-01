// ============================================================================
// Planner Workflow Sample - Microsoft Agent Framework (.NET)
// 
// Demonstrates: Plan-Execute-Replan pattern
// Pattern: Planner → Agent executes steps → Replanner evaluates → Continue/End
// 
// Use Case: Trip Budget Calculator
// - Planner breaks down the budget calculation into steps
// - Agent executes each step using calculation tools
// - Replanner decides if more steps are needed or if complete
// ============================================================================

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
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
// Define Budget Calculation Tools (no web search needed!)
// ============================================================================

// Accommodation cost calculator
[Description("Calculate hotel cost for a destination. Returns estimated cost per night in EUR.")]
static decimal GetHotelCostPerNight(string destination, string hotelType)
{
    // Simulated hotel prices by destination and type
    var basePrices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["Paris"] = 150, ["London"] = 180, ["Tokyo"] = 120, ["New York"] = 200,
        ["Dubai"] = 160, ["Rome"] = 130, ["Barcelona"] = 110, ["Amsterdam"] = 140,
        ["Sydney"] = 170, ["Helsinki"] = 125, ["default"] = 100
    };
    
    var multipliers = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["budget"] = 0.5m, ["standard"] = 1.0m, ["luxury"] = 2.5m, ["default"] = 1.0m
    };

    var basePrice = basePrices.GetValueOrDefault(destination, basePrices["default"]);
    var multiplier = multipliers.GetValueOrDefault(hotelType, multipliers["default"]);
    
    return Math.Round(basePrice * multiplier, 2);
}

[Description("Calculate total accommodation cost for the trip")]
static decimal CalculateAccommodationTotal(decimal costPerNight, int numberOfNights)
{
    return Math.Round(costPerNight * numberOfNights, 2);
}

// Flight cost calculator
[Description("Get estimated flight cost between two cities. Returns cost in EUR.")]
static decimal GetFlightCost(string origin, string destination, string flightClass)
{
    // Simulated flight costs based on distance approximation
    var distances = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["Paris"] = 0, ["London"] = 350, ["Tokyo"] = 9700, ["New York"] = 5850,
        ["Dubai"] = 5250, ["Rome"] = 1100, ["Barcelona"] = 830, ["Amsterdam"] = 430,
        ["Sydney"] = 16900, ["Helsinki"] = 1900
    };
    
    var classMultipliers = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["economy"] = 1.0m, ["business"] = 3.0m, ["first"] = 6.0m, ["default"] = 1.0m
    };

    // Base cost: ~0.10 EUR per km (simplified)
    var originDist = distances.GetValueOrDefault(origin, 1000);
    var destDist = distances.GetValueOrDefault(destination, 1000);
    var distance = Math.Abs(originDist - destDist);
    if (distance == 0) distance = 500; // Same city/nearby
    
    var baseCost = distance * 0.10m;
    var multiplier = classMultipliers.GetValueOrDefault(flightClass, classMultipliers["default"]);
    
    return Math.Round(Math.Max(baseCost * multiplier, 50), 2); // Minimum 50 EUR
}

// Daily expenses calculator
[Description("Calculate daily expenses (food, transport, activities) for a destination")]
static decimal GetDailyExpenses(string destination, string budgetLevel)
{
    var baseCosts = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["Paris"] = 80, ["London"] = 90, ["Tokyo"] = 70, ["New York"] = 100,
        ["Dubai"] = 85, ["Rome"] = 65, ["Barcelona"] = 60, ["Amsterdam"] = 75,
        ["Sydney"] = 80, ["Helsinki"] = 70, ["default"] = 60
    };
    
    var multipliers = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["budget"] = 0.6m, ["moderate"] = 1.0m, ["luxury"] = 2.0m, ["default"] = 1.0m
    };

    var baseCost = baseCosts.GetValueOrDefault(destination, baseCosts["default"]);
    var multiplier = multipliers.GetValueOrDefault(budgetLevel, multipliers["default"]);
    
    return Math.Round(baseCost * multiplier, 2);
}

[Description("Calculate total daily expenses for the trip")]
static decimal CalculateDailyExpensesTotal(decimal dailyCost, int numberOfDays)
{
    return Math.Round(dailyCost * numberOfDays, 2);
}

// Currency conversion
[Description("Convert amount from one currency to EUR")]
static decimal ConvertToEur(decimal amount, string fromCurrency)
{
    var rates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = 0.92m, ["GBP"] = 1.17m, ["JPY"] = 0.0062m, ["AED"] = 0.25m,
        ["AUD"] = 0.60m, ["EUR"] = 1.0m, ["default"] = 1.0m
    };
    
    var rate = rates.GetValueOrDefault(fromCurrency, rates["default"]);
    return Math.Round(amount * rate, 2);
}

// Sum calculator
[Description("Calculate the total budget by summing all components")]
static decimal CalculateTotalBudget(decimal accommodation, decimal flights, decimal dailyExpenses, decimal extras = 0)
{
    return Math.Round(accommodation + flights + dailyExpenses + extras, 2);
}

// Budget breakdown formatter
[Description("Format a detailed budget breakdown as a summary")]
static string FormatBudgetSummary(decimal accommodation, decimal flights, decimal dailyExpenses, decimal total)
{
    return $"""
        📊 BUDGET BREAKDOWN
        ═══════════════════════════════
        🏨 Accommodation:  €{accommodation:N2}
        ✈️ Flights:        €{flights:N2}
        🍽️ Daily Expenses: €{dailyExpenses:N2}
        ───────────────────────────────
        💰 TOTAL:          €{total:N2}
        ═══════════════════════════════
        """;
}

var tools = new AIFunction[]
{
    AIFunctionFactory.Create(GetHotelCostPerNight),
    AIFunctionFactory.Create(CalculateAccommodationTotal),
    AIFunctionFactory.Create(GetFlightCost),
    AIFunctionFactory.Create(GetDailyExpenses),
    AIFunctionFactory.Create(CalculateDailyExpensesTotal),
    AIFunctionFactory.Create(ConvertToEur),
    AIFunctionFactory.Create(CalculateTotalBudget),
    AIFunctionFactory.Create(FormatBudgetSummary)
};

// ============================================================================
// Define Planner Agent - Creates the step-by-step plan
// ============================================================================

var plannerAgent = new ChatClientAgent(
    chatClient,
    name: "TripBudgetPlanner",
    description: "Creates step-by-step plans for calculating trip budgets",
    instructions: """
        You are a trip budget planning specialist. Your job is to create a detailed
        step-by-step plan for calculating a complete trip budget.
        
        When given a trip request, break it down into clear, sequential steps:
        1. Calculate accommodation costs (hotel type × nights)
        2. Calculate flight costs (origin to destination, class)
        3. Calculate daily expenses (food, transport, activities)
        4. Sum up all costs for total budget
        5. Format the final budget breakdown
        
        Create a numbered plan with specific, actionable steps.
        Each step should be clear enough for another agent to execute.
        Include all relevant details from the user's request in each step.
        
        Output your plan as a numbered list.
        """
);

// ============================================================================
// Define Executor Agent - Executes each step of the plan
// ============================================================================

var executorAgent = new ChatClientAgent(
    chatClient,
    name: "BudgetCalculator",
    description: "Executes budget calculation steps using available tools",
    instructions: """
        You are a budget calculator. You execute specific calculation tasks
        using the available tools.
        
        For each task you're given:
        1. Identify which tool(s) to use
        2. Execute the calculations
        3. Report the result clearly
        
        Available tools include:
        - GetHotelCostPerNight: Get nightly hotel costs
        - CalculateAccommodationTotal: Total hotel cost
        - GetFlightCost: Flight prices by class
        - GetDailyExpenses: Daily costs for food/transport/activities
        - CalculateDailyExpensesTotal: Total daily expenses
        - ConvertToEur: Currency conversion
        - CalculateTotalBudget: Sum all costs
        - FormatBudgetSummary: Create formatted summary
        
        Execute the step you're given and report the numeric result.
        """,
    tools: tools
);

// ============================================================================
// Define Replanner Agent - Evaluates progress and decides next action
// ============================================================================

var replannerAgent = new ChatClientAgent(
    chatClient,
    name: "BudgetReplanner",
    description: "Evaluates progress and decides if more steps are needed",
    instructions: """
        You are a replanner that evaluates budget calculation progress.
        
        Given:
        - The original user request
        - The original plan
        - The steps completed so far with their results
        
        Decide:
        1. If all necessary calculations are complete, provide the FINAL response
           summarizing the budget with all calculated values.
        2. If more steps are needed, provide the REMAINING steps still to be done.
        
        When providing a final response, include:
        - All calculated costs
        - The total budget
        - A brief summary
        
        Be concise. If the budget is fully calculated, just provide the summary.
        """
);

// ============================================================================
// Manual Plan-Execute-Replan Loop
// ============================================================================

Console.WriteLine("\n" + new string('=', 80));
Console.WriteLine("📋 PLANNER WORKFLOW - Trip Budget Calculator");
Console.WriteLine(new string('=', 80));
Console.WriteLine();
Console.WriteLine("Pattern: Planner → Executor → Replanner → (Loop or End)");
Console.WriteLine();
Console.WriteLine("   ┌─────────┐     ┌──────────┐     ┌───────────┐");
Console.WriteLine("   │ Planner │ ──► │ Executor │ ──► │ Replanner │");
Console.WriteLine("   └─────────┘     └──────────┘     └─────┬─────┘");
Console.WriteLine("                         ▲               │");
Console.WriteLine("                         │   More steps? │");
Console.WriteLine("                         └───────────────┘");
Console.WriteLine("                                 │");
Console.WriteLine("                                 ▼ Done");
Console.WriteLine("                            [Response]");
Console.WriteLine();
Console.WriteLine(new string('=', 80));
Console.WriteLine();

// Interactive loop
Console.WriteLine("💬 Enter your trip budget request (type 'exit' to quit)");
Console.WriteLine();
Console.WriteLine("Example requests:");
Console.WriteLine("  • Calculate budget for 5 nights in Paris, flying from London in economy");
Console.WriteLine("  • I need a budget for a luxury trip to Tokyo, 7 nights from New York");
Console.WriteLine("  • Budget a moderate trip to Barcelona for 4 nights from Amsterdam");
Console.WriteLine();

while (true)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("You: ");
    Console.ResetColor();
    
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n👋 Goodbye!\n");
        break;
    }
    
    Console.WriteLine();
    
    // Initialize state
    var state = new PlanState { UserRequest = userInput };
    
    try
    {
        // ================================================================
        // STEP 1: PLANNER - Create the plan
        // ================================================================
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("📋 PHASE 1: PLANNING");
        Console.WriteLine(new string('-', 40));
        Console.ResetColor();
        
        var planMessages = new List<ChatMessage>
        {
            new(ChatRole.User, $"Create a step-by-step plan to calculate the budget for: {userInput}")
        };
        
        var planResponse = await chatClient.GetResponseAsync(planMessages, new()
        {
            ModelId = deploymentName
        });
        
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(planResponse.Text);
        Console.ResetColor();
        
        // Parse the plan (simple extraction - in production, use structured output)
        var planLines = planResponse.Text.Split('\n')
            .Where(line => line.Trim().Length > 0)
            .Where(line => char.IsDigit(line.Trim()[0]) || line.Trim().StartsWith("-") || line.Trim().StartsWith("•"))
            .Select(line => line.Trim().TrimStart('-', '•', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', ' '))
            .Where(line => line.Length > 5)
            .Take(6)
            .ToList();
        
        state.Plan = planLines;
        Console.WriteLine();
        
        // ================================================================
        // STEP 2 & 3: EXECUTE-REPLAN LOOP
        // ================================================================
        int maxIterations = 10;
        int iteration = 0;
        
        while (iteration < maxIterations && state.FinalResponse == null)
        {
            iteration++;
            
            // ============================================================
            // EXECUTE: Run the next step
            // ============================================================
            if (state.CurrentStepIndex < state.Plan.Count)
            {
                var currentStep = state.Plan[state.CurrentStepIndex];
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"⚙️ PHASE 2: EXECUTING STEP {state.CurrentStepIndex + 1}");
                Console.WriteLine(new string('-', 40));
                Console.ResetColor();
                Console.WriteLine($"Task: {currentStep}");
                Console.WriteLine();
                
                // Build context for executor
                var execMessages = new List<ChatMessage>
                {
                    new(ChatRole.User, $"""
                        Execute this budget calculation step:
                        
                        Step: {currentStep}
                        
                        Original request: {state.UserRequest}
                        
                        Previous results:
                        {string.Join("\n", state.CompletedSteps.Select(s => $"- {s.Step}: {s.Result}"))}
                        
                        Use the available tools to calculate and report the result.
                        """)
                };
                
                // Execute with tools
                string? lastExecutorId = null;
                var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(executorAgent)
                    .Build();
                
                await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, execMessages);
                await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
                
                string executionResult = "";
                
                await foreach (var evt in run.WatchStreamAsync())
                {
                    if (evt is AgentRunUpdateEvent updateEvent)
                    {
                        if (updateEvent.ExecutorId != lastExecutorId)
                        {
                            lastExecutorId = updateEvent.ExecutorId;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write($"[{updateEvent.ExecutorId}]: ");
                            Console.ResetColor();
                        }
                        
                        Console.Write(updateEvent.Update.Text);
                        executionResult += updateEvent.Update.Text;
                        
                        // Show tool calls
                        if (updateEvent.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is FunctionCallContent call)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine($"\n   🔧 Using: {call.Name}");
                            Console.ResetColor();
                        }
                    }
                    else if (evt is WorkflowOutputEvent)
                    {
                        break;
                    }
                }
                
                Console.WriteLine();
                state.CompletedSteps.Add((currentStep, executionResult));
                state.CurrentStepIndex++;
            }
            
            // ============================================================
            // REPLAN: Evaluate progress
            // ============================================================
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\n🔄 PHASE 3: REPLANNING (Iteration {iteration})");
            Console.WriteLine(new string('-', 40));
            Console.ResetColor();
            
            var replanMessages = new List<ChatMessage>
            {
                new(ChatRole.User, $"""
                    Evaluate the progress on this budget calculation:
                    
                    Original request: {state.UserRequest}
                    
                    Original plan:
                    {string.Join("\n", state.Plan.Select((s, i) => $"{i + 1}. {s}"))}
                    
                    Completed steps:
                    {string.Join("\n", state.CompletedSteps.Select(s => $"✓ {s.Step}\n   Result: {s.Result}"))}
                    
                    If all necessary calculations are done and we have enough information to provide 
                    a complete budget summary, provide the FINAL budget summary now starting with "FINAL:".
                    
                    If more calculations are needed, list the remaining steps starting with "CONTINUE:".
                    """)
            };
            
            var replanResponse = await chatClient.GetResponseAsync(replanMessages, new()
            {
                ModelId = deploymentName
            });
            
            var replanText = replanResponse.Text;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(replanText);
            Console.ResetColor();
            Console.WriteLine();
            
            // Check if we're done
            if (replanText.Contains("FINAL:", StringComparison.OrdinalIgnoreCase) || 
                state.CurrentStepIndex >= state.Plan.Count ||
                replanText.Contains("complete", StringComparison.OrdinalIgnoreCase) && !replanText.Contains("CONTINUE:", StringComparison.OrdinalIgnoreCase))
            {
                state.FinalResponse = replanText;
                break;
            }
        }
        
        // ================================================================
        // FINAL OUTPUT
        // ================================================================
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("✅ BUDGET CALCULATION COMPLETE");
        Console.WriteLine(new string('=', 60));
        Console.ResetColor();
        
        if (state.FinalResponse != null)
        {
            Console.WriteLine(state.FinalResponse.Replace("FINAL:", "").Trim());
        }
        else
        {
            Console.WriteLine("Completed all planned steps. See results above.");
        }
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"📊 Stats: {state.CompletedSteps.Count} steps executed in {iteration} iterations");
        Console.ResetColor();
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
Console.WriteLine("✅ Sample Complete: Planner Pattern");
Console.WriteLine(new string('=', 80));
Console.WriteLine();
Console.WriteLine("Key Concepts Demonstrated:");
Console.WriteLine("  ✓ Plan-Execute-Replan loop");
Console.WriteLine("  ✓ Dynamic planning based on user request");
Console.WriteLine("  ✓ Step-by-step execution with tool usage");
Console.WriteLine("  ✓ Replanning to evaluate progress");
Console.WriteLine("  ✓ Conditional termination when goal achieved");
Console.WriteLine();
Console.WriteLine("Pattern Flow:");
Console.WriteLine("  1. Planner creates step-by-step plan");
Console.WriteLine("  2. Executor runs each step with tools");
Console.WriteLine("  3. Replanner evaluates: continue or finish?");
Console.WriteLine("  4. Loop until complete");
Console.WriteLine();
Console.WriteLine("Use Cases:");
Console.WriteLine("  • Complex multi-step calculations");
Console.WriteLine("  • Research tasks with multiple queries");
Console.WriteLine("  • Any task that benefits from decomposition");
Console.WriteLine();

// ============================================================================
// State class (must be declared after top-level statements)
// ============================================================================

class PlanState
{
    public string UserRequest { get; set; } = "";
    public List<string> Plan { get; set; } = [];
    public List<(string Step, string Result)> CompletedSteps { get; set; } = [];
    public string? FinalResponse { get; set; }
    public int CurrentStepIndex { get; set; } = 0;
}
