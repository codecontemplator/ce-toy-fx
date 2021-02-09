﻿using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

namespace ce_toy_fx.sample.Dynamic
{
    public abstract class AstVisitor
    {
        public virtual void Visit(Condition condition) { }
        public virtual void Visit(Projection projection) { }
        public virtual void Visit(MRuleDef mruleDef) { }
        public virtual void Visit(MRuleJoin mruleJoin) { }
        public virtual void Visit(MRuleCase mCase) { }
        public virtual void Visit(SRuleLift sLift) { }
        public virtual void Visit(SRuleDef sRuleDef) { }
        public virtual void Visit(SRuleJoin sRuleJoin) { }
    }

    public interface AstNode
    {
        void Accept(AstVisitor visitor);
    }

    public class Condition : AstNode
    {
        public string Value { get; set; }

        public void Accept(AstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static implicit operator Condition(string s) => new Condition { Value = s };
    }

    public enum ProjectionType
    {
        Amount, Accept
    }
    public class Projection : AstNode
    {
        public string Value { get; set; }
        
        public void Accept(AstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public ProjectionType Type { get; set; }
    }

    public abstract class MRule : AstNode
    {
        public abstract void Accept(AstVisitor visitor);
    }

    public class MRuleDef : MRule
    {

        public string Name { get; set; }
        public Condition Condition { get; set; }
        public Projection Projection { get; set; }
        public string[] VariableReferences { get; set; }

        public override void Accept(AstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class MRuleJoin : MRule
    {
        public string Name { get; set; }
        public List<MRule> Children { get; set; }

        public override void Accept(AstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class MRuleCase : MRule
    {
        public List<(Condition,MRule)> Children { get; set; }

        public override void Accept(AstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class SRuleLift : MRule
    {
        public SRule Child { get; set; }

        public override void Accept(AstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public abstract class SRule : AstNode
    {
        public abstract void Accept(AstVisitor visitor);
    }

    public class SRuleDef : SRule
    {
        public string Name { get; set; }
        public Condition Condition { get; set; }
        public Projection Projection { get; set; }
        public string[] VariableReferences { get; set; }

        public override void Accept(AstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class SRuleJoin : SRule
    {
        public string Name { get; set; }
        public List<SRule> Children { get; set; }

        public override void Accept(AstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class AstCompiler : AstVisitor
    {
        private StringBuilder _stringBuilder = new StringBuilder();

        public override void Visit(Condition condition)
        {
            _stringBuilder.Append(".Where(Vars => ").Append(condition.Value).AppendLine(")");
        }

        public override void Visit(Projection projection)
        {
            _stringBuilder.Append(".Select(Vars => ");
            switch(projection.Type)
            {
                case ProjectionType.Amount:
                    _stringBuilder.Append($"new Amount({projection.Value})");
                    break;
                case ProjectionType.Accept:
                    _stringBuilder.Append("PassUnit.Value");
                    break;
                default:
                    throw new Exception("Unhandled projection type");
            }            
            _stringBuilder.AppendLine(")");
        }

        public override void Visit(MRuleDef mruleDef)
        {
            _stringBuilder.AppendLine("(");
            _stringBuilder.Append(GenerateVariableContext(mruleDef.VariableReferences, true));

            mruleDef.Condition?.Accept(this);
            mruleDef.Projection.Accept(this);

            if (!string.IsNullOrEmpty(mruleDef.Name))
            {
                _stringBuilder.Append($".LogContext(\"").Append(mruleDef.Name).AppendLine("\")");
            }

            _stringBuilder.AppendLine(").Apply()");
        }

        public override void Visit(MRuleJoin joinRule)
        {
            _stringBuilder.AppendLine("new RuleExprAst<Unit, RuleExprContext<Unit>>[] {");
            foreach(var (childrule, i) in joinRule.Children.Select((x,i) => (x,i)))
            {
                if (i > 0)
                    _stringBuilder.AppendLine(",");

                childrule.Accept(this);
            }

            _stringBuilder.Append("}.Join()");
            if (!string.IsNullOrEmpty(joinRule.Name))
                _stringBuilder.Append($".LogContext(\"").Append(joinRule.Name).Append("\")");
            _stringBuilder.AppendLine();
        }

        public override void Visit(MRuleCase mCase)
        {
            throw new NotImplementedException();
        }

        public override void Visit(SRuleLift sRuleLift)
        {
            sRuleLift.Child.Accept(this);
            _stringBuilder.AppendLine(".Lift()");
        }

        public override void Visit(SRuleDef sRuleDef)
        {
            _stringBuilder.AppendLine("(");
            _stringBuilder.Append(GenerateVariableContext(sRuleDef.VariableReferences, false));

            sRuleDef.Condition?.Accept(this);
            sRuleDef.Projection.Accept(this);

            if (!string.IsNullOrEmpty(sRuleDef.Name))
            {
                _stringBuilder.Append($".LogContext(\"").Append(sRuleDef.Name).AppendLine("\")");
            }

            _stringBuilder.AppendLine(").Apply()");
        }

        public override void Visit(SRuleJoin sRuleJoin)
        {
            _stringBuilder.AppendLine("new RuleExprAst<Unit, RuleExprContext<string>>[] {");
            foreach (var (childrule, i) in sRuleJoin.Children.Select((x, i) => (x, i)))
            {
                if (i > 0)
                    _stringBuilder.AppendLine(",");

                childrule.Accept(this);
            }

            _stringBuilder.Append("}.Join()");
            if (!string.IsNullOrEmpty(sRuleJoin.Name))
                _stringBuilder.Append($".LogContext(\"").Append(sRuleJoin.Name).Append("\")");
            _stringBuilder.AppendLine();
        }

        private static string GenerateVariableContext(string[] variables, bool mrule)
        {
            var code = new StringBuilder();
            string postfix = mrule ? "s" : "";
            foreach (var (variable, i) in variables.Select((s, i) => (s, i)))
            {
                var refBy = variable == "Amount" ? (mrule ? "Dsl.GetAmount<Unit>()" : "Dsl.GetAmount<string>()") : $"Variables.{variable}.Value{postfix}";
                switch (i)
                {
                    case 0:
                        code.AppendLine(refBy);
                        break;
                    case 1:
                        var prev = variables[i - 1];
                        code.AppendLine($".SelectMany(_ => {refBy}, ({prev}, {variable}) => new {{ {prev}, {variable} }})");
                        break;
                    default:
                        var allPrev = string.Join(',', variables.Take(i).Select(v => $"x.{v}"));
                        code.AppendLine($".SelectMany(_ => {refBy}, (x, {variable}) => new {{ {allPrev}, {variable} }})");
                        break;
                }
            }

            if (variables.Length == 1)
                code.AppendLine($".Select(x => new {{ {variables[0]} = x }})");
            return code.ToString();
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}
