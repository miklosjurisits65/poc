using System;
using System.Collections.Generic;

namespace ivp.ai.api
{
    public sealed class DocumentRecognitionSessionPool
    {
        private static readonly Lazy<DocumentRecognitionSessionPool> _instance = 
            new Lazy<DocumentRecognitionSessionPool>(() => new DocumentRecognitionSessionPool());

        private readonly Dictionary<Guid, DocumentRecognitionSession> _sessions = 
            new Dictionary<Guid, DocumentRecognitionSession>();

        public static DocumentRecognitionSessionPool Instance => _instance.Value;

        public Dictionary<Guid, DocumentRecognitionSession> Sessions => _sessions;

        public void Add(Guid sessionId, DocumentRecognitionSession session)
        {
            _sessions[sessionId] = session;
        }

        public bool Remove(Guid sessionId)
        {
            return _sessions.Remove(sessionId);
        }

        public DocumentRecognitionSession Get(Guid sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }

        public bool Contains(Guid sessionId)
        {
            return _sessions.ContainsKey(sessionId);
        }
    }
}
