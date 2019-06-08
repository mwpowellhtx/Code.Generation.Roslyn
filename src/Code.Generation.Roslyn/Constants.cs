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

        /// <summary>
        /// The pair of Open- and Close-Paren, literally &quot;()&quot;
        /// </summary>
        public const string Parens = "()";

        /// <summary>
        /// Gets the OpenParen, literally &apos;(&apos;.
        /// </summary>
        /// <see cref="Parens"/>
        public static char OpenParen => Parens[0];

        /// <summary>
        /// Gets the CloseParen, literally &apos;)&apos;.
        /// </summary>
        /// <see cref="Parens"/>
        public static char CloseParen => Parens[1];

        /// <summary>
        /// The pair of Open- and Close-CurlyBraces, literally &quot;{}&quot;
        /// </summary>
        public const string CurlyBraces = "{}";

        /// <summary>
        /// Gets the OpenCurlyBrace, literally &apos;{&apos;.
        /// </summary>
        /// <see cref="CurlyBraces"/>
        public static char OpenCurlyBrace => CurlyBraces[0];

        /// <summary>
        /// Gets the CloseCurlyBrace, literally &apos;}&apos;.
        /// </summary>
        /// <see cref="CurlyBraces"/>
        public static char CloseCurlyBrace => CurlyBraces[1];

        /// <summary>
        /// The pair of Open- and Close-SquareBrackets, literally &quot;[]&quot;
        /// </summary>
        public const string SquareBrackets = "[]";

        /// <summary>
        /// Gets the OpenSquareBracket, literally &apos;[&apos;.
        /// </summary>
        /// <see cref="SquareBrackets"/>
        public static char OpenSquareBracket => SquareBrackets[0];

        /// <summary>
        /// Gets the CloseSquareBracket, literally &apos;]&apos;.
        /// </summary>
        /// <see cref="SquareBrackets"/>
        public static char CloseSquareBracket => SquareBrackets[1];

        /// <summary>
        /// &apos;,&apos;
        /// </summary>
        public const char Comma = ',';

        /// <summary>
        /// &apos;:&apos;
        /// </summary>
        public const char Colon = ':';

        /// <summary>
        /// &apos;;&apos;
        /// </summary>
        public const char SemiColon = ';';

        /// <summary>
        /// &quot;using&quot;
        /// </summary>
        public static string Using = nameof(Using).ToLower();

        /// <summary>
        /// &quot;namespace&quot;
        /// </summary>
        public static string Namespace = nameof(Namespace).ToLower();
    }
}
