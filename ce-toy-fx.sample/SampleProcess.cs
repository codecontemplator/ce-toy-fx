using ce_toy_fx.sample.VariableTypes;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace ce_toy_fx.sample
{
    using RuleDef = RuleExprAst<Unit, RuleExprContext<Unit>>;

    class SampleProcess
    {
        private static readonly PassUnit passed = PassUnit.Value;
        private static readonly FailUnit rejected = FailUnit.Value;

        private static RuleDef AbsoluteMaxAmount(int amountLimit)
        {
            return
                (
                    from amount in Dsl.GetAmount<Unit>()
                    select new Amount(Math.Min(amount, amountLimit))
                ).Apply();
        }

        private static RuleDef MaxAmountForAge(int amountLimit, int ageLimit)
        {
            return
                (
                    from amount in Dsl.GetAmount<Unit>()
                    from ages in Variables.Age.Values
                    where ages.Max() < ageLimit
                    select new Amount(Math.Min(amount, amountLimit))
                ).Apply();
        }

        private static RuleDef MaxAmountPerApplicant(int amountLimit, int ageLimit)
        {
            return
                (
                    from amount in Dsl.GetAmount<string>()
                    from age in Variables.Age.Value
                    where age < ageLimit
                    select new Amount(Math.Min(amount, amountLimit))
                ).Lift();
        }

        private static RuleDef MaxTotalDebt(double debtLimit)
        {
            return
               (
                    from creditA in Variables.CreditA.Value
                    from creditB in Variables.CreditB.Value
                    let totalCredit = creditA + creditB
                    where totalCredit < debtLimit
                    select passed
               ).Lift();
        }

        private static RuleDef MinTotalSalary(int salaryLimit)
        {
            return
                (
                    from salaries in Variables.Salary.Values
                    where salaries.Sum() > salaryLimit
                    select passed
                ).Apply();
        }

        private static RuleDef PrimaryApplicantMustHaveAddress()
        {
            return
                (
                    from role in Variables.Role.Value
                    where role == Roles.Primary
                    from address in Variables.Address.Value
                    where !address.IsValid
                    select rejected
               ).Lift();
        }

        private static RuleDef CreditScoreUnderLimit(double limit)
        {
            return
               (
                    from creditScore in Variables.CreditScore.Value
                    where creditScore < limit
                    select passed
               ).Lift();
        }

        private static RuleDef Policies(int minAge, int maxAge, int maxFlags)
        {
            return
                new RuleDef[]
                {
                    Variables.Age.Value.RejectIf     (age => age < minAge || age > maxAge, $"Age must be greater than {minAge} and less than {maxAge}"),
                    Variables.Deceased.Value.RejectIf(deceased => deceased,                $"Must be alive"),
                    Variables.Flags.Value.RejectIf   (flags => flags >= 2,                 $"Flags must be less than {maxFlags}")
                }.Join();
        }


        private static RuleDef LiftPostitivePolicy(int minAge, int maxAge, int maxFlags)
        {
            RuleExprAst<PassUnit, RuleExprContext<string>> PolicyAcceptIf<T>(RuleExprAst<T, RuleExprContext<string>> expr, Func<T, bool> predicate, string message)
            {
                return expr.Where(x => predicate(x)).Select(_ => PassUnit.Value).LogContext(message);
            }

            return
                new RuleExprAst<PassUnit, RuleExprContext<string>>[]
                {
                    PolicyAcceptIf(Variables.Age.Value, age => age >= minAge && age <= maxAge, "Age policy"),
                    PolicyAcceptIf(Variables.Deceased.Value, deceased => !deceased, "Must be alive"),
                    PolicyAcceptIf(Variables.Flags.Value, flags => flags < 2, "Flags")
                }.Join().Lift();
        }

        private static RuleDef LiftNegativePolicy(int minAge, int maxAge, int maxFlags)
        {
            RuleExprAst<FailUnit, RuleExprContext<string>> PolicyRejectIf<T>(RuleExprAst<T, RuleExprContext<string>> expr, Func<T, bool> predicate, string message)
            {
                return expr.Where(x => predicate(x)).Select(_ => FailUnit.Value).LogContext(message);
            }

            return
                new RuleExprAst<PassUnit, RuleExprContext<string>>[]
                {
                    PolicyRejectIf(Variables.Age.Value, age => age < minAge || age > maxAge, "Age policy (-)").Apply(),
                    PolicyRejectIf(Variables.Deceased.Value, deceased => deceased, "Must be alive (-)").Apply(),
                    PolicyRejectIf(Variables.Flags.Value, flags => flags >= 2, "Flags (-)").Apply()
                }.Join().Lift();
        }

        private static RuleDef CaseRule()
        {
            return
                Variables.Age.Value.Case(
                        (age => age < 18,              (from salary in Variables.Salary.Value where salary > 10 select passed).LogContext("Age < 18")),
                        (age => age >= 18 && age < 65, (from salary in Variables.Salary.Value where salary > 20 select passed).LogContext("Age >= 18 && Age < 65")),
                        (age => age >= 65,             (from salary in Variables.Salary.Value where salary > 15 select passed).LogContext("Age >= 65"))
                    ).Lift();
        }

        public static Process GetProcess()
        {
            return
                new[]
                {
                    Policies(18, 100, 2).LogContext("Policies"),
                    LiftPostitivePolicy(18, 100, 2).LogContext("LiftPolicy(+)"),
                    LiftNegativePolicy(18, 100, 2).LogContext("LiftPolicy(-)"),
                    AbsoluteMaxAmount(100).LogContext("AbsoluteMaxAmount"),
                    MaxAmountForAge(90, 71).LogContext("MaxAmountForAge 1"),
                    MaxAmountForAge(50, 50).LogContext("MaxAmountForAge 2"),
                    MaxAmountPerApplicant(80, 60).LogContext("MaxAmountPerApplicant"),
                    MaxTotalDebt(50).LogContext("MaxTotalDebt"),
                    PrimaryApplicantMustHaveAddress().LogContext("PrimaryApplicantMustHaveAddress"),
                    CreditScoreUnderLimit(0.9).LogContext("CreditScoreUnderLimit"),
                    MinTotalSalary(50).LogContext("MinTotalSalary"),
                    CaseRule().LogContext("CaseRule"),
                }.CompileToProcess("Sample process");
        }
    }
}
