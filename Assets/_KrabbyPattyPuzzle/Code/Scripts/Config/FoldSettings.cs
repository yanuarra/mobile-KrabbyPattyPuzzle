using UnityEngine;

[System.Serializable]
public class FoldSettings
{
    [Header("Folding Configuration")]
    public float snapAngle = 45f;
    public float foldSpeed = 2f;
    public AnimationCurve foldCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float blockSize = 1f;
}
