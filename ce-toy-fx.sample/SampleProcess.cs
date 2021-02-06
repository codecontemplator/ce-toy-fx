using ce_toy_fx.sample.VariableTypes;
using System;
using System.Linq;

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

        private static RuleDef MaxTotalDebt(double debtLimit)
        {
            return
               (
                    from creditA in Variables.CreditA.Value
                    from creditB in Variables.CreditB.Value
                    let totalCredit = creditA + creditB
                    where totalCredit < debtLimit
                    select passed
               ).Lift().Apply();
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
               ).Lift().Apply();
        }

        private static RuleDef CreditScoreUnderLimit(double limit)
        {
            return
               (
                    from creditScore in Variables.CreditScore.Value
                    where creditScore < limit
                    select passed
               ).Lift().Apply();
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

        public static RuleDef CaseRule()
        {
            return
                Variables.Age.Value.Case(
                        (age => age < 18,              (from salary in Variables.Salary.Value where salary > 10 select passed).LogContext("Age < 18")),
                        (age => age >= 18 && age < 65, (from salary in Variables.Salary.Value where salary > 20 select passed).LogContext("Age >= 18 && Age < 65")),
                        (age => age >= 65,             (from salary in Variables.Salary.Value where salary > 15 select passed).LogContext("Age >= 65"))
                    ).Lift().Apply();
        }

        public static Process GetProcess()
        {
            return
                new[]
                {
                    Policies(18, 100, 2).LogContext("Policies"),
                    AbsoluteMaxAmount(100).LogContext("AbsoluteMaxAmount"),
                    MaxTotalDebt(50).LogContext("MaxTotalDebt"),
                    PrimaryApplicantMustHaveAddress().LogContext("PrimaryApplicantMustHaveAddress"),
                    CreditScoreUnderLimit(0.9).LogContext("CreditScoreUnderLimit"),
                    MinTotalSalary(50).LogContext("MinTotalSalary"),
                    CaseRule().LogContext("CaseRule"),
                }.CompileToProcess("Sample process");
        }
    }
}
