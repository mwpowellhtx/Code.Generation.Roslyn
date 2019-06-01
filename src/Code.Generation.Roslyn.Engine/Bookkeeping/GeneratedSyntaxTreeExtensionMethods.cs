//using System.IO;

//namespace Code.Generation.Roslyn
//{
//    using static Path;

//    internal static class GeneratedSyntaxTreeExtensionMethods
//    {
//        /// <summary>
//        /// Purges the <param name="registry"></param> of any Remaining
//        /// <see cref="GeneratedSyntaxTreeDescriptor"/>.
//        /// Leaves the Input Syntax Tree File itself intact.
//        /// </summary>
//        /// <param name="registry"></param>
//        /// <returns></returns>
//        public static GeneratedSyntaxTreeRegistry Purge(this GeneratedSyntaxTreeRegistry registry)
//        {
//            var outDir = registry.OutputDirectory;

//            foreach (var x in registry)
//            {
//                foreach (var y in x.GeneratedAssetKeys)
//                {
//                    var path = Combine(outDir, $"{y:D}.g.cs");
//                    if (!File.Exists(path))
//                    {
//                        continue;
//                    }

//                    File.Delete(path);
//                }

//                x.GeneratedAssetKeys.Clear();
//            }

//            return registry;
//        }
//    }
//}
