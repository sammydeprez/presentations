# Agent Architectures - Microsoft Agent Framework

This folder contains implementations of AI agent architecture patterns using **Microsoft Semantic Kernel** (the Microsoft Agent Framework), available in both **Python** and **C# polyglot notebooks**.

## 📖 Overview

This is a companion to the [original LangChain/LangGraph agent architectures](../agent-architectures/) presentation. The notebooks demonstrate the same architectural patterns but using Microsoft's Semantic Kernel framework instead of LangChain.

Microsoft Semantic Kernel is Microsoft's open-source SDK for integrating AI capabilities into applications. It provides similar functionality to LangChain but with a Microsoft-native approach and excellent support for both Python and .NET languages.

## 🎯 Notebooks

Each pattern is implemented in both Python (`.ipynb`) and C# (`.dib` - polyglot notebooks):

### [1. Simple LLM Call](./python/1_llm_call.ipynb) | [C# Version](./csharp/1_llm_call.dib)
The foundation - a basic LLM call using Semantic Kernel's chat completion services.

### [2.1 Using Tools](./python/2_1_tool_call.ipynb) | [C# Version](./csharp/2_1_tool_call.dib)
Learn how to bind tools/functions to your LLM using Semantic Kernel plugins.

### [2.2 ReAct Agent](./python/2_2_react_agent.ipynb) | [C# Version](./csharp/2_2_react_agent.dib)
Automatic tool execution using Semantic Kernel's automatic function calling (ReAct pattern).

### [3. Routing](./python/3_routing.ipynb) | [C# Version](./csharp/3_routing.dib)
Route requests to different specialized agents based on input.

### [4. Reflection](./python/4_reflection.ipynb) | [C# Version](./csharp/4_reflection.dib)
Agents review and critique their own outputs for iterative improvement.

### [5. Parallelism](./python/5_parallelism.ipynb) | [C# Version](./csharp/5_parallelism.dib)
Execute multiple agent tasks in parallel for improved efficiency.

### [6. Supervisor](./python/6_supervisor.ipynb) | [C# Version](./csharp/6_supervisor.dib)
Central agent delegates tasks to specialized worker agents.

### [7. Swarm](./python/7_swarm.ipynb) | [C# Version](./csharp/7_swarm.dib)
Agents dynamically hand off tasks to each other.

### [8. Planner](./python/8_planner.ipynb) | [C# Version](./csharp/8_planner.dib)
Planning agent that breaks down complex tasks into steps.

## 🚀 Getting Started

### Prerequisites

**For Python notebooks:**
- Python 3.10 or higher
- Azure OpenAI account

**For C# polyglot notebooks:**
- .NET 8.0 SDK or higher
- Visual Studio Code with [Polyglot Notebooks extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)
- Azure OpenAI account

### Installation - Python

1. **Navigate to this directory**
   ```bash
   cd agent-architectures-maf
   ```

2. **Create a virtual environment**
   ```bash
   python -m venv venv
   source venv/bin/activate  # On macOS/Linux
   # or
   venv\Scripts\activate  # On Windows
   ```

3. **Install Python dependencies**
   ```bash
   pip install -r python/requirements.txt
   ```

4. **Set up environment variables**
   ```bash
   cp .env-template .env
   # Edit .env with your Azure OpenAI credentials
   ```

5. **Launch Jupyter**
   ```bash
   jupyter notebook python/
   ```

### Installation - C# Polyglot Notebooks

1. **Install .NET SDK**
   - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

2. **Install VS Code and Polyglot Notebooks extension**
   - Install [Visual Studio Code](https://code.visualstudio.com/)
   - Install the [Polyglot Notebooks extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)

3. **Set up environment variables**
   ```bash
   cp .env-template .env
   # Edit .env with your Azure OpenAI credentials
   ```

4. **Open C# notebooks**
   - Open VS Code in this directory
   - Navigate to `csharp/` folder
   - Open any `.dib` file
   - Run cells using the Polyglot Notebooks extension

## 📦 Environment Setup

Copy the `.env-template` file to `.env` and fill in your credentials:

```plaintext
# Azure OpenAI Configuration
AZURE_OPENAI_ENDPOINT=https://your-resource-name.openai.azure.com/
AZURE_OPENAI_API_KEY=your-azure-openai-api-key
OPENAI_API_VERSION=2024-02-15-preview
```

**Where to get your API keys:**

- **Azure OpenAI**: 
  - Sign up at [Azure Portal](https://portal.azure.com)
  - Create an Azure OpenAI resource
  - Navigate to "Keys and Endpoint" to find your endpoint and API key

## 📚 Key Differences from LangChain

While the architectural patterns are the same, the implementation uses Microsoft Semantic Kernel:

| LangChain | Semantic Kernel |
|-----------|----------------|
| `ChatOpenAI` | `AzureChatCompletion` |
| `@tool` decorator | `@kernel_function` decorator / `[KernelFunction]` attribute |
| `bind_tools()` | Kernel plugins with automatic function calling |
| `create_react_agent()` | `FunctionChoiceBehavior.Auto()` |
| Message history | `ChatHistory` |
| Tool calling | Plugin functions |

## 🔑 Dependencies

**Python:**
- `semantic-kernel` - Microsoft's SDK for AI orchestration
- `python-dotenv` - Environment variable management

**C#:**
- `Microsoft.SemanticKernel` - Core Semantic Kernel package
- `DotNetEnv` - Environment variable management

See [python/requirements.txt](./python/requirements.txt) for Python package versions.

## 💡 Learning Path

1. **Start with Python or C#** based on your preference
2. **Follow the notebooks in order** (1 through 8)
3. **Compare with the LangChain versions** in the parent folder to understand different approaches
4. **Experiment** by modifying the examples

## 🤝 Contributing

Feel free to open issues or submit pull requests if you find any problems or have suggestions for improvements.

## 📄 License

This presentation uses a **dual license** structure:

- **Code** (notebooks, `.py` files, `.dib` files): [MIT License](../LICENSE-CODE.md) - Free to use and modify

**© 2025 Sammy Deprez** | See [root LICENSE](../LICENSE-CODE.md) for details.

## 🙏 Acknowledgments

Built with:
- [Microsoft Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/) - Microsoft's AI orchestration SDK
- [Azure OpenAI](https://azure.microsoft.com/en-us/products/ai-services/openai-service) - Enterprise AI services
- Inspired by [LangChain](https://langchain.com) and [LangGraph](https://langchain-ai.github.io/langgraph/)

## 📚 Additional Resources

- [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Semantic Kernel GitHub](https://github.com/microsoft/semantic-kernel)
- [Azure OpenAI Documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/)
- [Polyglot Notebooks](https://github.com/dotnet/interactive)

---

**Happy Learning! 🎉**
