using UnityEngine;
using UnityEngine.Events;

namespace YRA
{
    public class FoldingBlock : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private FoldSettings foldSettings;
        [SerializeField] private Transform _offset;
        public Transform parent { get; private set; }
        public BlockState State { get; private set; }
        public BlockType blockType { get; set; }
        public FoldHandler foldHandler;
        public FoldPermissions FoldPermissions { get; private set; }
        public Vector3 originalPosition { get; private set; }
        public Vector3 originalRotation { get; private set; }
        public Collider blockCollider { get; private set; }
        private Outlined[] _outlines;

        public void InitializeComponents()
        {
            State = new BlockState();
            FoldPermissions = new FoldPermissions();
            foldHandler = new FoldHandler(this, foldSettings);

            // Properties
            blockCollider = GetComponent<BoxCollider>();
            AssignMesh(MeshReferences.Instance.GetMeshByBlockType(blockType));
            _outlines = GetComponentsInChildren<Outlined>();
            ToggleOutline(false);
            SetOriginalBlock(this);
        }

        public void OnDeselected()
        {
            ToggleOutline(false);
        }
        
        public void OnSelected()
        {
            ToggleOutline(true);
        }
        
        public void SetOnFoldedEvent(UnityAction doneEvent)
        {
            if (doneEvent!=null)
                foldHandler.onFoldedEvent += doneEvent;
        }
        
        public void ToggleBlockCollider(bool state)
        {
            if (blockCollider == null)
                blockCollider = GetComponent<BoxCollider>();
            blockCollider.enabled = state;
        }

        public void ToggleOutline(bool state)
        {
            if (_outlines == null){
                _outlines = GetComponentsInChildren<Outlined>();
            }
            foreach (var item in _outlines)
            {
                item.enabled = state;
            }
        }

        public void AssignMesh(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, _offset);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
        }

        public void SetOriginalBlock(FoldingBlock original)
        {
            originalPosition = original.transform.position;
            originalRotation = original.transform.eulerAngles;
        }
    }
}
