﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace ce_toy_fx
{
    public delegate (Option<T>, RuleExprContext) RuleExpr<T, RuleExprContext>(RuleExprContext input);

    public record Applicant
    {
        public string Id { get; init; }
        public IEnumerable<ILoader> Loaders { get; init; }
        public ImmutableDictionary<string, object> KeyValueMap { get; init; }
    }

    public record LogEntry
    {
        public string Message { get; init; }
        public RuleExprContextBase PreContext { get; init; }
        public RuleExprContextBase PostContext { get; init; }
        public object Value { get; init; }
    }

    public abstract record RuleExprContextBase
    {
        public int Amount { get; init; }
        public ImmutableDictionary<string, Applicant> Applicants { get; init; }
        public ImmutableList<LogEntry> Log { get; init; }
        public abstract RuleExprContextBase WithNewAmount(Option<Amount> newAmount);
        public abstract RuleExprContextBase WithLogging(LogEntry entry);
    }

    public record RuleExprContext<SelectorType> : RuleExprContextBase
    {
        public SelectorType Selector { get; init; }
        public override RuleExprContext<SelectorType> WithNewAmount(Option<Amount> newAmount) => newAmount.isSome ? this with { Amount = newAmount.value.Value } : this;
        public override RuleExprContext<SelectorType> WithLogging(LogEntry entry) => this with { Log = Log.Add(entry) };
        public RuleExprContext<NewSelectorType> WithSelector<NewSelectorType>(NewSelectorType newSelector) =>
            new RuleExprContext<NewSelectorType> { Amount = Amount, Applicants = Applicants, Log = Log, Selector = newSelector };
    }

    public class Amount
    {
        public Amount(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    public record RuleExprAst<T, RuleExprContext>
    {
        public Expression<RuleExpr<T, RuleExprContext>> Expression { get; init; }
        public RuleExpr<T, RuleExprContext> Compile() => Expression.Compile();
        public IEnumerable<string> GetKeys()
        {
            var findKeysVisitor = new FindKeysVisistor();
            findKeysVisitor.Visit(Expression);
            return findKeysVisitor.FoundKeys;
        }
    }
}