using UnityEngine;

namespace YRA
{
    public struct InputState
    {
        public InputPhase phase;
        public Vector2 screenPosition;
        public bool isActive;
    }

    public enum InputPhase
    {
        None,
        Started,
        Ongoing,
        Ended
    }
}