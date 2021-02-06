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
        //private readonly PortableExecutableReference[] References;
        //private readonly string StandardHeader;

        //private static readonly Assembly SystemRuntime = Assembly.Load(new AssemblyName("System.Runtime"));
        //private static readonly Assembly NetStandard = Assembly.Load(new AssemblyName("netstandard"));

        public DynamicRule() //IEnumerable<Type> referencedTypes, IEnumerable<string> usings)
        {
            //References = GetReferences(new[] { typeof(object) } );

            //StandardHeader = GetUsingStatements(usings);
        }
        public int CSharpScriptEvaluate() //string lambda)
        {
            //var returnTypeAsString = GetCSharpRepresentation(typeof(T), true);
            string outerClass = "public class Wrapper { public static int DoStuff() { return 1+2; } }"; //  StandardHeader + $"public static class Wrapper {{ public static {returnTypeAsString} expr = {lambda}; }}";

            var compilation = CSharpCompilation.Create(
                "DynamicRuleCompiler_" + Guid.NewGuid(),
                new[] { CSharpSyntaxTree.ParseText(outerClass) },
                new[] { 
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location) 
                },
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
            var result = exprField.Invoke(null, null);
            return (int)result;
        }
    }
}
