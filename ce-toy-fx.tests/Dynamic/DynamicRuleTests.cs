using ce_toy_fx.sample;
using ce_toy_fx.sample.Dynamic;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ce_toy_fx.tests.Dynamic
{
    public class DynamicRuleTests
    {
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
                        Projection = new Projection { Value = "Math.Min(Vars.Amount, 1000)", ProjectionType = ProjectionType.Amount },
                    },
                    new MRuleDef
                    {
                        Name = "Child rule 2",
                        Condition = "Vars.Age.Max() < 35",
                        Projection = new Projection { Value = "Math.Min(Vars.Amount, 500)", ProjectionType = ProjectionType.Amount },
                    }
                }
            };

            var process = DynamicRule.CreateFromAst(sampleProcessAst);

            Assert.NotNull(process);

            Assert.Equal(1, process.Keys.Count);
            Assert.Equal("Age", process.Keys[0]);

            var applicants = new List<Applicant>
            {
                new Applicant
                {
                    Id = "a1",
                    KeyValueMap = new Dictionary<string,object>
                    {
                        { "Age", 25 }
                    }.ToImmutableDictionary()
                },
                new Applicant
                {
                    Id = "a2",
                    KeyValueMap = new Dictionary<string,object>
                    {
                        { "Age", 45 }
                    }.ToImmutableDictionary()
                }
            };

            var evalResult = process.RuleExpr(new RuleExprContext<Unit>
            {
                Log = ImmutableList<LogEntry>.Empty,
                Amount = 1500,
                Applicants = applicants.ToDictionary(x => x.Id).ToImmutableDictionary()
            });

            Assert.True(evalResult.Item1.isSome);
            Assert.Equal(1000, evalResult.Item2.Amount);
        }
    }
}
