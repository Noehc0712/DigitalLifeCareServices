using System;
using System.Collections;
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
    [Header("API키가 필요하므로 필요시 말씀해주세요.")]

    [Header("테스트 설정")]
    public bool useTestAudio = false;
    public AudioClip testAudioClip;

    private AudioClip recordedClip;
    private bool isRecording = false;
    private const int micSampleRate = 16000;

    private OrderParser orderParser;

    void Start()
    {
        orderParser = GetComponent<OrderParser>();

        EventTrigger trigger = recordButton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = recordButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { StartRecording(); });
        trigger.triggers.Add(pointerDown);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { StopRecordingAndSend(); });
        trigger.triggers.Add(pointerUp);
    }

    private void StartRecording()
    {
        if (useTestAudio && testAudioClip != null)
        {
            isRecording = true;
            resultText.text = "테스트 파일을 서버로 전송 중입니다...";
            byte[] wavData = ConvertAudioClipToWav(testAudioClip);
            StartCoroutine(SendAudioToNaver(wavData));
            return;
        }

        if (Microphone.devices.Length == 0)
        {
            resultText.text = "마이크를 찾을 수 없습니다.";
            return;
        }

        isRecording = true;
        resultText.text = "듣고 있습니다... (말씀하신 후 손을 떼주세요)";
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
            resultText.text = "음성이 너무 짧습니다. 버튼을 꾹 누르고 말씀해 주세요.";
            return;
        }

        resultText.text = "음성을 텍스트로 변환 중입니다...";

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

                string rawText = response.text;
                string formattedData = orderParser.AnalyzeOrderText(rawText);
                resultText.text = $"[인식 문장]\n{rawText}\n\n[웹 전송용 데이터]\n{formattedData}";
            }
            else
            {
                resultText.text = $"통신 오류: {request.error}\n{request.downloadHandler.text}";
            }
        }
    }

    [Serializable]
    private class ClovaResponse
    {
        public string text;
    }

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