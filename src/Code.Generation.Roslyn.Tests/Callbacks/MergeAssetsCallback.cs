namespace Code.Generation.Roslyn
{
    public delegate T MergeAssetsCallback<T, in TBits>(T obj, TBits bits);
}
