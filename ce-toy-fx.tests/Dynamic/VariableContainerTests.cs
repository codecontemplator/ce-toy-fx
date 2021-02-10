using ce_toy_fx.sample.Dynamic;
using System.Collections.Generic;
using Xunit;

namespace ce_toy_fx.tests.Dynamic
{
    public class VariableContainerTests
    {
        [Theory]
        [InlineData("Vars.Amount < 50 && Vars.Age > 20", "Amount", "Age")]
        [InlineData(null)]
        public void TestGetVariablesFromString(string code, params string[] expectedVars)
        {
            var result = VariableContainerHelper.GetVariablesFromString(code);
            Assert.Equal(expectedVars.Length, result.Length);
            Assert.Equal(new HashSet<string>(expectedVars), new HashSet<string>(result));
        }
    }
}
