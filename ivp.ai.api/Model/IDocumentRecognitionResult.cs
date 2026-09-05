namespace ivp.ai.api.Model
{
    public interface IDocumentRecognitionResult
    {
        string SessionId { get; set; }
        string FileId { get; set; }
        string ResponseText { get; set; }
        int UsedTokens { get; set; }
    }
}
