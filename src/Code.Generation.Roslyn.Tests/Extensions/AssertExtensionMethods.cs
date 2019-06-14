using System;
using System.Collections.Generic;
using System.IO;

namespace Code.Generation.Roslyn
{
    using Xunit;

    internal static class AssertExtensionMethods
    {
        public static T AssertNotNull<T>(this T @object)
        {
            Assert.NotNull(@object);
            return @object;
        }

        public static T AssertEqual<T>(this T actual, T expected)
        {
            Assert.Equal(expected, actual);
            return actual;
        }

        public static T AssertEqual<T>(this T actual, T expected, IEqualityComparer<T> comparer)
        {
            Assert.Equal(expected, actual, comparer);
            return actual;
        }

        public static T AssertNotEqual<T>(this T actual, T expected)
        {
            Assert.NotEqual(expected, actual);
            return actual;
        }

        public static string AssertEndsWith(this string actualString, string expectedEndString, StringComparison? comparison = null)
        {
            if (comparison.HasValue)
            {
                Assert.EndsWith(expectedEndString, actualString, comparison.Value);
            }
            else
            {
                Assert.EndsWith(expectedEndString, actualString);
            }

            return actualString;
        }

        public static bool AssertTrue(this bool value)
        {
            Assert.True(value);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return value;
        }

        public static bool AssertFalse(this bool value)
        {
            Assert.False(value);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return value;
        }

        public static T AssertFileExists<T>(this T info)
            where T : FileSystemInfo
        {
            Assert.True(File.Exists(Assert.IsType<FileInfo>(info).FullName));
            return info;
        }

        public static string AssertFileExists(this string path)
        {
            new FileInfo(path).AssertFileExists();
            return path;
        }

        public static T AssertFileDoesNotExist<T>(this T info)
            where T : FileSystemInfo
        {
            Assert.False(File.Exists(Assert.IsType<FileInfo>(info).FullName));
            return info;
        }

        public static string AssertFileDoesNotExist(this string path)
        {
            new FileInfo(path).AssertFileDoesNotExist();
            return path;
        }

        /// <summary>
        /// Asserts whether the File specified by <paramref name="path"/> Contains the
        /// <see cref="string"/> <paramref name="s"/>. Implicitly asserts whether
        /// <see cref="AssertFileExists(string)"/>.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string AssertFileContains(this string path, string s)
        {
            Assert.Contains(s, File.ReadAllText(AssertFileExists(path)));
            return path;
        }

        /// <summary>
        /// Asserts whether the File specified by <paramref name="path"/> Does Not Contain
        /// the <see cref="string"/> <paramref name="s"/>. Implicitly asserts whether
        /// <see cref="AssertFileExists(string)"/>.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string AssertFileDoesNotContain(this string path, string s)
        {
            Assert.DoesNotContain(s, File.ReadAllText(path.AssertFileExists()));
            return path;
        }

        public static IEnumerable<T> AssertContains<T>(this IEnumerable<T> collection, T expected)
        {
            // ReSharper disable PossibleMultipleEnumeration
            Assert.Contains(expected, collection);
            return collection;
            // ReSharper restore PossibleMultipleEnumeration
        }

        public static IEnumerable<T> AssertCollection<T>(this IEnumerable<T> values, params Action<T>[] elementInspectors)
        {
            // ReSharper disable PossibleMultipleEnumeration
            Assert.Collection(values, elementInspectors);
            return values;
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        /// Asserts whether <paramref name="object"/> <see cref="Assert.IsType{T}"/>
        /// Returns the strongly typed <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="object"></param>
        /// <returns></returns>
        public static T AssertIsType<T>(this object @object) => Assert.IsType<T>(@object);

        /// <summary>
        /// Asserts whether <paramref name="object"/> <see cref="Assert.IsAssignableFrom{T}"/>
        /// Returns the weakly typed Object IsAssignableFrom <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="object"></param>
        /// <returns></returns>
        public static T AssertIsAssignableFrom<T>(this object @object) => Assert.IsAssignableFrom<T>(@object);

        /// <summary>
        /// Returns whether the strongly typed <paramref name="value"/> is the Same as the
        /// <paramref name="expected"/> instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        public static T AssertSame<T>(this T value, object expected)
        {
            object actual = value;
            Assert.Same(expected, actual);
            return value;
        }

        /// <summary>
        /// Returns whether the strongly typed <paramref name="value"/> is the Not Same as the
        /// <paramref name="expected"/> instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        public static T AssertNotSame<T>(this T value, object expected)
        {
            object actual = value;
            Assert.NotSame(expected, actual);
            return value;
        }
    }
}
