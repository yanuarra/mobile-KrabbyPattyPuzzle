using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class InputResetManager : MonoBehaviour
{
    [Header("Input Reset Settings")]
   private bool _autoResetAfterUI = true;
   private float _resetDelay = 0.1f;
   private bool _debugMode = false;
   private bool _wasOverUI = false;
   private bool _inputNeedsReset = false;
   private  Vector3 _lastMousePosition;
   private  bool[] _mouseButtonStates = new bool[3]; 
   private System.Action _OnInputReset;
   private System.Action _OnUIInteractionDetected;
    
    private void Update()
    {
        CheckUIInteraction();
        
        if (_autoResetAfterUI && _inputNeedsReset)
        {
            StartCoroutine(DelayedInputReset());
            _inputNeedsReset = false;
        }
    }
    
    public void ResetInputAxes()
    {
        if (_debugMode)
            Debug.Log("[InputReset] Resetting input axes and clearing buffers");
        
        ClearMouseInput();
        ClearInputAxes();
        ClearTouchInput();
        ResetInputTracking();
        _OnInputReset?.Invoke();
    }
    
    private void CheckUIInteraction()
    {
        bool currentlyOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        
        if (_wasOverUI && !currentlyOverUI)
        {
            if (_debugMode)
                Debug.Log("[InputReset] Transitioned from UI to scene - marking for reset");
            
            _inputNeedsReset = true;
            _OnUIInteractionDetected?.Invoke();
        }
        
        _wasOverUI = currentlyOverUI;
    }
    
    private void ClearMouseInput()
    {
        for (int i = 0; i < 3; i++)
        {
            _mouseButtonStates[i] = Input.GetMouseButton(i);
        }
        _lastMousePosition = Input.mousePosition;
    }
    
    private void ClearInputAxes()
    {
        if (_debugMode)
            Debug.Log($"[InputReset] Current Mouse X: {Input.GetAxis("Mouse X")}, Mouse Y: {Input.GetAxis("Mouse Y")}");
        
    }

    private void ClearTouchInput()
    {
        if (Input.touchCount > 0)
        {
            if (_debugMode)
                Debug.Log($"[InputReset] Clearing {Input.touchCount} touch inputs");
        }
    }
    
    private void ResetInputTracking()
    {
        _inputNeedsReset = false;
        _lastMousePosition = Input.mousePosition;
    }
    
    private IEnumerator DelayedInputReset()
    {
        yield return new WaitForSeconds(_resetDelay);
        ResetInputAxes();
    }
    
    public bool ShouldIgnoreInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;
        
        for (int i = 0; i < 3; i++)
        {
            if (Input.GetMouseButtonDown(i) && _mouseButtonStates[i])
            {
                if (_debugMode)
                    Debug.Log($"[InputReset] Ignoring mouse button {i} - likely from UI interaction");
                return true;
            }
        }
        
        return false;
    }
    
    public bool GetMouseButtonDownSafe(int button)
    {
        if (ShouldIgnoreInput()) return false;
        return Input.GetMouseButtonDown(button);
    }
    
    public bool GetMouseButtonSafe(int button)
    {
        if (ShouldIgnoreInput()) return false;
        return Input.GetMouseButton(button);
    }
    
    public Vector2 GetMouseDeltaSafe()
    {
        if (ShouldIgnoreInput()) return Vector2.zero;
        
        Vector3 currentPos = Input.mousePosition;
        Vector2 delta = currentPos - _lastMousePosition;
        _lastMousePosition = currentPos;
        
        return delta;
    }
    
    public void ManualInputReset()
    {
        if (_debugMode)
            Debug.Log("[InputReset] Manual input reset triggered");
        
        StartCoroutine(ManualInputResetCoroutine());
    }
    
     IEnumerator ManualInputResetCoroutine()
    {
        yield return new WaitForEndOfFrame();
        ResetInputAxes();
        EventSystem.current.SetSelectedGameObject(null);
    }
    
    public bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    
    public void ExampleRaycastInteraction()
    {
        if (GetMouseButtonDownSafe(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log($"Hit object: {hit.collider.name}");
            }
        }
    }
    
}