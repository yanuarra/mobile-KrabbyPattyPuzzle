using UnityEngine;

namespace YRA
{
    public interface IFoldable
    {
        void ShowPreview(FoldingBlock block, FoldDirection direction, float strength);
        void HidePreview(FoldingBlock block);
        bool CanFoldInDirection(FoldDirection direction);
        FoldingBlock GetAdjacentBlock(FoldDirection direction);
        FoldDirection GetDirection(Vector2 dragVector);
        void ExecuteFold(FoldDirection direction, float dragStrength);
        void InvalidFold();
    }
}
