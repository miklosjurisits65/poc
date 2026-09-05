using System;
using ivp.ai.api.Model;

namespace ivp.ai.api
{
    public class DocumentRecognitionSession
    {
        public IRecognisableDocument RecognisableDocument { get; set; }
        public string PromptText { get; set; }
        public IDocumentRecognitionResult DocumentRecognitionResult { get; set; }
    }
}
