
using UnityEngine;

public class TargetFramerate : MonoBehaviour {

    [SerializeField] private int targetFrameRate = 60;

    private void Awake() {
        Application.targetFrameRate = targetFrameRate;
    }

}