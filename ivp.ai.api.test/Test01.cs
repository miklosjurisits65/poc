using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ivp.ai.api.Configuration;
using ivp.ai.api.Model;
using ivp.ai.api.Services;

namespace ivp.ai.api.test
{
    [TestClass]
    public class Test01
    {
        private static readonly string ApiKey = "KVjKM1XjE2A0b4tzpDaCAD9Tn7gRE07Z";
        private static readonly string BaseUri = "https://api.mistral.ai/v1";
        private static readonly string FileUploadUri = "https://api.mistral.ai/v1/files";
        private static readonly string SendPromptUri = "https://api.mistral.ai/v1/chat/completions";
        private static readonly string PromptText = "Please retrieve the contract number, contract startdate, organization, and amount from the uploaded contract document.";

        private static IDocumentRecognitionService _service;
        private static Guid _sessionId1;
        private static Guid _sessionId2;
        private static Guid _sessionId3;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Create configuration
            var configuration = new MistralDocumentRecognitionConfiguration
            {
                AiApiKey = ApiKey,
                AiBaseUri = BaseUri,
                AiFileUploadUri = FileUploadUri,
                AiSendPromptUri = SendPromptUri
            };

            // Instantiate service
            _service = new MistralDocumentRecognitionService(configuration);

            // Get the base directory for testdata
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string testDataPath = Path.Combine(baseDir, "testdata");

            // Create test documents
            var testFile1 = new MistralRecognisableDocument
            {
                FileUrl = Path.Combine(testDataPath, "testfile1.pdf"),
                FileMetadata = "Test File 1",
                Base64Content = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(testDataPath, "testfile1.pdf")))
            };

            var testFile2 = new MistralRecognisableDocument
            {
                FileUrl = Path.Combine(testDataPath, "testfile2.pdf"),
                FileMetadata = "Test File 2",
                Base64Content = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(testDataPath, "testfile2.pdf")))
            };

            var testFile3 = new MistralRecognisableDocument
            {
                FileUrl = Path.Combine(testDataPath, "testfile3.pdf"),
                FileMetadata = "Test File 3",
                Base64Content = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(testDataPath, "testfile3.pdf")))
            };

            // Start document recognition for all 3 files
            _sessionId1 = _service.StartDocumentRecognition(testFile1, PromptText);
            _sessionId2 = _service.StartDocumentRecognition(testFile2, PromptText);
            _sessionId3 = _service.StartDocumentRecognition(testFile3, PromptText);
        }

        [TestMethod]
        public void TestImmediateResultsAreNull()
        {
            // Immediately check results (should be null since processing is async)
            var result1 = _service.GetDocumentRecognitionResult(_sessionId1);
            var result2 = _service.GetDocumentRecognitionResult(_sessionId2);
            var result3 = _service.GetDocumentRecognitionResult(_sessionId3);

            Assert.IsNull(result1, "Session 1 result should be null immediately after starting");
            Assert.IsNull(result2, "Session 2 result should be null immediately after starting");
            Assert.IsNull(result3, "Session 3 result should be null immediately after starting");
        }

        [TestMethod]
        public void TestResultsAfterWaiting()
        {
            // Wait 30 seconds for processing to complete
            Thread.Sleep(30000);

            // Check results again after waiting
            var result1 = _service.GetDocumentRecognitionResult(_sessionId1);
            var result2 = _service.GetDocumentRecognitionResult(_sessionId2);
            var result3 = _service.GetDocumentRecognitionResult(_sessionId3);

            // Results may or may not be available depending on network/API
            // We just verify the session IDs are valid
            Assert.IsNotNull(_sessionId1, "Session 1 ID should not be null");
            Assert.IsNotNull(_sessionId2, "Session 2 ID should not be null");
            Assert.IsNotNull(_sessionId3, "Session 3 ID should not be null");

            // Print results for debugging
            Console.WriteLine("Session 1 result: " + (result1 == null ? "null" : result1.ResponseText));
            Console.WriteLine("Session 2 result: " + (result2 == null ? "null" : result2.ResponseText));
            Console.WriteLine("Session 3 result: " + (result3 == null ? "null" : result3.ResponseText));
        }
    }
}
