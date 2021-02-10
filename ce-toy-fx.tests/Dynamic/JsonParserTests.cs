using ce_toy_fx.sample.Dynamic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ce_toy_fx.tests.Dynamic
{
    public class JsonParserTests
    {
        private readonly ITestOutputHelper output;

        public JsonParserTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test()
        {
            var json = GetJsonData();
            output.WriteLine(json);

            var root = JsonParser.ParseMRule(json);

            Assert.IsType<MRuleJoin>(root);
            Assert.Equal("Sample process", ((MRuleJoin)root).Name);
            Assert.Equal(3, ((MRuleJoin)root).Children.Count);
            Assert.IsType<MRuleDef>(((MRuleJoin)root).Children[0]);
            Assert.Equal("Absolute max amount", ((MRuleDef)((MRuleJoin)root).Children[0]).Name);
            Assert.Equal("Math.Min(Vars.Amount, 100)", ((MRuleDef)((MRuleJoin)root).Children[0]).Projection.Value);
            Assert.Equal(ProjectionType.Amount, ((MRuleDef)((MRuleJoin)root).Children[0]).Projection.ProjectionType);
            Assert.IsType<MRuleDef>(((MRuleJoin)root).Children[1]);
            Assert.Equal("MinTotalSalary", ((MRuleDef)((MRuleJoin)root).Children[1]).Name);
            Assert.Null(((MRuleDef)((MRuleJoin)root).Children[1]).Projection.Value);
            Assert.Equal(ProjectionType.Policy, ((MRuleDef)((MRuleJoin)root).Children[1]).Projection.ProjectionType);
            Assert.IsType<SRuleLift>(((MRuleJoin)root).Children[2]);
            Assert.NotNull(((SRuleLift)((MRuleJoin)root).Children[2]).Child);
            Assert.IsType<SRuleJoin>(((SRuleLift)((MRuleJoin)root).Children[2]).Child);
            Assert.Single(((SRuleJoin)((SRuleLift)((MRuleJoin)root).Children[2]).Child).Children);
            Assert.IsType<SRuleDef>(((SRuleJoin)((SRuleLift)((MRuleJoin)root).Children[2]).Child).Children[0]);
            Assert.Equal("Credit limit", ((SRuleDef)((SRuleJoin)((SRuleLift)((MRuleJoin)root).Children[2]).Child).Children[0]).Name);
            Assert.Equal("Vars.CreditA + Vars.CreditB < 50", ((SRuleDef)((SRuleJoin)((SRuleLift)((MRuleJoin)root).Children[2]).Child).Children[0]).Condition);
            Assert.Equal(ProjectionType.Policy, ((SRuleDef)((SRuleJoin)((SRuleLift)((MRuleJoin)root).Children[2]).Child).Children[0]).Projection.ProjectionType);
        }

        private string GetJsonData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ce_toy_fx.tests.Dynamic.processdef.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
