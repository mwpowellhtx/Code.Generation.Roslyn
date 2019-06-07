using System;
using System.Collections.Generic;

namespace Code.Generation.Roslyn.Integration
{
    using static Constants;
    using static Domain;
    using static String;
    using AttributeRenderingOptionDictionary = Dictionary<string, object>;
    using IAttributeRenderingOptionDictionary = IDictionary<string, object>;

    /// <summary>
    /// Provides a succinct set of <see cref="Attribute"/> oriented extension methods.
    /// </summary>
    internal static class AttributeExtensionMethods
    {
        /// <summary>
        /// Returns the Notations associated with the <see cref="Attribute"/>
        /// <typeparamref name="TAttribute"/> type. <paramref name="obj"/> is not used herein
        /// except as an anchor for the extension method.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAttributeNotations<TAttribute>(this object obj)
            where TAttribute : Attribute
        {
            yield return obj.RenderAttributeNotation<TAttribute>();
            yield return obj.RenderAttributeNotation<TAttribute>(new AttributeRenderingOptionDictionary {{full_name, true}});
        }

        // TODO: TBD: whether Attribute arguments are necessary...
        // TODO: TBD: could provide as a set of string-factory methods?
        // TODO: TBD: or as a dictionary of string name/value pairs where property completion is desired...
        // TODO: TBD: for now we will assume simple parameter-/property-less attributes...
        /// <summary>
        /// Renders the <paramref name="type"/> given the <paramref name="options"/>.
        /// Defaults to <see cref="DefaultOptions"/> when Null.
        /// </summary>
        /// <param name="obj">Not used except as an extension method anchor.</param>
        /// <param name="options">Allowable Options are <see cref="shorthand"/>
        /// (<see cref="bool"/>) and <see cref="full_name"/> (<see cref="bool"/>).</param>
        /// <returns></returns>
        /// <see cref="AttributeName"/>
        /// <see cref="OpenSquareBracket"/>
        /// <see cref="CloseSquareBracket"/>
        /// <see cref="shorthand"/>
        /// <see cref="full_name"/>
        /// <see cref="bool"/>
        public static string RenderAttributeNotation(this object obj, Type type, IAttributeRenderingOptionDictionary options = null)
        {
            options = options ?? DefaultOptions;

            // TODO: TBD: if we are doing this, it might make sense to formalize an interface/concrete implementation with the same properties...
            // Form an anonymous type Options out of the Ad-Hoc Dictionary for local use.
            var o = new
            {
                UseShorthand = options.TryGetValue(shorthand, out var x) && (bool) x, // default: false
                UseFullName = !options.TryGetValue(full_name, out var y) || (bool) y, // default: true
                AsAssembly = options.TryGetValue(assembly, out var z) && (bool) z // default: false
            };

            // ReSharper disable once ImplicitlyCapturedClosure
            string RenderAsAssembly() => o.AsAssembly ? $"{nameof(assembly)}{Colon} " : Empty;

            string RenderFullName() => o.UseFullName ? type.FullName : type.Name;

            string Render(string name)
                => name == AttributeName
                    ? AttributeName
                    : o.UseShorthand && type.Name.EndsWith(AttributeName)
                        ? name.Substring(0, name.Length - AttributeName.Length)
                        : name;

            // TODO: TBD: for instance, might capture "square brackets" as an Option...
            // TODO: TBD: for example, [Attribute1, Attribute2, ..., AttributeN] is perfectly fine...
            return Join($"{RenderAsAssembly()}{Render(RenderFullName())}", $"{OpenSquareBracket}", $"{CloseSquareBracket}");
        }

        /// <summary>
        /// Gets the DefaultOptions, literally <see cref="shorthand"/> <value>false</value>,
        /// <see cref="full_name"/> <value>true</value>. Also whether as <see cref="assembly"/>.
        /// </summary>
        private static IAttributeRenderingOptionDictionary DefaultOptions
            => new AttributeRenderingOptionDictionary {{shorthand, false}, {full_name, true}, {assembly, false}}; // the defaults

        // TODO: TBD: might consider specifying Rendering Options...
        /// <summary>
        /// Renders the <typeparamref name="TAttribute"/> given the <paramref name="options"/>.
        /// Defaults to <see cref="DefaultOptions"/> when Null.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="obj">Not used except as an extension method anchor.</param>
        /// <param name="options">Allowable Options are <see cref="shorthand"/>
        /// (<see cref="bool"/>) and <see cref="full_name"/> (<see cref="bool"/>).</param>
        /// <returns></returns>
        /// <see cref="AttributeName"/>
        /// <see cref="OpenSquareBracket"/>
        /// <see cref="CloseSquareBracket"/>
        /// <see cref="RenderAttributeNotation(object,Type,IAttributeRenderingOptionDictionary)"/>
        /// <see cref="shorthand"/>
        /// <see cref="full_name"/>
        /// <see cref="bool"/>
        public static string RenderAttributeNotation<TAttribute>(this object obj, IAttributeRenderingOptionDictionary options = null)
            where TAttribute : Attribute
            => RenderAttributeNotation(obj, typeof(TAttribute), options);
    }
}
