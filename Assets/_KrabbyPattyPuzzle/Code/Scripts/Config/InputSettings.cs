using UnityEngine;

namespace YRA
{
    [System.Serializable]
    public class InputSettings
    {
        [Header("Input Configuration")]
        public float dragThreshold = 50f;
        public LayerMask blockLayerMask = -1;
        public float snapThreshold = 0.8f;
    }
}