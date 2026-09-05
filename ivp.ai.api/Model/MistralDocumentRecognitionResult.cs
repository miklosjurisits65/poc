using ivp.ai.api.Model;

namespace ivp.ai.api.Model
{
    public class MistralDocumentRecognitionResult : IDocumentRecognitionResult
    {
        public string SessionId { get; set; }
        public string FileId { get; set; }
        public string ResponseText { get; set; }
        public int UsedTokens { get; set; }
    }
}
