using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;
namespace EmotivUnityPlugin
{
    public class RealtimeGraph : MonoBehaviour
    {
        public GraphChartBase graph;
        private float timeSinceStartup;

        // These values will be updated from another script
        private float currentAlphaAverage;
        private float currentAudio1Envelope;
        private float currentAudio2Envelope;

        void Start()
        {
            if (graph == null)
            {
                Debug.LogError("Graph component not attached!");
                return;
            }

            // Initialize your graph here if needed
            graph.DataSource.StartBatch();
            graph.DataSource.ClearCategory("AlphaBand");
            graph.DataSource.ClearCategory("Audio1");
            graph.DataSource.ClearCategory("Audio2");
            graph.DataSource.EndBatch();
        }

        void Update()
        {
            currentAlphaAverage = _Coherance_2.Instance.GetCurrentAlphaAverage();
            currentAudio1Envelope = _Coherance_2.Instance.GetCurrentAudio1Envelope();
            currentAudio2Envelope = _Coherance_2.Instance.GetCurrentAudio2Envelope();
            print("A" + currentAlphaAverage + "A1:" + currentAudio1Envelope + "A2:" + currentAudio2Envelope);
            // Update the time
            timeSinceStartup += Time.deltaTime;

            // Add the new data points to the graph
            graph.DataSource.AddPointToCategoryRealtime("AlphaBand", timeSinceStartup, currentAlphaAverage, 1f / Time.deltaTime);
            graph.DataSource.AddPointToCategoryRealtime("Audio1", timeSinceStartup, currentAudio1Envelope, 1f / Time.deltaTime);
            graph.DataSource.AddPointToCategoryRealtime("Audio2", timeSinceStartup, currentAudio2Envelope, 1f / Time.deltaTime);
        }

    }
}