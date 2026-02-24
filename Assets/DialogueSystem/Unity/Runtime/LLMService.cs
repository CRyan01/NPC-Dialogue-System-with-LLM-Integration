using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DialogueSystem.Unity {
    public class  LLMService: MonoBehaviour {
        // API configuration.
        [SerializeField] private string apiKey = "";
        [SerializeField] private string model = "gpt-4o-mini";
        [SerializeField] private int timeoutSeconds = 15;

        [SerializeField] private float temperature = 0.7f; // creativity / correctness.
        [SerializeField] private int maxTokens = 120;

        private const string Endpoint = "https://api.openai.com/v1/chat/completions";

        // A chat request.
        [Serializable] private class ChatRequest {
            public string model;
            public float temperature;
            public int max_tokens;
            public Message[] messages;
        }

        // The message included in a request.
        [Serializable] private class Message {
            public string role;
            public string content;

            // Constructor.
            public Message(string role, string content) {
                this.role = role;
                this.content = content;
            }
        }

        // A response from the api.
        [Serializable] private class ChatResponse {
            public Choice[] choices;

            [Serializable] public class Choice {
                public ResponseMessage message;

                [Serializable] public class ResponseMessage {
                    public string role;
                    public string content;
                }
            }
        }

        // An async operation to send a web request to the openAI api and wait to recieve a response.
        public async Task<string> GenerateNPCReplyAsync(string playerChoiceText, string canonNpcLine) {
            // Ensure there is a valid api key to use.
            if (string.IsNullOrWhiteSpace(apiKey)) {
                // If not throw an exception.
                throw new InvalidOperationException("OpenAIChatService: API key is empty.");
            }

            // Instructions on how to behave.
            string system =
                "You are an NPC in a fantasy RPG game. Stay in-character. " +
                "Reply in 1-2 sentences. Do not mention you are an AI. " +
                "Do not reference the real world. " +
                "You must preserve the meaning of the provided canonical NPC line.";

            // Content of the prompt.
            string user =
                $"The player chose: \"{playerChoiceText}\".\n\n" +
                $"Canonical NPC line (do not change the meaning): \"{canonNpcLine}\".\n\n" +
                "Rewrite the canonical line naturally as the NPC would say it.";

            // Construct the payload.
            var payload = new ChatRequest {
                model = model,
                temperature = temperature,
                max_tokens = maxTokens,
                messages = new[] {
                    new Message("system", system),
                    new Message("user", user)
                }
            };

            // Convert to json.
            string json = JsonUtility.ToJson(payload);

            // Create the web request.
            using var request = new UnityWebRequest(Endpoint, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = timeoutSeconds;

            // Set the request header.
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            // Create an async operation to send the request and wait till its done.
            var asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone) {
                await Task.Yield();
            }

            // If the request fails throw an exception.
            if (request.result != UnityWebRequest.Result.Success) {
                throw new Exception($"OpenAI request failed: {request.responseCode} {request.error}\n{request.downloadHandler.text}");
            }

            // Parse the response.
            var parsedJson = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);

            // Throw an exception if the parsed response is null.
            if (parsedJson == null) {
                throw new Exception("OpenAI response parse failed (parsedJson is null). Raw:\n" + request.downloadHandler.text);
            }

            // Throw an exception if the parsed response is empty.
            if (parsedJson.choices == null || parsedJson.choices.Length == 0 || parsedJson.choices[0].message == null) {
                throw new Exception("OpenAI response parse failed (no choices/message). Raw:\n" + request.downloadHandler.text);
            }

            // Return the response.
            string reply = (parsedJson.choices[0].message.content ?? "").Trim();
            reply = reply.Trim('"');
            return reply;
        }
    }
}
