namespace Game.Runtime
{
    /// <summary>The four BlockParts meshes. InnerCorner covers three quadrants around a vertex; the others one quadrant.</summary>
    public enum BlockPartKind
    {
        OuterCorner,
        Edge,
        Center,
        InnerCorner
    }
}
