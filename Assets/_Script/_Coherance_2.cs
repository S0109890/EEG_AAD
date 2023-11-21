using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using MathNet.Numerics;
using System;

namespace EmotivUnityPlugin
{
    public class _Coherance_2 : MonoBehaviour
    {

        // 싱글톤 인스턴스
        public static _Coherance_2 Instance { get; private set; }


        public AudioSource audioSource1;
        public AudioSource audioSource2;

        public AudioDataReceiver audioDataReceiver1;
        public AudioDataReceiver audioDataReceiver2;

        private const int chunkSize1 = 960; //fixed update 50hz / audio 48000hz
        private const int chunkSize2 = 882; //fixed update 50hz / audio 44100hz
        private const int chunkSize = 320; //fixed update 50hz / audio 44100hz

        private float threshold = 0.001f;
        private const float MIN_ALPHA_THRESHOLD = 0.01f; // 임계값 설정

        public Slider coherenceSlider;

        private List<float> alphaBuffer = new List<float>(); // alpha band 값을 저장할 버퍼

        private const int sampleRate = 16000; // 타겟 샘플 레이트
        private const int bufferSize = sampleRate; // 1초 동안의 샘플 수

        // 오디오 데이터 버퍼
        private List<float> audioBuffer1 = new List<float>();
        private List<float> audioBuffer2 = new List<float>();

        //내보내기
        // 현재 알파 밴드 평균값을 저장하는 변수
        private float currentAlphaAverage = 0f;
        // 현재 오디오 1과 오디오 2의 인벨로프 값을 저장하는 변수
        private float currentAudio1Envelope = 0f;
        private float currentAudio2Envelope = 0f;


        void Awake()
        {
            // 싱글톤 인스턴스가 이미 존재하는지 확인
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // 씬이 변경되어도 파괴되지 않도록 설정
            }
            else
            {
                Destroy(gameObject); // 중복 인스턴스 파괴
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            int samplingRate1 = audioSource2.clip.frequency;
            print("Audio Clip Sampling Rate1: " + samplingRate1);
            int samplingRate2 = audioSource1.clip.frequency;
            print("Audio Clip Sampling Rate2: " + samplingRate2);

            Time.fixedDeltaTime = 0.02f; // 1초 / 50 = 0.02초

        }

        private float[] alphaAverage_50 = new float[50];
        private int alphaIndex = 0;

        float[] oneSecondAudioEnvelope1;
        float[] oneSecondAudioEnvelope2;
        float[] oneSecondAlphaValues;
        // Update is called once per frame

        void FixedUpdate()
        {
            // 알파 밴드 데이터를 모읍니다.

            float alphaAverage = EmotivUnityItf.Instance.averageAlphaValue;
            // 배열에 알파 밴드 샘플을 추가합니다.
            if (alphaIndex < alphaAverage_50.Length)
            {
                alphaAverage_50[alphaIndex] = alphaAverage;
                Debug.Log($"#Added {alphaAverage} to alphaAverage_50 at index {alphaIndex}");
                Debug.Log($"#Current array state: [{string.Join(", ", alphaAverage_50)}]");
                alphaIndex++;
            }


            //오디오 버퍼를 받아옴
            List<float> buffer1 = audioDataReceiver1.audioBuffer;
            List<float> buffer2 = audioDataReceiver2.audioBuffer;
            //print(buffer1.Count + "))))");
            //print(buffer2.Count + "((((");

            // 버퍼에 충분한 데이터가 축적되었는지 확인
            if (buffer1.Count >= audioSource1.clip.frequency && buffer2.Count >= audioSource2.clip.frequency)
            {

                alphaIndex = 0;

                // List를 배열로 변환
                float[] bufferArray1 = buffer1.ToArray();
                float[] bufferArray2 = buffer2.ToArray();

                // 다운샘플링
                float[] downsampledData1 = DownsampleAudio(bufferArray1, audioSource1.clip.frequency, sampleRate);
                float[] downsampledData2 = DownsampleAudio(bufferArray2, audioSource2.clip.frequency, sampleRate);

                // 받아온 샘플의 개수를 확인
                //Debug.Log("##downsampledData1: " + downsampledData1.Length);
                //Debug.Log("##downsampledData2: " + downsampledData2.Length);

                // 정규화
                float[] normalizedData1 = NormalizeData(downsampledData1);
                float[] normalizedData2 = NormalizeData(downsampledData2);

                // 인벨로프 계산
                int windowSizeForEnvelope = buffer1.Count / 16000; // 예를 들어, 16000개의 샘플을 얻기 위한 윈도우 크기
                float[] envelope1 = CalculateEnvelopeRMS(normalizedData1, windowSizeForEnvelope);
                float[] envelope2 = CalculateEnvelopeRMS(normalizedData2, windowSizeForEnvelope);

                // 코히어런스 계산을 위한 데이터 준비
                oneSecondAudioEnvelope1 = envelope1;
                oneSecondAudioEnvelope2 = envelope2;
                //alpha 밴드값 40000개 만들자
                float[] upsampledAlphaValue = UpsampleLinear(alphaAverage_50, alphaAverage_50.Length, envelope1.Length);
                Debug.Log("downsampledData1: " + upsampledAlphaValue.Length);

                oneSecondAlphaValues = upsampledAlphaValue;
                // 값내보내기 :여 기에 값을 저장하는 코드 추가
                currentAlphaAverage = oneSecondAlphaValues.Average(); // 알파 밴드 평균값 저장
                print("@@@@" + currentAlphaAverage);
                currentAudio1Envelope = oneSecondAudioEnvelope1.Length > 0 ? oneSecondAudioEnvelope1[oneSecondAudioEnvelope1.Length - 1] : 0f; // 오디오 1 인벨로프 마지막 값 저장
                currentAudio2Envelope = oneSecondAudioEnvelope2.Length > 0 ? oneSecondAudioEnvelope2[oneSecondAudioEnvelope2.Length - 1] : 0f; // 오디오 2 인벨로프 마지막 값 저장
                // 오디오1과 알파 밴드의 코히어런스 계산
                float coherence1 = CalculateCoherence(oneSecondAudioEnvelope1, oneSecondAlphaValues);

                // 오디오2와 알파 밴드의 코히어런스 계산
                float coherence2 = CalculateCoherence(oneSecondAudioEnvelope2, oneSecondAlphaValues);

                // 코히어런스 값 출력
                Debug.Log("Coherence1: " + coherence1);
                Debug.Log("Coherence2: " + coherence2);

                // 두 코히어런스 값을 UI에 반영
                UpdateSlider(coherence1, coherence2);

                // 버퍼 초기화
                buffer1.Clear();
                buffer2.Clear();
                alphaBuffer.Clear();
            }

        }

        //2. 정규화
        private float[] NormalizeData(float[] data)
        {
            float minValue = data.Min();
            float maxValue = data.Max();

            if (maxValue - minValue == 0) // if all values are the same
            {
                return data.Select(_ => 0.5f).ToArray(); // or handle it in some other way
            }

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (data[i] - minValue) / (maxValue - minValue);
            }
            return data;
        }
        //1. upsample
        private float[] UpsampleLinear(float[] input, int originalSampleRate, int targetSampleRate)
        {
            // Calculate the upsampling factor
            float factor = (float)targetSampleRate / originalSampleRate;
            // Calculate the new size after upsampling
            int newSize = Mathf.CeilToInt(input.Length * factor);
            float[] upsampled = new float[newSize];

            //UnityEngine.Debug.Log($"@Upsampling from {input.Length} to {newSize} samples");

            // Perform the upsampling with linear interpolation
            for (int i = 0; i < newSize; i++)
            {
                // Find the position in the original data
                float position = i / factor;
                int index = (int)position;
                float t = position - index;

                // Linearly interpolate between the two nearest data points
                float value;
                if (index < input.Length - 1)
                {
                    value = input[index] * (1 - t) + input[index + 1] * t;
                    //UnityEngine.Debug.Log($"@Interpolating between index {index} and {index + 1}, t={t}, value={value}");
                }
                else
                {
                    // For the last data point, no interpolation is needed
                    value = input[input.Length - 1];
                    //UnityEngine.Debug.Log($"@Using last value for index {index}, value={value}");
                }

                upsampled[i] = value;
            }

            UnityEngine.Debug.Log("@Upsampled Data: " + string.Join(", ", upsampled));
            return upsampled;
        }



        //1. downsample
        float[] DownsampleAudio(float[] originalSamples, int originalSampleRate, int targetSampleRate)
        {
            float downsampleFactor = (float)originalSampleRate / targetSampleRate;
            int newSampleCount = (int)(originalSamples.Length / downsampleFactor);
            float[] downsampledSamples = new float[newSampleCount];

            for (int i = 0; i < newSampleCount; i++)
            {
                int originalSampleIndex = (int)(i * downsampleFactor);
                downsampledSamples[i] = originalSamples[originalSampleIndex];
                // 필요하다면 여기에 로우패스 필터링을 적용하세요.
            }

            return downsampledSamples;
        }


        //3. envelop
        float[] CalculateEnvelopeRMS(float[] samples, int windowSize)
        {

            // 샘플을 윈도우 사이즈로 나눠서 본다.
            // 여기서 남은 샘플을 처리하기 위해 Math.Ceiling을 사용합니다.
            int envelopeSize = (int)Math.Ceiling((double)samples.Length / windowSize);
            float[] envelope = new float[envelopeSize];

            for (int i = 0; i < envelopeSize; i++)
            {
                float sumOfSquares = 0f;
                int startSampleIndex = i * windowSize;
                int endSampleIndex = Math.Min((i + 1) * windowSize, samples.Length);
                int samplesInWindow = endSampleIndex - startSampleIndex;

                for (int j = startSampleIndex; j < endSampleIndex; j++)
                {
                    float sampleValue = samples[j];
                    sumOfSquares += sampleValue * sampleValue;
                }

                float rms = Mathf.Sqrt(sumOfSquares / samplesInWindow); // Calculate the RMS for the window
                envelope[i] = rms;
            }

            return envelope;
        }

        //4. 코히어런스 (alpha + audio1,2)
        // 4. Coherence (alpha + audio1,2)
        float CalculateCoherence(float[] envelopeSignal, float[] alphaValues)
        {
            // 두 배열의 길이가 다른 경우, 예외를 발생시키거나 두 배열을 같은 길이로 조정
            if (envelopeSignal.Length != alphaValues.Length)
            {
                // 예외를 발생시키거나, 또는
                // 더 짧은 배열의 길이에 맞춰서 두 배열을 자르는 로직을 추가할 수 있습니다.
                throw new ArgumentException("@@The length of envelopeSignal and alphaValues must be the same.");
            }


            // Ensure both arrays are of the same length
            if (envelopeSignal.Length != alphaValues.Length)
                throw new ArgumentException("The length of envelopeSignal and alphaValues must be the same.");

            // Convert alpha values to complex form
            Complex32[] alphaComplexArray = alphaValues.Select(alpha => new Complex32(alpha, 0)).ToArray();

            // Calculate cross-spectrum
            Complex32 crossSpectrumSum = Complex32.Zero;
            for (int i = 0; i < envelopeSignal.Length; i++)
            {
                crossSpectrumSum += new Complex32(envelopeSignal[i], 0) * Complex32.Conjugate(alphaComplexArray[i]);
            }

            // Calculate power spectrums
            float powerSpectrumEnvelope = envelopeSignal.Select(x => x * x).Sum();
            float powerSpectrumAlpha = alphaComplexArray.Select(x => (float)x.MagnitudeSquared()).Sum();

            // Check if alpha power is too small
            if (powerSpectrumAlpha < MIN_ALPHA_THRESHOLD)
            {
                return 0f; // Return 0 if alpha power is below the threshold
            }

            // Calculate coherence
            float coherence = (float)crossSpectrumSum.MagnitudeSquared() / (powerSpectrumEnvelope * powerSpectrumAlpha);

            return coherence;
        }


        //UI slider
        private float targetValue = 0;
        private float transitionSpeed = 1f;

        private void UpdateSlider(float coherence1, float coherence2)
        {
            if (coherence1 > threshold && coherence1 > coherence2)
            {
                targetValue = 1;
            }
            else if (coherence2 > threshold && coherence2 > coherence1)
            {
                targetValue = -1;
            }
            else
            {
                targetValue = 0;
            }
            print(targetValue + "********");
            coherenceSlider.value = Mathf.Lerp(coherenceSlider.value, targetValue, transitionSpeed);
        }

        // 외부에서 알파 밴드 평균값을 가져갈 수 있는 메서드
        public float GetCurrentAlphaAverage()
        {
            return currentAlphaAverage;
        }

        // 외부에서 오디오 1 인벨로프 값을 가져갈 수 있는 메서드
        public float GetCurrentAudio1Envelope()
        {
            return currentAudio1Envelope;
        }

        // 외부에서 오디오 2 인벨로프 값을 가져갈 수 있는 메서드
        public float GetCurrentAudio2Envelope()
        {
            return currentAudio2Envelope;
        }

    }
}