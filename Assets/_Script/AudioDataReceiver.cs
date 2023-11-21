// AudioDataReceiver.cs
using UnityEngine;
using System.Collections.Generic;

public class AudioDataReceiver : MonoBehaviour
{
    public List<float> audioBuffer = new List<float>();
    int sampleRate = 44100;
    // 이 메소드는 오디오 소스가 오디오 데이터를 필터링할 때마다 호출됩니다.
    void OnAudioFilterRead(float[] data, int channels)
    {
        // 오디오 데이터를 버퍼에 추가합니다.
        for (int i = 0; i < data.Length; i += channels)
        {
            audioBuffer.Add(data[i]);
            if (channels == 2 && i + 1 < data.Length) // 스테레오 오디오를 위한 처리
            {
                audioBuffer.Add(data[i + 1]);
            }
        }

        // 버퍼가 너무 커지지 않도록 관리합니다.
        if (audioBuffer.Count > 2 * sampleRate) // 예를 들어, 2초 분량 이상의 데이터는 필요하지 않다고 가정
        {
            audioBuffer.RemoveRange(0, audioBuffer.Count - 2 * sampleRate);
        }
    }
}
