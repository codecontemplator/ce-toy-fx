using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ce_toy_fx.dynamic
{

    class DynamicRule
    {
        private readonly PortableExecutableReference[] References;
        private readonly string StandardHeader;

        private static readonly Assembly SystemRuntime = Assembly.Load(new AssemblyName("System.Runtime"));
        private static readonly Assembly NetStandard = Assembly.Load(new AssemblyName("netstandard"));

        public DynamicRule() //IEnumerable<Type> referencedTypes, IEnumerable<string> usings)
        {
            References = GetReferences(new[] { typeof(object) } );

            //StandardHeader = GetUsingStatements(usings);
        }
        public int CSharpScriptEvaluate() //string lambda)
        {
            //var returnTypeAsString = GetCSharpRepresentation(typeof(T), true);
            string outerClass = "public class Wrapper { public static int DoStuff() { return 1+2; } }"; //  StandardHeader + $"public static class Wrapper {{ public static {returnTypeAsString} expr = {lambda}; }}";

            var compilation = CSharpCompilation.Create("FilterCompiler_" + Guid.NewGuid(),
                                                        new[] { CSharpSyntaxTree.ParseText(outerClass) },
                                                        References,
                                                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var assemblyLoadContext = new CollectibleAssemblyLoadContext();
            using var ms = new MemoryStream();

            var cr = compilation.Emit(ms);
            if (!cr.Success)
            {
                throw new InvalidOperationException("Error in expression: " + cr.Diagnostics.First(e =>
                    e.Severity == DiagnosticSeverity.Error).GetMessage());
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = assemblyLoadContext.LoadFromStream(ms);

            var outerClassType = assembly.GetType("Wrapper");

            var exprField = outerClassType.GetMethod("DoStuff", BindingFlags.Public | BindingFlags.Static);
            // ReSharper disable once PossibleNullReferenceException
            var result = exprField.Invoke(null, null);
            return 1;
        }

        private static PortableExecutableReference[] GetReferences(IEnumerable<Type> referencedTypes)
        {
            //var standardReferenceHints = new[] { typeof(string), typeof(IQueryable), typeof(IReadOnlyCollection<>), typeof(EF), typeof(Enumerable) };
            //var allHints = standardReferenceHints.Concat(referencedTypes);
            var includedAssemblies = new[] { SystemRuntime, NetStandard }.Concat(referencedTypes.Select(t => t.Assembly)).Distinct();

            return includedAssemblies.Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray();
        }
    }
}
