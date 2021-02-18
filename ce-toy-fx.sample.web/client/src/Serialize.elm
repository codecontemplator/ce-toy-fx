module Serialize exposing (encodeRuleList, toJson, mkHttpRequestBody)

import Model exposing (..)
import Json.Encode as Encode
import Maybe.Extra

encodeRule : TreeNode Rule -> Encode.Value
encodeRule (TreeNode n (Rule r)) = 
    let 
        ruleTypeString = if r.ruleAggregationType == All then "MRuleDef" else "SRuleDef"
        unlifted = 
            case r.type_ of
                Group -> 
                    encodeRuleList (Just r.name) n.children
                Policy -> 
                    Encode.object 
                    [
                        ("name", Encode.string r.name),
                        ("type", Encode.string ruleTypeString),
                        ("condition", Encode.string r.condition),
                        ("projection", Encode.object [ ("projectionType", Encode.string "Policy") ])
                    ]
                Limit ->
                    Encode.object 
                    [
                        ("name", Encode.string r.name),
                        ("type", Encode.string ruleTypeString),
                        ("condition", Encode.string r.condition),
                        ("projection", 
                            Encode.object 
                            [ ("projectionType", Encode.string "Amount") 
                            , ("value", Encode.string r.projection)
                            ]
                        )
                    ]
    in
        if r.ruleAggregationType == All then
            unlifted
        else
            Encode.object [ ("type", Encode.string "SRuleLift"), ("child", unlifted)]

encodeRuleList : Maybe String -> List (TreeNode Rule) -> Encode.Value
encodeRuleList maybeName rules = 
    Encode.object ([ ("type", Encode.string "MRuleJoin"), ("children", Encode.list encodeRule rules) ] ++ Maybe.Extra.toList (Maybe.map (\s -> ("name", Encode.string s)) maybeName))

toJson : List (TreeNode Rule) -> String
toJson process = encodeRuleList Nothing process |> Encode.encode 4

mkHttpRequestBody : AppModel -> String
mkHttpRequestBody model = toJson model.process
