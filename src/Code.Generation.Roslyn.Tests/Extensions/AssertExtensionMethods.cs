using System;
using System.Collections.Generic;
using System.IO;

namespace Code.Generation.Roslyn
{
    using Xunit;

    internal static class AssertExtensionMethods
    {
        public static T AssertEqual<T>(this T actual, T expected)
        {
            Assert.Equal(expected, actual);
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

        public static IEnumerable<T> AssertCollection<T>(this IEnumerable<T> values, params Action<T>[] elementInspectors)
        {
            // ReSharper disable PossibleMultipleEnumeration
            Assert.Collection(values, elementInspectors);
            return values;
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}
