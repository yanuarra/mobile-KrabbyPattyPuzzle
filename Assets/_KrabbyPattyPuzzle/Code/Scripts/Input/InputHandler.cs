using UnityEngine;
using UnityEngine.EventSystems;

namespace YRA
{
    public class InputHandler : MonoBehaviour
    {
        [Header("Input Settings")]
        public float dragThreshold = 50f;
        public LayerMask blockLayerMask = -1;
        public Camera gameCamera;
        
        private Vector2 _startPos;
        private Vector2 _currentPos;
        private bool _isDragging = false;
        private FoldingBlock _selectedBlock;
        private FoldDirection _currentFoldDirection;
        private float _currentDragMagnitude = 0f;
        private bool _isInputDown = false;
        private Vector2 _inputPosition;
        private InputResetManager _inputManager;

        void Start()
        {
            if (gameCamera == null)
                gameCamera = Camera.main;

            _inputManager = FindObjectOfType<InputResetManager>();
        }

        void Update()
        {
            HandleInput();

            if (_isDragging && _selectedBlock != null)
            {
                UpdateDragVisualization();
            }
        }

        bool CheckIsPointerOverGameObject()
        {
            bool ifs = EventSystem.current.IsPointerOverGameObject();
            return ifs;
        }

        void HandleInput()
        {
            bool inputStarted = false;
            bool inputEnded = false;

            if (_inputManager.GetMouseButtonDownSafe(0))
            {
                if (CheckIsPointerOverGameObject())
                    return ;
                inputStarted = true;
                _inputPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                inputEnded = true;
            }
            else if (_inputManager.GetMouseButtonSafe(0))
            {
                _inputPosition = Input.mousePosition;
            }
            
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                _inputPosition = touch.position;
                
                if (touch.phase == TouchPhase.Began)
                {
                    inputStarted = true;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    inputEnded = true;
                }
            }
            
            if (inputStarted && !CheckIsPointerOverGameObject())
            {
                OnInputStart();
            }
            else if (inputEnded && !CheckIsPointerOverGameObject())
            {
                OnInputEnd();
            }
            else if (_isInputDown && !CheckIsPointerOverGameObject())
            {
                OnInputDrag();
            }
        }

        void OnInputStart()
        {
            _isInputDown = true;
            _startPos = _inputPosition;
            _currentPos = _inputPosition;
            FoldingBlock block = GetBlockAtScreenPosition(_inputPosition);
    
            if (block != null && block.State.IsSelectable)
                SelectBlock(block);
        }
        
        void OnInputDrag()
        {
            _currentPos = _inputPosition;
            
            if (_selectedBlock != null && !_isDragging)
            {
                float dragDistance = Vector2.Distance(_startPos, _currentPos);
                if (dragDistance > dragThreshold)
                {
                    StartDrag();
                }
            }
            
            if (_isDragging)
            {
                UpdateDragDirection();
            }
        }
        
        void OnInputEnd()
        {
            _isInputDown = false;
            
            if (_isDragging && _selectedBlock != null)
            {
                EndDrag();
            }
            else if (_selectedBlock != null)
            {
                DeselectBlock();
            }
        }
        
        FoldingBlock GetBlockAtScreenPosition(Vector2 screenPos)
        {
            bool isOverUI = EventSystem.current.IsPointerOverGameObject();
            if (isOverUI)
                return null;
            Ray ray = gameCamera.ScreenPointToRay(screenPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, blockLayerMask) && !isOverUI)
            {
                return hit.collider.GetComponent<FoldingBlock>();
            }
            
            return null;
        }
        
        void UpdateDragDirection()
        {
            Vector2 dragVector = _currentPos - _startPos;
            FoldDirection newDirection = GetFoldDirectionFromVector(dragVector);

            if (newDirection != _currentFoldDirection)
            {
                _currentFoldDirection = newDirection;
                UpdateFoldPreview();
            }
        }

        FoldDirection GetFoldDirectionFromVector(Vector2 dragVector)
        {
            if (dragVector.magnitude < dragThreshold)
                return FoldDirection.None;

            float angle = Mathf.Atan2(dragVector.y, dragVector.x) * Mathf.Rad2Deg;

            if (angle < 0) angle += 360;

            if (angle >= 315 || angle < 45)
                return FoldDirection.Right;
            else if (angle >= 45 && angle < 135)
                return FoldDirection.Up;
            else if (angle >= 135 && angle < 225)
                return FoldDirection.Left;
            else if (angle >= 225 && angle < 315)
                return FoldDirection.Down;

            return FoldDirection.None;
        }

        void UpdateFoldPreview()
        {
            if (_selectedBlock == null || _currentFoldDirection == FoldDirection.None)
                return;
            bool canFold = _selectedBlock.foldHandler.CanFoldInDirection(_currentFoldDirection);
        }

        void StartDrag()
        {
            _isDragging = true;
            UpdateDragDirection();
        }
        
        void UpdateDragVisualization()
        {
            if (_selectedBlock != null)
            {
                Vector2 dragVector = _currentPos - _startPos;
                float dragMagnitude = dragVector.magnitude;

                // _selectedBlock.OnDragUpdate(dragVector, dragMagnitude / dragThreshold);
            }
        }

        void OnDrag()
        {

        }

        void EndDrag()
        {
            if (_selectedBlock != null && _currentFoldDirection != FoldDirection.None)
            {
                if (_selectedBlock.foldHandler.CanFoldInDirection(_currentFoldDirection))
                {
                    Vector2 dragVector = _currentPos - _startPos;
                    float currentDragMagnitude = dragVector.magnitude;
                    _selectedBlock.foldHandler.ExecuteFold(_currentFoldDirection, currentDragMagnitude / dragThreshold);
                }
                else
                {
                    _selectedBlock.foldHandler.InvalidFold();
                }
            }
            DeselectBlock();
        }
        
        void SelectBlock(FoldingBlock block)
        {
            if (_selectedBlock != null)
            {
                DeselectBlock();
            }
            _selectedBlock = block;
            _selectedBlock.OnSelected();
        }
        
        void DeselectBlock()
        {
            if (_selectedBlock != null)
            {
                _selectedBlock.OnDeselected();
                _selectedBlock = null;
            }
            _isDragging = false;
            _currentFoldDirection = FoldDirection.None;
        }
    }
}
