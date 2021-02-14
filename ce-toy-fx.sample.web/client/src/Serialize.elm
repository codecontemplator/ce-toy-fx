module Serialize exposing (encodeRuleList, toJson, mkHttpRequestBody)

import Model exposing (..)
import Json.Encode as Encode
import Maybe.Extra

encodeRule : TreeNode Rule -> Encode.Value
encodeRule (TreeNode n (Rule r)) = 
    case r.type_ of
    Group -> 
        encodeRuleList (Just r.name) n.children
    Policy -> 
        Encode.object 
        [
            ("name", Encode.string r.name),
            ("type", Encode.string "MRuleDef"),
            ("condition", Encode.string r.condition),
            ("projection", Encode.object [ ("projectionType", Encode.string "Policy") ])
        ]
    Limit ->
        Encode.object 
        [
            ("name", Encode.string r.name),
            ("type", Encode.string "MRuleDef"),
            ("condition", Encode.string r.condition),
            ("projection", 
                Encode.object 
                [ ("projectionType", Encode.string "Amount") 
                , ("value", Encode.string r.projection)
                ]
            )
        ]

encodeRuleList : Maybe String -> List (TreeNode Rule) -> Encode.Value
encodeRuleList maybeName rules = 
    let
        joined = Encode.object ([ ("type", Encode.string (if perApplicant then "SRuleJoin" else "MRuleJoin")), ("children", Encode.list encodeRule rules) ] ++ Maybe.Extra.toList (Maybe.map (\s -> ("name", Encode.string s)) maybeName))
        perApplicant = List.any (\(TreeNode _ (Rule child)) -> child.scope == AnyApplicant) rules
    in        
        if perApplicant then
        Encode.object [ ("type", Encode.string "SRuleLift"), ("child", joined)]
        else
        joined

toJson : List (TreeNode Rule) -> String
toJson process = encodeRuleList Nothing process |> Encode.encode 4

mkHttpRequestBody : AppModel -> String
mkHttpRequestBody model = toJson model.process
