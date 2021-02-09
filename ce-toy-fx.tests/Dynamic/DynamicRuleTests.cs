using ce_toy_fx.sample;
using ce_toy_fx.sample.Dynamic;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace ce_toy_fx.tests.Dynamic
{
    public class DynamicRuleTests
    {
        private readonly ITestOutputHelper output;

        public DynamicRuleTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test()
        {
            var sampleProcessAst = new MRuleJoin
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

            var process = DynamicRule.CreateFromAst(sampleProcessAst);

            Assert.NotNull(process);
        }
    }
}
