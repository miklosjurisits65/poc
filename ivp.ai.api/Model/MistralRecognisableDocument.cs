using ivp.ai.api.Model;

namespace ivp.ai.api.Model
{
    public class MistralRecognisableDocument : IRecognisableDocument
    {
        public string FileMetadata { get; set; }
        public string FileUrl { get; set; }
        public string Base64Content { get; set; }
    }
}
