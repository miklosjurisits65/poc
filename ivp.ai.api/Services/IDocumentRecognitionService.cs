using System;
using ivp.ai.api.Model;

namespace ivp.ai.api.Services
{
    public interface IDocumentRecognitionService
    {
        Guid StartDocumentRecognition(IRecognisableDocument recognisableDocument, string promptText);
        IDocumentRecognitionResult GetDocumentRecognitionResult(Guid sessionId);
    }
}
