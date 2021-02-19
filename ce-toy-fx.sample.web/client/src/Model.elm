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