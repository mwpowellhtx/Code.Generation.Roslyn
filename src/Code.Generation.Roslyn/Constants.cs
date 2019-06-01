namespace Code.Generation.Roslyn
{
    internal static class Constants
    {
        /// <summary>
        /// &quot;\r&quot;
        /// </summary>
        public const string CarriageReturn = "\r";

        /// <summary>
        /// &quot;\n&quot;
        /// </summary>
        public const string LineFeed = "\n";

        /// <summary>
        /// <see cref="CarriageReturn"/> plus <see cref="LineFeed"/>.
        /// </summary>
        /// <see cref="CarriageReturn"/>
        /// <see cref="LineFeed"/>
        public const string CarriageReturnLineFeed = CarriageReturn + LineFeed;
    }
}
