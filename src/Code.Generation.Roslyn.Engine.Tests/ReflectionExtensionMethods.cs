using System;
using System.Linq;
using System.Reflection;

namespace Code.Generation.Roslyn
{
    using static BindingFlags;

    // TODO: TBD: this could potentially be delivered through an xunit.assert.fluently.reflectively ...
    public static class ReflectionExtensionMethods
    {
        private static bool ContainsAny(this BindingFlags flags, params BindingFlags[] candidates)
            => candidates.Any(x => (flags & x) == x);

        public static T AssertReflectedField<T, TField>(this T value, string name, BindingFlags flags, Action<T, TField> verify)
        {
            // TODO: TBD: perhaps an exception is justified somewhere in here...
            TField GetFieldValue(FieldInfo fieldInfo)
            {
                if (flags.ContainsAny(Instance))
                {
                    return (TField) fieldInfo.GetValue(value);
                }

                if (flags.ContainsAny(Static))
                {
                    return (TField) fieldInfo.GetValue(null);
                }

                return default;
            }

            var actualValue = GetFieldValue(typeof(T).GetField(name, flags));
            verify.Invoke(value, actualValue);
            return value;
        }
    }
}
