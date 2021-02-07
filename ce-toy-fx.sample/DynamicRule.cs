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
        public static Process CreateFromString(string program) //string lambda)
        {
            //var returnTypeAsString = GetCSharpRepresentation(typeof(T), true);
            //string outerClass = "public class Wrapper { public static int DoStuff() { return 1+2; } }"; //  StandardHeader + $"public static class Wrapper {{ public static {returnTypeAsString} expr = {lambda}; }}";

            //The location of the .NET assemblies
            var systemPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var compilation = CSharpCompilation.Create(
                "DynamicRuleCompiler_" + Guid.NewGuid(),
                new[] { CSharpSyntaxTree.ParseText(program) },
                new[] { 
                    MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(systemPath, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(systemPath, "System.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(systemPath, "System.Core.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(systemPath, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(systemPath, "System.Linq.dll")),
                    MetadataReference.CreateFromFile(typeof(Applicant).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Expression).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(DynamicRule).Assembly.Location)
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

            var outerClassType = assembly.GetType("ce_toy_fx.sample.SampleProcessDynamic");

            var exprField = outerClassType.GetMethod("GetProcess", BindingFlags.Public | BindingFlags.Static);
            var result = exprField.Invoke(null, null);
            return (Process)result;
        }
    }

    public class CodeGenerator
    {
        public abstract class Rule
        {
            public string Name { get; set; }
        }
        /*
         Variables.Age.Value
            .SelectMany(_ => Variables.Salary.Value, (Age, Salary) => new { Age, Salary })
            .SelectMany(_ => Variables.Role.Value, (x, Role) => new { x.Age,x.Salary, Role })
            .SelectMany(_ => Variables.CreditScore.Value, (x, CreditScore) => new { x.Age,x.Salary,x.Role, CreditScore })
            .Where(x => x.Age < 18 && x.Salary > 100 && x.Role == Roles.Primary)
            .Select(x => x.Age > 10 ? passed : passed).Lift() 
         */
        public class PolicyRule : Rule
        {
            public string[] Variables { get; set; } = new string[] { "Age", "Salary", "Role", "CreditScore" };
            public string Condition { get; set; } = "x => x.Age < 18 && x.Salary > 100 && x.Role == Roles.Primary";
            public string Projection { get; } = "pass";
        }

        /*
                Condition.Eval("...").
                    SelectMany(c => c ?                         
                        new RuleDef[]
                        {
                           ... children...
                        }.Join(),
                        Wrap(Unit.Value), (_,x) => x);
         */

        public class GroupRule : Rule
        {
            public string Condition { get; set; } = "x => x.Age < 18 && x.Salary > 100 && x.Role == Roles.Primary";

            public List<Rule> Children { get; set; }
        }

        public static string GenerateCodeForPolicyRule()
        {
            return GenerateCodeForPolicyRule(
                new string[] { "Age", "Salary", "Role", "CreditScore" },
                "x => x.Age < 18 && x.Salary > 100 && x.Role == Roles.Primary",
                "x => x.Age > 10 ? passed : passed", "rulename"
                );
        }

        public static string GenerateCodeForPolicyRule(
            string[] variables, 
            string condition, 
            string projection,
            string name)
        {
            var code = new StringBuilder();
            code.Append(GenerateVariableContext(variables));

            if (condition != null)
            {
                code.Append(".Where(").Append(condition).AppendLine(")");
            }

            code.Append(".Select(").Append(projection).AppendLine(")");
            code.AppendLine(".Lift()");
            if (name != null)
            {
                code.Append($".LogContext(\"").Append(name).AppendLine("\")");
            }
            return code.ToString();
        }

        public static string GenerateVariableContext(string[] variables)
        {
            var code = new StringBuilder();
            foreach (var (variable, i) in variables.Select((s, i) => (s, i)))
            {
                switch (i)
                {
                    case 0:
                        code.AppendLine($"Variables.{variable}.Value");
                        break;
                    case 1:
                        var prev = variables[i - 1];
                        code.AppendLine($".SelectMany(_ => Variables.{variable}.Value, ({prev}, {variable}) => new {{ {prev}, {variable} }})");
                        break;
                    default:
                        var allPrev = string.Join(',', variables.Take(i).Select(v => $"x.{v}"));
                        code.AppendLine($".SelectMany(_ => Variables.{variable}.Value, (x, {variable}) => new {{ {allPrev}, {variable} }})");
                        break;
                }
            }
            return code.ToString();
        }
    }
}
