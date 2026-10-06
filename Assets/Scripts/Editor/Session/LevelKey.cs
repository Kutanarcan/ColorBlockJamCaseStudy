namespace Game.LevelEditor
{
    /// <summary>A level key is its file name and its address: letters, digits, '-' and '_' only.</summary>
    public static class LevelKey
    {
        public static bool IsValid(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            foreach (char c in key)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                    return false;
            }

            return true;
        }
    }
}
