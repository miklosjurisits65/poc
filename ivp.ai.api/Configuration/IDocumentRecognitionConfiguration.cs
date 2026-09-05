namespace ivp.ai.api.Configuration
{
    public interface IDocumentRecognitionConfiguration
    {
        string AiApiKey { get; set; }
        string AiBaseUri { get; set; }
        string AiFileUploadUri { get; set; }
        string AiSendPromptUri { get; set; }
    }
}
