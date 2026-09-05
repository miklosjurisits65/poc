using System;
using System.IO;
using System.Threading;
using ivp.ai.api.Configuration;
using ivp.ai.api.Model;
using ivp.ai.api.Services;

namespace ivp.ai.api
{
    public class Test01
    {
        public static void Run()
        {
            // Create configuration
            var configuration = new MistralDocumentRecognitionConfiguration
            {
                AiApiKey = "KVjKM1XjE2A0b4tzpDaCAD9Tn7gRE07Z",
                AiBaseUri = "https://api.mistral.ai/v1",
                AiFileUploadUri = "https://api.mistral.ai/v1/files",
                AiSendPromptUri = "https://api.mistral.ai/v1/chat/completions"
            };

            // Instantiate service
            var service = new MistralDocumentRecognitionService(configuration);

            // Prompt text
            string promptText = "Please retrieve the contract number, contract startdate, organization, and amount from the uploaded contract document.";

            // Create test documents
            var testFile1 = new MistralRecognisableDocument
            {
                FileUrl = Path.Combine("testdata", "testfile1.pdf"),
                FileMetadata = "Test File 1",
                Base64Content = Convert.ToBase64String(File.ReadAllBytes(Path.Combine("testdata", "testfile1.pdf")))
            };

            var testFile2 = new MistralRecognisableDocument
            {
                FileUrl = Path.Combine("testdata", "testfile2.pdf"),
                FileMetadata = "Test File 2",
                Base64Content = Convert.ToBase64String(File.ReadAllBytes(Path.Combine("testdata", "testfile2.pdf")))
            };

            var testFile3 = new MistralRecognisableDocument
            {
                FileUrl = Path.Combine("testdata", "testfile3.pdf"),
                FileMetadata = "Test File 3",
                Base64Content = Convert.ToBase64String(File.ReadAllBytes(Path.Combine("testdata", "testfile3.pdf")))
            };

            // Start document recognition for all 3 files
            var sessionId1 = service.StartDocumentRecognition(testFile1, promptText);
            var sessionId2 = service.StartDocumentRecognition(testFile2, promptText);
            var sessionId3 = service.StartDocumentRecognition(testFile3, promptText);

            Console.WriteLine("Started document recognition for 3 files:");
            Console.WriteLine("Session 1: " + sessionId1);
            Console.WriteLine("Session 2: " + sessionId2);
            Console.WriteLine("Session 3: " + sessionId3);

            // Immediately check results (will most likely be null)
            Console.WriteLine("\nImmediate results:");
            var result1 = service.GetDocumentRecognitionResult(sessionId1);
            var result2 = service.GetDocumentRecognitionResult(sessionId2);
            var result3 = service.GetDocumentRecognitionResult(sessionId3);

            Console.WriteLine("Session 1 result: " + (result1 == null ? "null" : result1.ResponseText));
            Console.WriteLine("Session 2 result: " + (result2 == null ? "null" : result2.ResponseText));
            Console.WriteLine("Session 3 result: " + (result3 == null ? "null" : result3.ResponseText));

            // Wait 30 seconds
            Console.WriteLine("\nWaiting 30 seconds...");
            Thread.Sleep(30000);

            // Check results again after waiting
            Console.WriteLine("\nResults after 30 seconds:");
            result1 = service.GetDocumentRecognitionResult(sessionId1);
            result2 = service.GetDocumentRecognitionResult(sessionId2);
            result3 = service.GetDocumentRecognitionResult(sessionId3);

            Console.WriteLine("Session 1 result: " + (result1 == null ? "null" : result1.ResponseText));
            Console.WriteLine("Session 2 result: " + (result2 == null ? "null" : result2.ResponseText));
            Console.WriteLine("Session 3 result: " + (result3 == null ? "null" : result3.ResponseText));
        }
    }
}
