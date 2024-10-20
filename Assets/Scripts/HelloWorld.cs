//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//
// <code>
using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
#if PLATFORM_IOS
using UnityEngine.iOS;
using System.Collections;
#endif

public class HelloWorld : MonoBehaviour
{
    // Hook up the two properties below with a Text and Button object in your UI.
    public Text outputText;
    public Button startRecoButton;
    public enum Language
    {
        English,
        ChineseTraditional,
        Japanese
    }

    [SerializeField] private Language selectedLanguage;
    public static Language CurrentLanguage { get; private set; }

    private object threadLocker = new object();
    private bool waitingForReco;
    private string message;

    private bool micPermissionGranted = false;

#if PLATFORM_ANDROID || PLATFORM_IOS
    // Required to manifest microphone permission, cf.
    // https://docs.unity3d.com/Manual/android-manifest.html
    private Microphone mic;
#endif

    public async void ButtonClick()
    {
        // 添加語音金鑰 & 地區
        var config = SpeechConfig.FromSubscription("", "");

        // 設置識別輸入語言
        switch (selectedLanguage)
        {
            case Language.English:
                config.SpeechRecognitionLanguage = "en-US"; // English
                break;
            case Language.ChineseTraditional:
                config.SpeechRecognitionLanguage = "zh-TW"; // Traditional Chinese
                break;
            case Language.Japanese:
                config.SpeechRecognitionLanguage = "ja-JP"; // Japanese
                break;
        }

        // Make sure to dispose the recognizer after use!
        using (var recognizer = new SpeechRecognizer(config))
    {
        lock (threadLocker)
        {
            waitingForReco = true;
        }

        var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

        string newMessage = string.Empty;
        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            newMessage = result.Text;
        }
        else if (result.Reason == ResultReason.NoMatch)
        {
            newMessage = "NOMATCH: Speech could not be recognized.";
        }
        else if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = CancellationDetails.FromResult(result);
            newMessage = $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}";
        }

        lock (threadLocker)
        {
            message = newMessage;
            waitingForReco = false;
        }
    }
    }

    void Start()
    {
        CurrentLanguage = selectedLanguage;

        if (outputText == null)
        {
            UnityEngine.Debug.LogError("outputText property is null! Assign a UI Text element to it.");
        }
        else if (startRecoButton == null)
        {
            message = "startRecoButton property is null! Assign a UI Button to it.";
            UnityEngine.Debug.LogError(message);
        }
        else
        {
            // Continue with normal initialization, Text and Button objects are present.
#if PLATFORM_ANDROID
            // Request to use the microphone, cf.
            // https://docs.unity3d.com/Manual/android-RequestingPermissions.html
            message = "Waiting for mic permission";
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#elif PLATFORM_IOS
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Application.RequestUserAuthorization(UserAuthorization.Microphone);
            }
#else
            micPermissionGranted = true;
            message = "音声をテキストコンテンツに変換..."; //將音訊轉換為文字內容...
#endif
            startRecoButton.onClick.AddListener(ButtonClick);
        }
    }

    void Update()
    {
#if PLATFORM_ANDROID
        if (!micPermissionGranted && Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            micPermissionGranted = true;
            message = "Click button to recognize speech";
        }
#elif PLATFORM_IOS
        if (!micPermissionGranted && Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            micPermissionGranted = true;
            message = "Click button to recognize speech";
        }
#endif

        lock (threadLocker)
        {
            if (startRecoButton != null)
            {
                startRecoButton.interactable = !waitingForReco && micPermissionGranted;
            }
            if (outputText != null)
            {
                outputText.text = message;
            }
        }
    }
}
// </code>
