# AI Development Presentations

Welcome to this collection of technical presentations on AI agent development and LLM safety. These materials provide hands-on demonstrations and practical examples for building intelligent, safe, and responsible AI applications using LangChain, LangGraph, and Azure AI services.

## 📚 Presentations

This repository contains multiple comprehensive presentations with accompanying notebooks:

### 🤖 [Agent Architectures](./agent-architectures/)

**Explore different AI agent patterns from simple to sophisticated.**

Learn how to build intelligent agents using LangChain and LangGraph, progressing from basic LLM calls to complex multi-agent systems. This presentation covers 8 different architectural patterns including:

- Simple LLM calls and tool usage
- ReAct agents for autonomous tool execution
- Routing patterns for specialized agents
- Reflection for self-improvement
- Parallel execution strategies
- Supervisor and Swarm architectures
- Planning agents with task decomposition

**[📖 View Full Documentation →](./agent-architectures/README.md)**

**Technologies**: LangChain, LangGraph, Azure OpenAI, Tavily Search

---

### 🤖 [Agent Architectures - Microsoft Agent Framework](./agent-architectures-maf/)

**Same agent patterns, Microsoft Semantic Kernel implementation.**

This is a companion to the LangChain presentation above, implementing the same 8 architectural patterns using **Microsoft Semantic Kernel** (the Microsoft Agent Framework). Available in both **Python** and **C# polyglot notebooks**.

**Key Features:**
- Python and C# implementations side-by-side
- Uses Microsoft Semantic Kernel instead of LangChain
- Same architectural patterns for easy comparison
- Polyglot notebooks for interactive C# development

**[📖 View Full Documentation →](./agent-architectures-maf/README.md)**

**Technologies**: Microsoft Semantic Kernel, Azure OpenAI, .NET, Python

---

### 🛡️ [LLM Safety & Security](./llm-safety-security/)

**⚠️ Contains explicit content for educational purposes**

Master the critical aspects of LLM safety using Azure Content Safety services. This presentation demonstrates comprehensive techniques for protecting AI applications from various security risks and harmful content.

Topics covered include:
- Prompt injection detection and prevention
- Content filtering across multiple categories (Hate, Violence, Sexual, SelfHarm)
- Custom blocklists and pattern matching
- Image content analysis
- Hallucination detection (Groundedness)
- Copyright and protected material detection

**[📖 View Full Documentation →](./llm-safety-security/README.md)**

**Technologies**: Azure Content Safety, Azure OpenAI, LangChain, Ollama

**⚠️ Warning**: This presentation contains provocative examples and explicit content designed to test safety features. All content is for educational purposes only.

---

## 🚀 Quick Start

Each presentation is self-contained with its own setup instructions. To get started:

1. **Choose a presentation** based on your interest
2. **Navigate to the folder** (`agent-architectures/` or `llm-safety-security/`)
3. **Follow the README** in that folder for detailed setup instructions
4. **Configure your environment** using the provided `.env-template`
5. **Run the notebooks** to explore the concepts

### Common Prerequisites

Both presentations require:
- Python 3.8 or higher
- Azure OpenAI account
- Basic understanding of Python and LLMs

### Installation Pattern

Each presentation follows this pattern:

```bash
# Navigate to the presentation folder
cd agent-architectures  # or llm-safety-security

# Create virtual environment
python -m venv venv
source venv/bin/activate  # macOS/Linux
# or venv\Scripts\activate  # Windows

# Install dependencies
pip install -r requirements.txt

# Configure environment
cp .env-template .env
# Edit .env with your API keys

# Launch Jupyter
jupyter notebook
```

## 📖 Learning Path

### For Beginners
Start with **Agent Architectures** to understand the fundamentals:
1. Begin with notebook 1 (Simple LLM Call)
2. Progress through each pattern sequentially
3. Experiment with the examples
4. Then move to LLM Safety & Security to learn about protecting your applications

### For Experienced Developers
You can explore either presentation based on your needs:
- **Agent Architectures**: If you want to build sophisticated multi-agent systems
- **LLM Safety & Security**: If you need to implement content moderation and safety features

### For Production Deployments
We recommend completing both presentations to understand:
1. How to architect effective AI agents (Agent Architectures)
2. How to secure and protect them (LLM Safety & Security)

## 🎯 What You'll Learn

### From Agent Architectures
- Foundation of agent-based systems
- When to use different architectural patterns
- How to implement tool-calling agents
- Building multi-agent collaborative systems
- Task planning and decomposition strategies

### From LLM Safety & Security
- Protecting against prompt injection attacks
- Implementing content moderation
- Detecting harmful images and text
- Preventing hallucinations with grounding
- Avoiding copyright violations
- Building responsible AI systems

## 🛠️ Technologies Used

### Core Frameworks
- **LangChain** - Framework for developing LLM applications
- **LangGraph** - Building stateful, multi-actor applications
- **Microsoft Semantic Kernel** - Microsoft's AI orchestration SDK
- **Azure OpenAI** - Enterprise-grade LLM access
- **Azure Content Safety** - Comprehensive safety services

### Additional Tools
- **Tavily Search** - Web search for AI agents
- **Ollama** - Local LLM execution (optional)
- **Jupyter** - Interactive notebooks
- **Polyglot Notebooks** - Interactive C# notebooks
- **httpx** - Modern HTTP client

## 📁 Repository Structure

```
presentations/
├── README.md                          # This file
├── agent-architectures/               # Agent patterns with LangChain
│   ├── README.md                      # Detailed documentation
│   ├── .env-template                  # Environment configuration template
│   ├── requirements.txt               # Python dependencies
│   ├── Agent Architect.key            # Keynote presentation
│   ├── 1_llm_call.ipynb              # Simple LLM call
│   ├── 2_1_tool_call.ipynb           # Manual tool calling
│   ├── 2_2_react_agent.ipynb         # ReAct agent pattern
│   ├── 3_routing.ipynb               # Routing pattern
│   ├── 4_reflection.ipynb            # Reflection pattern
│   ├── 5_parallelism.ipynb           # Parallel execution
│   ├── 6_supervisor.ipynb            # Supervisor pattern
│   ├── 7_swarm.ipynb                 # Swarm pattern
│   ├── 8_planner.ipynb               # Planning agent
│   └── docs/                          # Supporting images
│
├── agent-architectures-maf/           # Agent patterns with MS Semantic Kernel
│   ├── README.md                      # Detailed documentation
│   ├── .env-template                  # Environment configuration template
│   ├── python/                        # Python notebooks
│   │   ├── requirements.txt           # Python dependencies
│   │   ├── 1_llm_call.ipynb          # Through 8_planner.ipynb
│   │   └── ...                        # All 8 notebooks in Python
│   └── csharp/                        # C# polyglot notebooks
│       ├── 1_llm_call.dib            # Through 8_planner.dib
│       └── ...                        # All 8 notebooks in C#
│
└── llm-safety-security/               # Safety & security presentation
    ├── README.md                      # Detailed documentation
    ├── .env-template                  # Environment configuration template
    ├── requirements.txt               # Python dependencies
    ├── LLM Safety.key                 # Keynote presentation
    └── content-safety.ipynb           # Comprehensive safety demo
```

## 🔑 Required API Keys

### For Agent Architectures (both LangChain and Semantic Kernel versions)
- **Azure OpenAI**: Endpoint, API Key, and API Version
- **Tavily API**: Key for web search (notebook 8 only, LangChain version)

### For LLM Safety & Security
- **Azure OpenAI**: Endpoint, API Key, and API Version
- **Azure Content Safety**: Endpoint and Subscription Key

See individual presentation READMEs for detailed instructions on obtaining these keys.

## 💡 Best Practices

When working with these presentations:

1. **Start Simple**: Don't skip the basics. Each concept builds on previous ones.
2. **Experiment**: Modify the examples to understand how they work.
3. **Use Virtual Environments**: Keep dependencies isolated per presentation.
4. **Protect Your Keys**: Never commit `.env` files to version control.
5. **Monitor Costs**: Azure OpenAI charges per token. Be mindful when experimenting.
6. **Review Documentation**: Each README contains important setup and usage information.

## 🤝 Contributing

Contributions are welcome! If you find issues or have suggestions:

1. Open an issue describing the problem or enhancement
2. Submit a pull request with your changes
3. Ensure your code follows the existing style
4. Update documentation as needed

## 📄 License

This repository uses a **dual license** structure:

### 📊 Presentation Slides
**[Creative Commons BY-NC-ND 4.0](./LICENSE-SLIDES.md)**

The presentation materials (`.key`, `.pdf`, `.pptx` files and slide diagrams) are licensed under CC BY-NC-ND 4.0:
- ✅ Share with attribution
- ❌ No commercial use
- ❌ No derivatives/modifications
- ❌ **Cannot be reused without proper attribution to Sammy Deprez**

### 💻 Code & Notebooks
**[MIT License](./LICENSE-CODE.md)**

All code, Jupyter notebooks, Python files, and configuration files are licensed under the MIT License:
- ✅ Free to use, modify, and distribute
- ✅ Commercial use allowed
- ✅ Must include copyright notice

---

**© 2025 Sammy Deprez**

When referencing these presentations, please cite:
- **Author**: Sammy Deprez
- **Repository**: https://github.com/sammydeprez/presentations
- **Year**: 2025

For permissions beyond the license scope, please open an issue.

## 🙏 Acknowledgments

These presentations are built with:
- [LangChain](https://langchain.com) - LLM application framework
- [LangGraph](https://langchain-ai.github.io/langgraph/) - Agent orchestration
- [Azure AI Services](https://azure.microsoft.com/en-us/products/ai-services/) - Enterprise AI capabilities
- [Tavily](https://tavily.com) - AI-optimized search

## 📚 Additional Resources

### Learning Resources
- [LangChain Documentation](https://python.langchain.com/)
- [LangGraph Documentation](https://langchain-ai.github.io/langgraph/)
- [Azure OpenAI Documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/)
- [Azure Content Safety Documentation](https://learn.microsoft.com/en-us/azure/ai-services/content-safety/)

### Community
- [LangChain Discord](https://discord.gg/langchain)
- [LangChain GitHub](https://github.com/langchain-ai/langchain)
- [Azure AI Community](https://techcommunity.microsoft.com/category/ai-machine-learning)

### Responsible AI
- [Microsoft Responsible AI Principles](https://www.microsoft.com/en-us/ai/responsible-ai)
- [OpenAI Usage Policies](https://openai.com/policies/usage-policies)

---

**Happy Learning! 🎉**

*These presentations are designed for educational purposes to help developers build intelligent and responsible AI applications.*
