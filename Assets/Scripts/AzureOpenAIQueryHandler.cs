using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AzureOpenAIQueryHandler : MonoBehaviour
{
    public Text outputText;
    public Text inputText;
    public TextToSpeechHandler ttsHandler;

    private static readonly string endpoint = "https://cmedia-4o-mini.openai.azure.com/";
    private static readonly string apiKey = "";
    private static readonly string modelName = "gpt-4o-mini";
    private static readonly string apiVersion = "2023-03-15-preview";

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class RequestBody
    {
        public Message[] messages;
        public int max_tokens;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    [Serializable]
    public class ResponseBody
    {
        public Choice[] choices;
    }

    public async void OnSendRequestButtonClicked()
    {
        if (string.IsNullOrWhiteSpace(inputText.text))
        {
            DisplayResult("Please enter some text.");
            return;
        }

        try
        {
            string result = await SendOpenAIRequest(inputText.text);
            DisplayResult(result);

            if (ttsHandler != null)
            {
                await ttsHandler.SynthesizeSpeechAsync(result);
            }
        }
        catch (Exception ex)
        {
            DisplayResult($"Error: {ex.Message}");
        }
    }

    // 發送請求的方法
    public async Task<string> SendOpenAIRequest(string prompt)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"{endpoint}openai/deployments/{modelName}/chat/completions?api-version={apiVersion}";
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            // 根據 HelloWorld.CurrentLanguage 獲取系統消息
            string languageSystemMessage = GetSystemMessageBasedOnLanguage(HelloWorld.CurrentLanguage);

            var requestBody = new RequestBody
            {
                messages = new[]
                {
                    new Message { role = "system", content = languageSystemMessage },
                    new Message { role = "user", content = prompt }
                },
                max_tokens = 25
            };

            string jsonContent = JsonUtility.ToJson(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                ResponseBody result = JsonUtility.FromJson<ResponseBody>(responseBody);
                return result.choices[0].message.content;
            }
            else
            {
                throw new Exception($"Request failed with status code: {response.StatusCode}");
            }
        }
    }

    // 根據選擇的語言返回相應的系統提示
    private string GetSystemMessageBasedOnLanguage(HelloWorld.Language language)
    {
        switch (language)
        {
            case HelloWorld.Language.English:
                return "You are a helpful assistant, please respond in English.";
            case HelloWorld.Language.ChineseTraditional:
                return "你是一個幫助用戶的助手，請使用繁體中文回答問題。";
            case HelloWorld.Language.Japanese:
                return "あなたはユーザーを助けるアシスタントです。質問に日本語で答えてください。";
            default:
                return "You are a helpful assistant, please respond in English.";
        }
    }

    // 顯示結果的方法
    private void DisplayResult(string result)
    {
        if (outputText != null)
        {
            outputText.text = result;
        }
    }
}
