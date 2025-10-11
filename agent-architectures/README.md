# Agent Architectures

This repository contains a comprehensive presentation on different AI agent architectures using LangChain and LangGraph. The materials explore various patterns for building intelligent agents, from simple LLM calls to sophisticated multi-agent systems.

## 📖 Overview

AI agents are systems that can perceive their environment, make decisions, and take actions to achieve specific goals. This presentation demonstrates various architectural patterns for building agents, each with increasing complexity and capabilities. Whether you're building a simple chatbot or a complex multi-agent system, understanding these patterns will help you choose the right architecture for your use case.

## 📚 Presentation

The full presentation is available in [Agent Architect.pdf](./Agent%20Architect.pdf).

## 🎯 Notebooks

The repository includes 8 Jupyter notebooks that demonstrate different agent architectures:

### [1. Simple LLM Call](./1_llm_call.ipynb)
The foundation of all agents - a basic LLM call that takes a prompt and returns tokens. This notebook demonstrates the simplest form of an agent that cannot execute actions but provides intelligent responses based on the input prompt.

### [2.1 Using Tools](./2_1_tool_call.ipynb)
Learn how to bind tools/functions to your LLM. The model receives information about available tools and can request their execution. This notebook shows the manual process of handling tool calls and returning results to the LLM.

### [2.2 ReAct Agent](./2_2_react_agent.ipynb)
A simplified approach to tool-calling agents using the ReAct (Reasoning and Acting) pattern. This architecture streamlines the process shown in 2.1, automatically handling tool execution without manual intervention.

### [3. Routing](./3_routing.ipynb)
Demonstrates how to route requests to different specialized agents based on the input. This pattern allows you to create a system where different types of queries are handled by agents optimized for specific tasks.

### [4. Reflection](./4_reflection.ipynb)
Implements a reflection pattern where agents can review and critique their own outputs. This self-improvement mechanism helps generate higher-quality responses through iterative refinement.

### [5. Parallelism](./5_parallelism.ipynb)
Shows how to execute multiple agent tasks in parallel for improved efficiency. Learn how to manage state across parallel executions and combine results from multiple concurrent operations.

### [6. Supervisor](./6_supervisor.ipynb)
Introduces the supervisor pattern where a central agent delegates tasks to specialized worker agents. The supervisor coordinates multiple agents, each with specific tools and capabilities, to accomplish complex tasks.

### [7. Swarm](./7_swarm.ipynb)
Demonstrates a swarm architecture where agents can dynamically hand off tasks to each other. This pattern enables flexible agent collaboration with agents autonomously deciding when to transfer control to more suitable agents.

### [8. Planner](./8_planner.ipynb)
Implements a planning agent that breaks down complex tasks into smaller steps, executes them, and tracks progress. This architecture includes integration with the Tavily search tool for information retrieval.

## 🚀 Getting Started

### Prerequisites

- Python 3.9 or higher
- Azure OpenAI account (or OpenAI account)
- Tavily API key (for notebook 8)

### Installation

1. **Clone this repository**
   ```bash
   git clone <repository-url>
   cd agent-architectures
   ```

2. **Create a virtual environment** (recommended)
   ```bash
   python -m venv venv
   source venv/bin/activate  # On macOS/Linux
   # or
   venv\Scripts\activate  # On Windows
   ```

3. **Install dependencies**
   ```bash
   pip install -r requirements.txt
   ```

### Environment Setup

1. **Copy the environment template**
   ```bash
   cp .env-template .env
   ```

2. **Configure your API keys**
   
   Open the `.env` file and fill in your credentials:

   ```plaintext
   # Azure OpenAI Configuration
   OPENAI_API_VERSION=2024-02-15-preview
   AZURE_OPENAI_ENDPOINT=https://your-resource-name.openai.azure.com/
   AZURE_OPENAI_API_KEY=your-azure-openai-api-key

   # Tavily API Key (required for notebook 8_planner.ipynb)
   TAVILY_API_KEY=your-tavily-api-key
   ```

   **Where to get your API keys:**
   
   - **Azure OpenAI**: 
     - Sign up at [Azure Portal](https://portal.azure.com)
     - Create an Azure OpenAI resource
     - Navigate to "Keys and Endpoint" to find your endpoint and API key
     - The API version should match your Azure OpenAI deployment
   
   - **Tavily API**:
     - Sign up at [Tavily](https://tavily.com)
     - Get your API key from the dashboard
     - This is only needed if you plan to run notebook 8 (Planner)

3. **Load environment variables**
   
   The notebooks will automatically load the `.env` file using python-dotenv. Make sure your `.env` file is in the same directory as the notebooks.

### Running the Notebooks

1. **Launch Jupyter**
   ```bash
   jupyter notebook
   ```
   Or use VS Code with the Jupyter extension.

2. **Open any notebook** and run the cells sequentially.

3. **Start with notebook 1** (`1_llm_call.ipynb`) and progress through the notebooks in order for the best learning experience.

## 📦 Dependencies

The project uses the following main packages:

- `langchain-openai` - LangChain integration with OpenAI/Azure OpenAI
- `langgraph` - Framework for building stateful, multi-actor applications with LLMs
- `langchain` - Core LangChain framework
- `langgraph-supervisor` - Supervisor pattern implementation
- `langgraph-swarm` - Swarm pattern implementation
- `langchain-tavily` - Tavily search integration

See [requirements.txt](./requirements.txt) for the complete list with versions.

## 🤝 Contributing

Feel free to open issues or submit pull requests if you find any problems or have suggestions for improvements.

## 📄 License

This presentation uses a **dual license** structure:

- **Slides** (`.key`/`.pdf` files): [CC BY-NC-ND 4.0](../LICENSE-SLIDES.md) - Cannot be reused without attribution
- **Code** (notebooks, `.py` files): [MIT License](../LICENSE-CODE.md) - Free to use and modify

**© 2025 Sammy Deprez** | See [root LICENSE](../LICENSE-SLIDES.md) for details.

## � Acknowledgments

Built with [LangChain](https://langchain.com) and [LangGraph](https://langchain-ai.github.io/langgraph/).

---

**Happy Learning! 🎉**
