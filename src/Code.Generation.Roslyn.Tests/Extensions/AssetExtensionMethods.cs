namespace Code.Generation.Roslyn
{
    public static class AssetExtensionMethods
    {
        /// <summary>
        /// Merges the <paramref name="bits"/> with the <paramref name="obj"/> by conditionally
        /// invoking <paramref name="merge"/> based on the <paramref name="predicate"/>. The
        /// default Predicate yields True, or, rather, unconditionally allows the Merge to occur.
        /// </summary>
        /// <typeparam name="T">The Type of the Object under consideration.</typeparam>
        /// <typeparam name="TBits">The Type of the Bits under consideration.</typeparam>
        /// <param name="obj">The Object under Merge consideration.</param>
        /// <param name="bits">The Bits under Merge consideration.</param>
        /// <param name="merge">The Merge Callback.</param>
        /// <param name="predicate">Optionally invokes the Predicate. Merges when the Response
        /// is True. Does not merge when there is a Null Predicate or when the Response was False.</param>
        /// <returns></returns>
        public static T MergeAssets<T, TBits>(this T obj, TBits bits, MergeAssetsCallback<T, TBits> merge, AssetMergePredicate<TBits> predicate = null)
            => predicate?.Invoke(bits) == true ? merge(obj, bits) : obj;
    }
}
