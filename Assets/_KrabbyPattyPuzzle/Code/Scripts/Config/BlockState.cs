
namespace YRA
{
    public class BlockState
    {
        public bool IsSelectable { get; set; } = true;
        public bool IsFolding { get; set; } = false;
        public bool IsFolded { get; set; } = false;
        public FoldDirection CurrentFoldDirection { get; set; } = FoldDirection.None;
        public float CurrentFoldAngle { get; set; } = 0f;

        public bool CanBeSelected()
        {
            return IsSelectable && !IsFolding;
        }
    }
}