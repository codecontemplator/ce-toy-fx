﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ce_toy_fx
{
    public static class RuleExprLift
    {
        public delegate Option<T> VoteMethod<T>(IEnumerable<Option<T>> input);

        private class VoteMethods
        {
            public static Option<PassUnit> AllShouldPass(IEnumerable<Option<PassUnit>> input) => input.Any(x => !x.isSome) ? Option<PassUnit>.None : Option<PassUnit>.Some(PassUnit.Value);
            public static Option<FailUnit> NoneShouldPass(IEnumerable<Option<FailUnit>> input) => input.Any(x => x.isSome) ? Option<FailUnit>.None : Option<FailUnit>.Some(FailUnit.Value);
            public static Option<T> MinValue<T>(IEnumerable<Option<T>> input)
            {
                var values = input.Where(x => x.isSome).Select(x => x.value).ToList();
                return values.Any() ? Option<T>.Some(values.Min()) : Option<T>.None;
            }
        }

        public static RuleExprAst<Unit, RuleExprContext<Unit>> Lift(this RuleExprAst<PassUnit, RuleExprContext<string>> sRuleExprAst)
        {
            return sRuleExprAst.Lift(VoteMethods.AllShouldPass).Select(_ => Unit.Value);
        }

        public static RuleExprAst<Unit, RuleExprContext<Unit>> Lift(this RuleExprAst<FailUnit, RuleExprContext<string>> sRuleExprAst)
        {
            return sRuleExprAst.Lift(VoteMethods.NoneShouldPass).Select(_ => Unit.Value);
        }

        public static RuleExprAst<Unit, RuleExprContext<Unit>> Lift(this RuleExprAst<Amount, RuleExprContext<string>> sRuleExprAst)
        {
            return sRuleExprAst.Lift(VoteMethods.MinValue).Apply();
        }

        public static RuleExprAst<T, RuleExprContext<Unit>> Lift<T>(this RuleExprAst<T, RuleExprContext<string>> sRuleExprAst, VoteMethod<T> vote)
        {
            var sRule = sRuleExprAst.ExceptionContext().Compile();
            var sKeys = sRuleExprAst.GetKeys();
            return new RuleExprAst<T, RuleExprContext<Unit>>
            {
                Expression = mcontext => sRule.LiftImpl(vote, sKeys)(mcontext)
            };
        }

        public static RuleExpr<T, RuleExprContext<Unit>> LiftImpl<T>(this RuleExpr<Option<T>, RuleExprContext<string>> sRule, VoteMethod<T> vote, IEnumerable<string> sKeys)
        {
            return mcontext =>
            {
                var scontext = mcontext.WithSelector<string>(null);
                var result = new List<Option<T>>();
                foreach (var applicant in mcontext.Applicants)
                {
                    scontext = scontext.WithSelector(applicant.Key);
                    var (maybea, newSContext) = sRule(scontext);
                    scontext = newSContext;
                    if (!maybea.IsSome(out var a))
                        throw new Exception("Internal error. Lifting failed. Exception scope did not catch error as expected.");
                    result.Add(a);
                }

                return (vote(result), scontext.WithSelector(Unit.Value));
            };
        }
    }
}