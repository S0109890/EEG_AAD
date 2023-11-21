using UnityEngine;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

[RequireComponent(typeof(AudioSource))]
public class H_FFT : MonoBehaviour
{
    //sm
    private AudioSource audioSource;
    public int sampleDataLength = 1024;
    private float[] sampleData;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        sampleData = new float[sampleDataLength];

        // 8Hz로 FixedUpdate() 호출 빈도 설정
        Time.fixedDeltaTime = 0.125f;  // 8Hz는 0.125초 간격입니다.
    }

    void FixedUpdate()  // Update() 대신 FixedUpdate()를 사용합니다.
    {
        audioSource.GetOutputData(sampleData, 0);

        // 힐베르트 변환 수행
        Complex32[] complexData = sampleData.Select(x => new Complex32(x, 0)).ToArray();
        Fourier.Forward(complexData, FourierOptions.Default);

        // 인벨로프 계산
        float[] envelope = complexData.Select(x => x.Magnitude).ToArray();

        // 여기서 envelope 배열에 인벨로프 값이 저장됩니다.
    }
}