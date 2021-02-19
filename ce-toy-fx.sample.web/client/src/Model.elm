module Model exposing (..)

type ProcessView = UI | Raw
type alias AppModel = { process : List (TreeNode Rule), processView : ProcessView, nextId : Int, application : Application, response : Result String String }

type TreeNode a = TreeNode { header : String, isExpanded : Bool, id : Int, children : List (TreeNode a), isHeaderEditEnabled : Bool } a

type RuleType = Limit | Policy | Group
--type RuleScope = AllApplicants | AnyApplicant
type RuleAggregationType = All | Single
type Rule = Rule { type_ : RuleType, name : String, condition : String, projection : String, ruleAggregationType : RuleAggregationType }

type alias Applicant = { id : String, keyValues : List (String, String) }
type alias Application = { applicants : List Applicant, requestedAmount : Int }

defaultKeyValues : List (String,String)
defaultKeyValues = [
        ("Age", "25"),
        ("Salary", "1000"),
        ("Credit", "100")
    ]

initialProcess : List (TreeNode Rule)
initialProcess = 
    [ TreeNode { header = "Max Absolute Limit", isExpanded = False, id = 1, children = [], isHeaderEditEnabled = False } 
               (Rule { type_ = Limit, name = "Max Absolute Limit", condition = "", projection = "Math.Min(Vars.Amount, 1000)", ruleAggregationType = All}),
      TreeNode { header = "Max Age Policy", isExpanded = False, id = 2, children = [], isHeaderEditEnabled = False } 
               (Rule { type_ = Policy, name = "Max Age Policy", condition = "Vars.Age < 75", projection = "", ruleAggregationType = Single})                       
    ]

initialAppModel : AppModel
initialAppModel =
            { process = initialProcess
            , processView = UI
            , nextId = 10
            , response = Ok "No response yet"
            , application = { applicants = [], requestedAmount = 1000 } 
            }