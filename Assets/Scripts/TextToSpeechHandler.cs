using System.Threading.Tasks;
using UnityEngine;
using Microsoft.CognitiveServices.Speech;

public class TextToSpeechHandler : MonoBehaviour
{
    // Speech SDK 的訂閱和區域
    private static readonly string speechKey = ""; // 替換為你的 Speech API Key
    private static readonly string speechRegion = "japaneast"; // 替換為你使用的地區

    // 語音合成並播放
    public async Task SynthesizeSpeechAsync(string text)
    {
        var config = SpeechConfig.FromSubscription(speechKey, speechRegion);

        // 根據用戶選擇的語言設置語音
        config.SpeechSynthesisVoiceName = GetVoiceNameForLanguage(HelloWorld.CurrentLanguage);

        using (var synthesizer = new SpeechSynthesizer(config))
        {
            var result = await synthesizer.SpeakTextAsync(text);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                Debug.Log("Speech synthesis succeeded.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                Debug.LogError($"CANCELED: Reason={cancellation.Reason}, ErrorDetails={cancellation.ErrorDetails}");
            }
        }
    }

    // 根據語言返回對應的語音名稱
    private string GetVoiceNameForLanguage(HelloWorld.Language language)
    {
        switch (language)
        {
            case HelloWorld.Language.English:
                return "en-US-JennyNeural"; // 英文語音
            case HelloWorld.Language.ChineseTraditional:
                return "zh-TW-HsiaoChenNeural"; // 繁體中文語音
            case HelloWorld.Language.Japanese:
                return "ja-JP-NanamiNeural"; // 日文語音
            default:
                return "en-US-JennyNeural"; // 默認為英文
        }
    }
}
