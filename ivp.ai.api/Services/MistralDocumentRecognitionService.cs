using System;
using System.IO;
using System.Net;
using System.Text;
using ivp.ai.api.Configuration;
using ivp.ai.api.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ivp.ai.api.Services
{
    public class MistralDocumentRecognitionService : IDocumentRecognitionService
    {
        private readonly IDocumentRecognitionConfiguration _configuration;

        public MistralDocumentRecognitionService(IDocumentRecognitionConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Guid StartDocumentRecognition(IRecognisableDocument recognisableDocument, string promptText)
        {
            var sessionId = Guid.NewGuid();
            var session = new DocumentRecognitionSession
            {
                RecognisableDocument = recognisableDocument,
                PromptText = promptText
            };

            DocumentRecognitionSessionPool.Instance.Add(sessionId, session);

            // Start async processing (fire and forget for this example)
            // In a real implementation, you might want to use Task.Run or similar
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    var fileUploadResult = await FileUpload(recognisableDocument);
                    if (fileUploadResult != null && !string.IsNullOrEmpty(fileUploadResult.FileId))
                    {
                        var sendPromptResult = await SendPrompt(fileUploadResult.FileId, promptText);
                        session.DocumentRecognitionResult = sendPromptResult;
                    }
                }
                catch (Exception ex)
                {
                    // Handle error - in production you might want to log this
                    session.DocumentRecognitionResult = new MistralDocumentRecognitionResult
                    {
                        SessionId = sessionId.ToString(),
                        FileId = recognisableDocument.FileUrl,
                        ResponseText = "Error: " + ex.Message,
                        UsedTokens = 0
                    };
                }
            });

            return sessionId;
        }

        public IDocumentRecognitionResult GetDocumentRecognitionResult(Guid sessionId)
        {
            var session = DocumentRecognitionSessionPool.Instance.Get(sessionId);
            return session?.DocumentRecognitionResult;
        }

        private async System.Threading.Tasks.Task<MistralDocumentRecognitionResult> FileUpload(IRecognisableDocument recognisableDocument)
        {
            try
            {
                var boundary = "----WebKitFormBoundary" + Guid.NewGuid().ToString();
                var request = WebRequest.Create(_configuration.AiFileUploadUri) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("Authorization", "Bearer " + _configuration.AiApiKey);
                request.ContentType = "multipart/form-data; boundary=" + boundary;

                using (var requestStream = request.GetRequestStream())
                {
                    // Write file part
                    var fileBytes = Convert.FromBase64String(recognisableDocument.Base64Content);
                    string fileName = Path.GetFileName(recognisableDocument.FileUrl);

                    var fileHeader = string.Format(
                        "--{0}\r\n" +
                        "Content-Disposition: form-data; name=\"file\"; filename=\"{1}\"\r\n" +
                        "Content-Type: application/pdf\r\n\r\n",
                        boundary, fileName);

                    var fileHeaderBytes = Encoding.UTF8.GetBytes(fileHeader);
                    requestStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                    requestStream.Write(Encoding.UTF8.GetBytes("\r\n"), 0, 2);

                    // Write purpose part
                    var purposeHeader = string.Format(
                        "--{0}\r\n" +
                        "Content-Disposition: form-data; name=\"purpose\"\r\n\r\n" +
                        "ocr\r\n",
                        boundary);

                    var purposeHeaderBytes = Encoding.UTF8.GetBytes(purposeHeader);
                    requestStream.Write(purposeHeaderBytes, 0, purposeHeaderBytes.Length);

                    // End boundary
                    var footer = string.Format("--{0}--\r\n", boundary);
                    var footerBytes = Encoding.UTF8.GetBytes(footer);
                    requestStream.Write(footerBytes, 0, footerBytes.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    var responseText = reader.ReadToEnd();
                    var jsonResponse = JObject.Parse(responseText);

                    return new MistralDocumentRecognitionResult
                    {
                        SessionId = Guid.NewGuid().ToString(),
                        FileId = jsonResponse["id"]?.ToString(),
                        ResponseText = responseText,
                        UsedTokens = 0 // Tokens not used in file upload
                    };
                }
            }
            catch (WebException webEx)
            {
                string responseText = string.Empty;
                if (webEx.Response != null)
                {
                    try
                    {
                        using (var stream = webEx.Response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            responseText = reader.ReadToEnd();
                        }
                    }
                    catch { }
                }
                return new MistralDocumentRecognitionResult
                {
                    SessionId = Guid.NewGuid().ToString(),
                    FileId = recognisableDocument.FileUrl,
                    ResponseText = "File upload error: " + webEx.Message + " Response: " + responseText,
                    UsedTokens = 0
                };
            }
            catch (Exception ex)
            {
                return new MistralDocumentRecognitionResult
                {
                    SessionId = Guid.NewGuid().ToString(),
                    FileId = recognisableDocument.FileUrl,
                    ResponseText = "File upload error: " + ex.Message,
                    UsedTokens = 0
                };
            }
        }

        private async System.Threading.Tasks.Task<MistralDocumentRecognitionResult> SendPrompt(string fileId, string promptText)
        {
            try
            {
                var request = WebRequest.Create(_configuration.AiSendPromptUri) as HttpWebRequest;
                request.Method = "POST";
                request.Headers.Add("Authorization", "Bearer " + _configuration.AiApiKey);
                request.ContentType = "application/json";

                var requestBody = new
                {
                    model = "mistral-large-latest",
                    messages = new object[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "text", text = promptText },
                                new { type = "file", file_id = fileId }
                            }
                        }
                    }
                };

                var jsonBody = JsonConvert.SerializeObject(requestBody);
                var bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

                request.ContentLength = bodyBytes.Length;

                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bodyBytes, 0, bodyBytes.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    var responseText = reader.ReadToEnd();
                    var jsonResponse = JObject.Parse(responseText);

                    var usage = jsonResponse["usage"] as JObject;
                    var totalTokens = usage?["total_tokens"]?.Value<int>() ?? 0;

                    var choices = jsonResponse["choices"] as JArray;
                    var content = choices?[0]?["message"]?["content"]?.ToString() ?? "";

                    return new MistralDocumentRecognitionResult
                    {
                        SessionId = Guid.NewGuid().ToString(),
                        FileId = fileId,
                        ResponseText = content,
                        UsedTokens = totalTokens
                    };
                }
            }
            catch (Exception ex)
            {
                return new MistralDocumentRecognitionResult
                {
                    SessionId = Guid.NewGuid().ToString(),
                    FileId = fileId,
                    ResponseText = "Prompt error: " + ex.Message,
                    UsedTokens = 0
                };
            }
        }
    }
}
