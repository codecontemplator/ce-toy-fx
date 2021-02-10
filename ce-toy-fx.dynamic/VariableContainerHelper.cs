using System.Linq;
using System.Text.RegularExpressions;

namespace ce_toy_fx.sample.Dynamic
{
    public static class VariableContainerHelper
    {
        public static string[] GetVariablesFromString(string code)
        {
            if (code == null) return new string[] { };
            var matches = Regex.Matches(code, "Vars\\.[A-Za-z0-9]+");
            return matches.Select(m => m.Value.Replace("Vars.", "")).ToArray();
        }
    }
}
