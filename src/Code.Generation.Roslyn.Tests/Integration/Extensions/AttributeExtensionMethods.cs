using System;
using System.Reflection;

namespace Code.Generation.Roslyn.Integration
{
    using static String;

    /// <summary>
    /// Provides a succinct set of <see cref="Attribute"/> oriented extension methods.
    /// </summary>
    internal static class AttributeExtensionMethods
    {
        /// <summary>
        /// &quot;Attribute&quot;
        /// </summary>
        /// <see cref="Attribute"/>
        private const string AttributeName = nameof(Attribute);

        /// <summary>
        /// The pair of Open- and Close-SquareBrackets, literally &quot;[]&quot;
        /// </summary>
        private const string SquareBrackets = "[]";

        /// <summary>
        /// Gets the OpenSquareBracket, literally &apos;[&apos;.
        /// </summary>
        /// <see cref="SquareBrackets"/>
        private static char OpenSquareBracket => SquareBrackets[0];

        /// <summary>
        /// Gets the CloseSquareBracket, literally &apos;]&apos;.
        /// </summary>
        /// <see cref="SquareBrackets"/>
        private static char CloseSquareBracket => SquareBrackets[1];

        // TODO: TBD: whether Attribute arguments are necessary...
        // TODO: TBD: could provide as a set of string-factory methods?
        // TODO: TBD: or as a dictionary of string name/value pairs where property completion is desired...
        // TODO: TBD: for now we will assume simple parameter-/property-less attributes...
        /// <summary>
        /// Renders the <paramref name="type"/> <see cref="Attribute"/> in
        /// <paramref name="shorthand"/> or long-hand. With the odd corner case of
        /// <see cref="Attribute"/> itself.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="shorthand"></param>
        /// <returns></returns>
        /// <see cref="AttributeName"/>
        /// <see cref="OpenSquareBracket"/>
        /// <see cref="CloseSquareBracket"/>
        public static string RenderAttributeNotation(this object obj, Type type, bool shorthand = true)
        {
            string Render<TMemberInfo>(TMemberInfo memberInfo)
                where TMemberInfo : MemberInfo
                => memberInfo.Name == AttributeName
                    ? AttributeName
                    : shorthand && type.Namespace.EndsWith(AttributeName)
                        ? memberInfo.Name.Substring(0, memberInfo.Name.Length - AttributeName.Length)
                        : memberInfo.Name;

            return Join(Render(type), $"{OpenSquareBracket}", $"{CloseSquareBracket}");
        }

        /// <summary>
        /// Renders the <typeparamref name="T"/> <see cref="Attribute"/> in
        /// <paramref name="shorthand"/> or long-hand. With the odd corner case of
        /// <see cref="Attribute"/> itself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="shorthand"></param>
        /// <returns></returns>
        /// <see cref="AttributeName"/>
        /// <see cref="OpenSquareBracket"/>
        /// <see cref="CloseSquareBracket"/>
        /// <see cref="RenderAttributeNotation(object,Type,bool)"/>
        public static string RenderAttributeNotation<T>(this object obj, bool shorthand = true) => RenderAttributeNotation(obj, typeof(T), shorthand);
    }
}
