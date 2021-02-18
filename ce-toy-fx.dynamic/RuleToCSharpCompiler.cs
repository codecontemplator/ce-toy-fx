using ce_toy_fx.sample.Dynamic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace ce_toy_fx.sample
{
    class CollectibleAssemblyLoadContext : AssemblyLoadContext, IDisposable
    {
        public CollectibleAssemblyLoadContext() : base(true)
        { }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }

        public void Dispose()
        {
            Unload();
        }
    }

    public class RuleToCSharpCompiler
    {
        public static Process CreateFromAst(AstNode n, string[] namespaces, Type[] types)
        {
            var sb = new StringBuilder();
            foreach (var ns in namespaces)
                sb.AppendLine($"using {ns};");
            sb.AppendLine(@"
using System;
using System.Linq;

namespace ce_toy_fx.dynamic
{
    using RuleDef = RuleExprAst<Unit, RuleExprContext<Unit>>;

    class DynamicProcess
    {
        public static Process GetProcess()
        {
            return
            ");

            var compiler = new AstCompiler();
            n.Accept(compiler);
            sb.AppendLine(compiler.ToString());

            sb.AppendLine(@"
                .CompileToProcess(""Sample process"");
        }
    }
}
");
            return CreateFromString(sb.ToString(), types);
        }

        public static Process CreateFromString(string program, params Type[] types)
        {
            var systemPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var metaDataRefs = new List<MetadataReference>
            {
                    MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(systemPath, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(systemPath, "System.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(systemPath, "System.Core.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(systemPath, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(systemPath, "System.Linq.dll")),
                    MetadataReference.CreateFromFile(typeof(Applicant).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Expression).Assembly.Location),
            };

            foreach(var type in types)
            {
                metaDataRefs.Add(MetadataReference.CreateFromFile(type.Assembly.Location));
            }

            var compilation = CSharpCompilation.Create(
                "DynamicRuleCompiler_" + Guid.NewGuid(),
                new[] { CSharpSyntaxTree.ParseText(program) },
                metaDataRefs.ToArray(),
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

            var outerClassType = assembly.GetType("ce_toy_fx.dynamic.DynamicProcess");

            var exprField = outerClassType.GetMethod("GetProcess", BindingFlags.Public | BindingFlags.Static);
            var result = exprField.Invoke(null, null);
            return (Process)result;
        }
    }
}
