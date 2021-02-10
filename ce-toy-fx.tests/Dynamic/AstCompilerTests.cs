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
        [InlineData(ProjectionType.Policy, "PassUnit.Value")]
        public void CompileProjectionAmount(ProjectionType type, string expectedProjection)
        {
            var projection = new Projection { Value = "x", ProjectionType = type };

            var compiler = new AstCompiler();
            projection.Accept(compiler);

            var code = compiler.ToString();
            output.WriteLine(code);
            Assert.NotNull(code);
            Assert.Equal($".Select(Vars => {expectedProjection})", code.TrimEnd());
        }

        [Fact]
        public void CompilingMRuleDef()
        {
            var mrule = new MRuleDef
            {
                Name = "Test rule",
                Condition = "Vars.Age.Max() < 55",
                Projection = new Projection { Value = "Math.Min(Vars.Amount, 1000)", ProjectionType = ProjectionType.Amount },
                //VariableReferences = new string[] { "Age", "Amount" }
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
                        Projection = new Projection { Value = "Math.Min(Vars.Amount, 1000)", ProjectionType = ProjectionType.Amount },
                    },
                    new MRuleDef
                    {
                        Name = "Child rule 2",
                        Condition = "Vars.Age.Max() < 35",
                        Projection = new Projection { Value = "Math.Min(Vars.Amount, 2000)", ProjectionType = ProjectionType.Amount },
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
                            Projection = new Projection { ProjectionType = ProjectionType.Policy },
                        },
                        new SRuleDef
                        {
                            Name = "srule 2",
                            Condition = "Vars.Amount < 1000",
                            Projection = new Projection { ProjectionType = ProjectionType.Policy },
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

        [Fact]
        public void CompileSCase()
        {
            var case1 = new SRuleDef
            {
                Name = "case 1",
                Condition = "Vars.Salary > 1000",
                Projection = new Projection { ProjectionType = ProjectionType.Policy },
            };

            var case2 = new SRuleDef
            {
                Name = "case 2",
                Condition = "Vars.Salary > 1500",
                Projection = new Projection { ProjectionType = ProjectionType.Policy },
            };

            var sCaseRule = new SRuleCase
            {
                Name = "scase rule",
                Variable = "Age",
                Children = new List<(Condition, SRule)>
                {
                    ("Vars.Age < 18", case1),
                    ("Vars.Age >= 18", case2)
                }
            };

            var compiler = new AstCompiler();
            sCaseRule.Accept(compiler);

            var code = compiler.ToString();
            output.WriteLine(code);
            Assert.NotNull(code);
        }
    }
}
