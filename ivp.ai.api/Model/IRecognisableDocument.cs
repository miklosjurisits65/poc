namespace ivp.ai.api.Model
{
    public interface IRecognisableDocument
    {
        string FileMetadata { get; set; }
        string FileUrl { get; set; }
        string Base64Content { get; set; }
    }
}
