using UnityEngine;

namespace YRA
{
    public interface IDragHandler
    {
        void StartDrag(Vector2 startPosition, FoldingBlock block);
        void UpdateDrag(Vector2 currentPosition);
        void EndDrag();
    }
}