module Model exposing (..)

type ProcessView = UI | Raw
type alias AppModel = { process : List (TreeNode Rule), processView : ProcessView, nextId : Int, response : Result String String }

type TreeNode a = TreeNode { header : String, isExpanded : Bool, id : Int, children : List (TreeNode a), isHeaderEditEnabled : Bool } a

type RuleType = Limit | Policy | Group
type RuleScope = AllApplicants | AnyApplicant
type Rule = Rule { type_ : RuleType, name : String, condition : String, projection : String, scope : RuleScope }

