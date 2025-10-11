# LLM Safety & Security

This repository contains a comprehensive presentation on LLM safety and security using Azure Content Safety services. The materials demonstrate various techniques for protecting AI applications from harmful content, prompt injections, and other security risks.

## ⚠️ CONTENT WARNING

**This presentation contains explicit examples designed to test and demonstrate content safety features, including:**
- Provocative and potentially offensive language
- Examples of prompt injection attacks
- Test cases involving hate speech, violence, and self-harm scenarios
- Explicit images used for image analysis demonstrations

**These materials are for educational purposes only** and are intended to demonstrate how content safety systems detect and prevent harmful content. All examples are used exclusively to test and showcase security features.

**Please proceed only if you understand and accept this disclaimer.**

---

## 📖 Overview

As Large Language Models (LLMs) become increasingly integrated into applications, ensuring their safe and responsible use is critical. This presentation covers Azure Content Safety services, which provide comprehensive tools for detecting and preventing harmful content, prompt injections, hallucinations, and copyright violations.

Whether you're building chatbots, content moderation systems, or AI-powered applications, understanding these safety mechanisms is essential for creating trustworthy and compliant AI solutions.

## 📚 Presentation

The full presentation is available in [LLM Safety.pdf](./LLM%20Safety.pdf).

## 🎯 Notebook Content

### [Content Safety Notebook](./content-safety.ipynb)

This comprehensive notebook covers all major aspects of Azure Content Safety:

#### 1. **Prompt Shield**
Protect your LLM applications from prompt injection attacks. Learn how to detect:
- Direct jailbreak attempts where users try to override system instructions
- Indirect attacks embedded in documents or emails
- Examples of bypassing safety guardrails

#### 2. **Text Analytics**
Analyze text content for harmful material across multiple categories:
- **Built-in Categories**: Hate, SelfHarm, Sexual, Violence
- **Severity Levels**: Configure detection with 4 or 8 severity levels
- **Custom Thresholds**: Set appropriate sensitivity for your use case
- **Real-time Detection**: Analyze user inputs before processing

#### 2.1 **Blocklists**
Create and manage custom blocklists to filter specific content:
- Block specific words, phrases, or competitor names
- Use regular expressions for pattern matching (e.g., email addresses)
- Combine blocklists with category detection
- Dynamically update blocklists without code changes

#### 2.2 **Custom Categories**
Train custom content categories beyond the built-in ones:
- Define domain-specific safety requirements
- Create industry-specific content filters
- Build custom classifiers for your use case

#### 3. **Image Analysis**
Detect harmful visual content in images:
- Analyze images from URLs or base64-encoded data
- Detect Violence, Hate, Sexual, and SelfHarm content in images
- Configure category-specific detection
- Integrate with multimodal LLM applications

#### 4. **Grounding Detection**
Prevent hallucinations and ensure factual accuracy:
- Detect when LLM outputs are not grounded in source materials
- Identify ungrounded claims and get percentage metrics
- Get detailed reasoning with GPT-4o integration
- Receive suggested corrections for ungrounded text

#### 5. **Protected Material Detection**
Identify copyrighted or protected content:
- **Text**: Detect song lyrics, book excerpts, and other copyrighted text
- **Code**: Identify protected code snippets and copyrighted implementations
- Prevent inadvertent copyright violations
- Ensure compliance with intellectual property laws

#### 6. **Extra: Structured Output**
Bonus content on ensuring type-safe LLM responses:
- Use Pydantic models for structured outputs
- Ensure consistent response formats
- Reduce parsing errors with schema validation

---

## 🚀 Getting Started

### Prerequisites

- Python 3.9 or higher
- Azure OpenAI account
- Azure Content Safety resource
- (Optional) Local Ollama installation for testing with open-source models

### Installation

1. **Clone this repository**
   ```bash
   git clone <repository-url>
   cd llm-safety-security
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

4. **(Optional) Install Ollama**
   
   If you want to test with the local Gemma model:
   ```bash
   # Install Ollama from https://ollama.ai
   # Then pull the Gemma model
   ollama pull gemma3:1b
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

   # Azure Content Safety Configuration
   CONTENT_SAFETY_ENDPOINT=https://your-content-safety-resource.cognitiveservices.azure.com/
   CONTENT_SAFETY_SUBSCRIPTION_KEY=your-content-safety-subscription-key
   ```

   **Where to get your API keys:**
   
   - **Azure OpenAI**: 
     - Sign up at [Azure Portal](https://portal.azure.com)
     - Create an Azure OpenAI resource
     - Navigate to "Keys and Endpoint" to find your endpoint and API key
     - The API version should match your Azure OpenAI deployment
   
   - **Azure Content Safety**:
     - In the [Azure Portal](https://portal.azure.com), create a "Content Safety" resource
     - After deployment, go to "Keys and Endpoint"
     - Copy the endpoint URL and one of the subscription keys
     - This service is required for all content safety features in the notebook

3. **Load environment variables**
   
   The notebook will automatically load the `.env` file using python-dotenv. Make sure your `.env` file is in the same directory as the notebook.

### Running the Notebook

1. **Launch Jupyter**
   ```bash
   jupyter notebook
   ```
   Or use VS Code with the Jupyter extension.

2. **Open the notebook** (`content-safety.ipynb`) and run the cells sequentially.

3. **Note**: Some cells demonstrate harmful content detection. These examples are intentionally provocative to showcase the safety features.

## 🛡️ Key Features Demonstrated

- **Prompt Injection Detection**: Shield your applications from jailbreak attempts
- **Multi-Category Content Filtering**: Detect Hate, Violence, Sexual, and SelfHarm content
- **Custom Blocklists**: Filter specific terms, patterns, and competitor names
- **Image Safety**: Analyze visual content for harmful material
- **Hallucination Prevention**: Verify LLM outputs are grounded in source data
- **Copyright Protection**: Detect protected text and code
- **Severity Tuning**: Configure detection sensitivity for your needs
- **Real-time Protection**: Integrate safety checks into production workflows

## 📦 Dependencies

The project uses the following main packages:

- `httpx` - Modern HTTP client for API calls to Azure Content Safety
- `langchain-openai` - LangChain integration with Azure OpenAI
- `langchain-ollama` - LangChain integration with local Ollama models

See [requirements.txt](./requirements.txt) for the complete list with versions.

## 🏗️ Azure Resources Required

To run all examples in this notebook, you need:

1. **Azure OpenAI Service** - For LLM inference
   - Deploy at least one GPT model (e.g., GPT-4o-mini)
   - Optionally: Deploy a model with custom content filters disabled for comparison

2. **Azure Content Safety** - For all content safety features
   - Text analysis
   - Image analysis
   - Prompt Shield
   - Groundedness detection
   - Protected material detection

## 🔒 Best Practices

When implementing content safety in production:

1. **Layer Your Defenses**: Use multiple safety features (prompt shield + text analysis + grounding)
2. **Tune Thresholds**: Adjust severity levels based on your use case
3. **Monitor Continuously**: Track safety violations and adjust filters
4. **Test Thoroughly**: Use diverse test cases including edge cases
5. **Respect Privacy**: Be cautious with user data in safety logs
6. **Stay Updated**: Keep up with new attack vectors and safety features

## 🤝 Contributing

Feel free to open issues or submit pull requests if you find any problems or have suggestions for improvements.

## 🙏 Acknowledgments

Built with [Azure Content Safety](https://azure.microsoft.com/en-us/products/ai-services/ai-content-safety) and [LangChain](https://langchain.com).

## 📚 Additional Resources

- [Azure Content Safety Documentation](https://learn.microsoft.com/en-us/azure/ai-services/content-safety/)
- [Prompt Shield Documentation](https://learn.microsoft.com/en-us/azure/ai-services/content-safety/quickstart-jailbreak)
- [Azure OpenAI Content Filtering](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/content-filter)
- [Responsible AI Principles](https://www.microsoft.com/en-us/ai/responsible-ai)

---

**Remember**: These tools are for building safer AI systems. Always use them responsibly and ethically. 🛡️

---

**Happy Learning! 🎉**
