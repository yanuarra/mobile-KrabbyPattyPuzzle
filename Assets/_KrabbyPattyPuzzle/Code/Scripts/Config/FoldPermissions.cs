
namespace YRA
{
    public class FoldPermissions
    {
        public bool CanFoldUp { get; set; } = true;
        public bool CanFoldDown { get; set; } = true;
        public bool CanFoldLeft { get; set; } = true;
        public bool CanFoldRight { get; set; } = true;

        public bool CanFoldInDirection(FoldDirection direction)
        {
            switch (direction)
            {
                case FoldDirection.Up: return CanFoldUp;
                case FoldDirection.Down: return CanFoldDown;
                case FoldDirection.Left: return CanFoldLeft;
                case FoldDirection.Right: return CanFoldRight;
                default: return false;
            }
        }
    }
}