using UnityEngine;

namespace YRA
{
    public interface IBlockSelector
    {
        FoldingBlock GetBlockAtPosition(Vector2 screenPosition);
    }
}