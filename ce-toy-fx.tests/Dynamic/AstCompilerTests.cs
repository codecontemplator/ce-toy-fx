using ce_toy_fx.sample.Dynamic;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace ce_toy_fx.tests
{
    public class AstCompilerTests
    {
        private readonly ITestOutputHelper output;

        public AstCompilerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData(ProjectionType.Amount, "new Amount(x)")]
        [InlineData(ProjectionType.PolicyAccept, "PassUnit.Value")]
        public void CompileProjectionAmount(ProjectionType type, string expectedProjection)
        {
            var projection = new Projection { Value = "x", Type = type };

            var compiler = new AstCompiler();
            projection.Accept(compiler);

            var code = compiler.ToString();
            output.WriteLine(code);
            Assert.NotNull(code);
            Assert.Equal($".Select(Vars => {expectedProjection})\r\n", code);
        }

        [Fact]
        public void CompilingMRuleDef()
        {
            var mrule = new MRuleDef
            {
                Name = "Test rule",
                Condition = "Vars.Age.Max() < 55",
                Projection = new Projection { Value = "Math.Min(Vars.Amount, 1000)", Type = ProjectionType.Amount },
                VariableReferences = new string[] { "Age", "Amount" }
            };

            var compiler = new AstCompiler();
            mrule.Accept(compiler);

            var code = compiler.ToString();
            output.WriteLine(code);
            Assert.NotNull(code);
        }

        [Fact]
        public void CompileMRuleJoin()
        {
            var mrulejoin = new MRuleJoin
            {
                Name = "Join rule",
                Children = new List<MRule>
                {
                    new MRuleDef
                    {
                        Name = "Child rule 1",
                        Condition = "Vars.Age.Max() < 55",
                        Projection = new Projection { Value = "Math.Min(Vars.Amount, 1000)", Type = ProjectionType.Amount },
                        VariableReferences = new string[] { "Age", "Amount" }
                    },
                    new MRuleDef
                    {
                        Name = "Child rule 2",
                        Condition = "Vars.Age.Max() < 35",
                        Projection = new Projection { Value = "Math.Min(Vars.Amount, 2000)", Type = ProjectionType.Amount },
                        VariableReferences = new string[] { "Age", "Amount" }
                    }
                }
            };

            var compiler = new AstCompiler();
            mrulejoin.Accept(compiler);

            var code = compiler.ToString();
            output.WriteLine(code);
            Assert.NotNull(code);
        }

        [Fact]
        public void CompileSRule()
        {
            var liftedrule = new SRuleLift
            {
                Child = new SRuleJoin
                {
                    Name = "Srule group",
                    Children = new List<SRule>
                    {
                        new SRuleDef
                        {
                            Name = "srule 1",
                            Condition = "Vars.Salary < 1000 && Vars.Age < 25",
                            Projection = new Projection { Type = ProjectionType.PolicyAccept },
                            VariableReferences = new string[] { "Salary", "Age" }
                        },
                        new SRuleDef
                        {
                            Name = "srule 2",
                            Condition = "Vars.Amount < 1000",
                            Projection = new Projection { Type = ProjectionType.PolicyAccept },
                            VariableReferences = new string[] { "Amount" }
                        }
                    }
                }
            };

            var compiler = new AstCompiler();
            liftedrule.Accept(compiler);

            var code = compiler.ToString();
            output.WriteLine(code);
            Assert.NotNull(code);
        }
    }
}
