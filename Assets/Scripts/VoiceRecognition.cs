using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using TMPro;

public class VoiceRecognition : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI resultText;
    public Button recordButton;

    [Header("API 인증 정보")]
    public string clientId = "Client_ID";
    public string clientSecret = "Client_Secret";

    [Header("테스트 설정")]
    public bool useTestAudio = false;
    public AudioClip testAudioClip;

    private AudioClip recordedClip;
    private bool isRecording = false;
    private const int micSampleRate = 16000;

    private OrderParser orderParser;


    private List<ParsedOrder> shoppingCart = new List<ParsedOrder>();

    private string currentStatusMsg = "";

    void Start()
    {
        orderParser = GetComponent<OrderParser>();

        EventTrigger trigger = recordButton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = recordButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { StartRecording(); });
        trigger.triggers.Add(pointerDown);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { StopRecordingAndSend(); });
        trigger.triggers.Add(pointerUp);

        UpdateStatusText("버튼을 꾹 누르고 메뉴를 말씀해 주세요!");
    }

    private void StartRecording()
    {
        if (useTestAudio && testAudioClip != null)
        {
            isRecording = true;
            UpdateStatusText("🔊 테스트 파일을 서버로 전송 중...");
            byte[] wavData = ConvertAudioClipToWav(testAudioClip);
            StartCoroutine(SendAudioToNaver(wavData));
            return;
        }

        if (Microphone.devices.Length == 0)
        {
            UpdateStatusText("⚠️ 마이크를 찾을 수 없습니다.");
            return;
        }

        isRecording = true;
        UpdateStatusText("🎤 듣고 있습니다... (누른 채로 말씀하세요)");
        recordedClip = Microphone.Start(null, false, 20, micSampleRate);
    }

    private void StopRecordingAndSend()
    {
        if (!isRecording) return;
        isRecording = false;

        if (useTestAudio) return;

        int lastPosition = Microphone.GetPosition(null);
        Microphone.End(null);

        if (lastPosition < micSampleRate * 0.5f)
        {
            UpdateStatusText("⚠️ 음성이 너무 짧습니다. 꾹 누르고 말씀해 주세요.");
            return;
        }

        UpdateStatusText("⏳ 음성 인식 중...");

        float[] samples = new float[lastPosition * recordedClip.channels];
        recordedClip.GetData(samples, 0);

        AudioClip trimmedClip = AudioClip.Create("TrimmedClip", lastPosition, recordedClip.channels, micSampleRate, false);
        trimmedClip.SetData(samples, 0);

        byte[] wavData = ConvertAudioClipToWav(trimmedClip);
        StartCoroutine(SendAudioToNaver(wavData));
    }

    private IEnumerator SendAudioToNaver(byte[] audioData)
    {
        string url = "https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=Kor";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(audioData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/octet-stream");
            request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", clientId);
            request.SetRequestHeader("X-NCP-APIGW-API-KEY", clientSecret);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                ClovaResponse response = JsonUtility.FromJson<ClovaResponse>(jsonResponse);

                List<ParsedOrder> parsedList = orderParser.AnalyzeOrderText(response.text);

                if (parsedList.Count == 0)
                {
                    UpdateStatusText($"[인식 실패] '{response.text}' 메뉴를 찾지 못했습니다.");
                }
                else
                {
                    foreach (ParsedOrder parsed in parsedList)
                    {
                        if (parsed.isCancel)
                        {
                            int removeIndex = shoppingCart.FindLastIndex(item => item.menuName == parsed.menuName);
                            if (removeIndex != -1) shoppingCart.RemoveAt(removeIndex);
                        }
                        else
                        {
                            shoppingCart.Add(parsed);
                        }
                    }
                    UpdateCartUI();
                    UpdateStatusText("✅ 처리 완료! 다음 메뉴를 말씀해 주세요.");
                }
            }
            else
            {
                UpdateStatusText($"통신 오류: {request.error}");
            }
        }
    }

    private void UpdateCartUI()
    {
        string uiText = $"<size=110%><color=#ff5500><b>{currentStatusMsg}</b></color></size>\n\n";

        if (shoppingCart.Count == 0)
        {
            uiText += "<color=#aaaaaa>장바구니가 비어있습니다.</color>";
        }
        else
        {
            uiText += "<color=#333333><b>=== 🛒 주문 목록 ===</b></color>\n\n";

            for (int i = 0; i < shoppingCart.Count; i++)
            {
                uiText += $"<color=#0055ff>{i + 1}.</color> {shoppingCart[i].displayText}\n";
                uiText += $"<size=60%><color=#888888>   데이터: {shoppingCart[i].finalDataFormat}</color></size>\n\n";
            }
        }

        resultText.text = uiText;
    }

    private void UpdateStatusText(string message)
    {
        currentStatusMsg = message;
        UpdateCartUI();
    }

    [Serializable]
    private class ClovaResponse { public string text; }

    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        int frequency = clip.frequency;
        MemoryStream stream = new MemoryStream();
        byte[] header = new byte[44];
        stream.Write(header, 0, 44);

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] audioData = new byte[samples.Length * 2];
        int rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            short data = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(data);
            audioData[i * 2] = byteArr[0];
            audioData[i * 2 + 1] = byteArr[1];
        }

        stream.Write(audioData, 0, audioData.Length);

        int fileSize = (int)stream.Length;
        stream.Seek(0, SeekOrigin.Begin);

        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(fileSize - 8), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        stream.Write(BitConverter.GetBytes((short)1), 0, 2);
        stream.Write(BitConverter.GetBytes((short)clip.channels), 0, 2);
        stream.Write(BitConverter.GetBytes(frequency), 0, 4);
        stream.Write(BitConverter.GetBytes(frequency * clip.channels * 2), 0, 4);
        stream.Write(BitConverter.GetBytes((short)(clip.channels * 2)), 0, 2);
        stream.Write(BitConverter.GetBytes((short)16), 0, 2);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(audioData.Length), 0, 4);

        return stream.ToArray();
    }
}