using ivp.ai.api.Configuration;

namespace ivp.ai.api.Configuration
{
    public class MistralDocumentRecognitionConfiguration : IDocumentRecognitionConfiguration
    {
        public string AiApiKey { get; set; }
        public string AiBaseUri { get; set; } = "https://api.mistral.ai";
        public string AiFileUploadUri { get; set; } = "https://api.mistral.ai/v1/files";
        public string AiSendPromptUri { get; set; } = "https://api.mistral.ai/v1/chat/completions";
    }
}
