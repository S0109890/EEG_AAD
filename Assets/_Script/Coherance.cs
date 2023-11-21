//using UnityEngine;
//using System.Linq;
//using MathNet.Numerics;
//using MathNet.Numerics.IntegralTransforms;
//using UnityEngine.UI;
//using System.Collections.Generic;

//namespace EmotivUnityPlugin
//{
//    public class Coherance : MonoBehaviour
//    {
//        public AudioSource audioSource1;
//        public AudioSource audioSource2;

//        private const int chunkSize = 6000;

//        public Slider coherenceSlider;
//        private float threshold = 0.001f;

//        void Start()
//        {
//            Time.fixedDeltaTime = 0.125f;
//            int samplingRate = audioSource1.clip.frequency;
//            print("Audio Clip Sampling Rate: " + samplingRate);
//        }

//        void FixedUpdate()
//        {
//            float[] alphaAverage = EmotivUnityItf.Instance.averageAlphaValue;
//            //print("alpha" + alphaAverage);

//            float[] tempData1 = new float[chunkSize];
//            float[] tempData2 = new float[chunkSize];

//            audioSource1.GetOutputData(tempData1, 0);
//            audioSource2.GetOutputData(tempData2, 0);

//            // Normalize audio envelope values
//            float[] envelope1 = NormalizeData(GetEnvelopeUsingHilbert(tempData1));
//            float[] envelope2 = NormalizeData(GetEnvelopeUsingHilbert(tempData2));


//            float coherence1 = CalculateCoherence(envelope1, alphaAverage);
//            float coherence2 = CalculateCoherence(envelope2, alphaAverage);

//            print(coherence1 + "&&&&");
//            print(coherence2 + "####");
//            UpdateSlider(coherence1, coherence2);

//            print("First sample: " + tempData1[0] + ", Last sample: " + tempData1[chunkSize - 1]);

//        }

//        private float[] NormalizeData(float[] data)
//        {
//            float minValue = data.Min();
//            float maxValue = data.Max();

//            for (int i = 0; i < data.Length; i++)
//            {
//                data[i] = (data[i] - minValue) / (maxValue - minValue);
//            }
//            return data;
//        }
//        private const float MIN_ALPHA_THRESHOLD = 0.01f; // 임계값 설정

//        float CalculateCoherence(float[] envelopeSignal, float alphaValue)
//        {
//            Complex32 alphaComplex = new Complex32(alphaValue, 0);

//            Complex32 crossSpectrum = envelopeSignal.Select(x => new Complex32(x, 0)).Aggregate(Complex32.Zero, (acc, x) => acc + x * Complex32.Conjugate(alphaComplex));

//            print("audio" + envelopeSignal + "alpha" + alphaValue + "~~");
//            float powerSpectrum = envelopeSignal.Select(x => x * x).Sum();
//            float alphaPower = (float)alphaComplex.MagnitudeSquared();

//            // Check if alphaPower is too small
//            if (alphaPower < MIN_ALPHA_THRESHOLD)
//            {
//                return 0f; // Return 0 if alphaPower is below the threshold
//            }

//            float coherence = (float)crossSpectrum.MagnitudeSquared() / (powerSpectrum * alphaPower);
//            return coherence;
//        }


//        private float[] GetEnvelopeUsingHilbert(float[] signal)
//        {
//            Complex32[] complexSignal = signal.Select(x => new Complex32(x, 0)).ToArray();

//            // 푸리에 변환 수행
//            Fourier.Forward(complexSignal, FourierOptions.Default);

//            // 양의 주파수 성분만 유지하고 음의 주파수 성분을 0으로 설정
//            int N = complexSignal.Length;
//            for (int i = N / 2 + 1; i < N; i++)
//            {
//                complexSignal[i] = 0;
//            }
//            for (int i = 1; i < N / 2; i++)
//            {
//                complexSignal[i] *= 2;
//            }

//            // 역 푸리에 변환 수행
//            Fourier.Inverse(complexSignal, FourierOptions.Default);

//            // 인벨로프 계산
//            return complexSignal.Select(x => x.Magnitude).ToArray();
//        }

//        private float targetValue = 0;
//        private float transitionSpeed = 1f;

//        private void UpdateSlider(float coherence1, float coherence2)
//        {
//            if (coherence1 > threshold && coherence1 > coherence2)
//            {
//                targetValue = 1;
//            }
//            else if (coherence2 > threshold && coherence2 > coherence1)
//            {
//                targetValue = -1;
//            }
//            else
//            {
//                targetValue = 0;
//            }
//            print(targetValue + "********");
//            coherenceSlider.value = Mathf.Lerp(coherenceSlider.value, targetValue, transitionSpeed);
//        }
//    }
//}
