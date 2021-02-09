using System.Collections.Generic;
using System.Collections.Immutable;

namespace ce_toy_fx
{
    public record Process
    {
        public string Name { get; init; }
        public RuleExpr<Unit, RuleExprContext<Unit>> RuleExpr { get; init; }
        public IImmutableList<string> Keys { get; init; }
    }

    public static class RuleExtensions
    {
        public static Process CompileToProcess(this RuleExprAst<Unit, RuleExprContext<Unit>> ruleAst, string name)
        {
            return new Process
            {
                Name = name,
                Keys = ruleAst.GetKeys().ToImmutableList(),
                RuleExpr = ruleAst.Compile()
            };
        }

        public static Process CompileToProcess(this IEnumerable<RuleExprAst<Unit, RuleExprContext<Unit>>> ruleAsts, string name)
        {
            return ruleAsts.Join().CompileToProcess(name);
        }
    }
}
