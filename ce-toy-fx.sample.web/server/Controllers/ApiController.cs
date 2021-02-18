using ce_toy_fx.sample.web.Models;
using Microsoft.AspNetCore.Mvc;
using ce_toy_fx.sample.Dynamic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ce_toy_fx.sample.web.Models.VariableTypes;

namespace ce_toy_fx.sample.web.Controllers
{
    public class ApiController : Controller
    {
        [HttpPost]
        public IActionResult Evaluate([FromBody] object jtoken)
        {
            try
            {
                var json = jtoken.ToString();
                var rootAst = JsonParser.ParseMRule(json);
                var process = RuleToCSharpCompiler.CreateFromAst(
                    rootAst, 
                    new string[] { "ce_toy_fx.sample.web.Models", "ce_toy_fx.sample.web.Models.VariableTypes" },
                    new Type[] { typeof(Variables) });

                var applicants = new List<Applicant>
                    {
                        new Applicant
                        {
                            Id = "a1",
                            KeyValueMap = new Dictionary<string,object>
                            {
                                { Variables.Age, 25 },
                                { Variables.Credit, 1000 },
                                { Variables.CreditA, 250 },
                                { Variables.CreditB, 250 },
                                { Variables.Salary, 100 },
                                { Variables.Flags, 1 },
                                { Variables.Deceased, false },
                                { Variables.Address, new Address { Street = "Street 1", PostalCode = "12345" } },
                                { Variables.Role, Roles.Primary },
                            }.ToImmutableDictionary()
                        },
                        new Applicant
                        {
                            Id = "a2",
                            KeyValueMap = new Dictionary<string,object>
                            {
                                { Variables.Age, 35 },
                                { Variables.Credit, 100 },
                                { Variables.CreditA, 25 },
                                { Variables.CreditB, 0 },
                                { Variables.Salary, 200 },
                                { Variables.Flags, 0 },
                                { Variables.Deceased, false },
                                { Variables.Address, new Address { Street = "Street 2", PostalCode = "58098" } },
                                { Variables.Role, Roles.Other },
                            }.ToImmutableDictionary()
                        }
                    };

                var evalResult = process.RuleExpr(new RuleExprContext<Unit>
                {
                    Log = ImmutableList<LogEntry>.Empty,
                    Amount = 1500,
                    Applicants = applicants.ToDictionary(x => x.Id).ToImmutableDictionary()
                });

                return Json(evalResult.Item1.isSome ? ("Granted, " + evalResult.Item2.Amount) : "Rejected");
            }
            catch(Exception e)
            {
                return Json(e.Message);
            }
        }
    }
}