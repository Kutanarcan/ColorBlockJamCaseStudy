namespace Game.Core
{
    /// <summary>The level is won when every block still on the board is exempt from winning.</summary>
    public static class WinRule
    {
        public static bool IsWon(Board board)
        {
            for (int id = 0; id < board.EntityCount; id++)
            {
                if (board.GetEntity(id) is Block block && !block.IsExited && !IsExempt(block))
                    return false;
            }

            return true;
        }

        private static bool IsExempt(Block block)
        {
            for (int i = 0; i < block.ModifierCount; i++)
            {
                if (block.GetModifier(i) is IWinExempt)
                    return true;
            }

            return false;
        }
    }
}
