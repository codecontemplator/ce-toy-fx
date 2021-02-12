module Main exposing (..)

import Browser
import Html exposing (Html, button, div, text)
import Html.Attributes exposing (class, type_)
import Html.Events exposing (onClick)
import Html.Keyed as Keyed
import Bootstrap.Grid as Grid
import Bootstrap.Grid.Col as Col
import Bootstrap.CDN as CDN
import Bootstrap.Form as Form
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Select as Select
import Bootstrap.Form.Checkbox as Checkbox
import Bootstrap.Form.Radio as Radio
import Bootstrap.Form.Textarea as Textarea
import Bootstrap.Form.Fieldset as Fieldset
import Html.Attributes exposing (hidden)

-- https://github.com/evancz/elm-todomvc/blob/master/src/Main.elm

main =
  Browser.sandbox { init = { process = [], nextId = 0 }, update = update, view = view }

type alias AppModel = { process : List (TreeNode Rule), nextId : Int }

type TreeNode a = TreeNode { header : String, isExpanded : Bool, id : Int } a

type RuleType = Limit | Policy | Group | Vote
type RuleScope = AllApplicants | AnyApplicant
type Rule = Rule { type_ : RuleType, name : String, condition : String, projection : String, children : List Rule, scope : RuleScope }

type AppMsg = AddRule | ToggleTreeNode Int | UpdateRuleType Int RuleType

update : AppMsg -> AppModel -> AppModel
update msg model =
  case msg of
    AddRule ->
      let
        name = "new rule"
        mrule = Rule { type_ = Limit, name = name, condition = "<condition>", projection = "<projection>", children = [], scope = AllApplicants }
        node = TreeNode { id = model.nextId, header = name, isExpanded = False } mrule
      in
        { model | nextId = model.nextId + 1, process = model.process ++ [ node ] }
    ToggleTreeNode id -> 
      let
        updateNode (TreeNode n pl) = if id == n.id then TreeNode { n | isExpanded = not n.isExpanded } pl else TreeNode n pl
      in
        { model | process = List.map updateNode model.process }
    UpdateRuleType id newType ->
      let
        updateNode (TreeNode n (Rule r)) = if id == n.id then TreeNode n (Rule { r | type_ = newType }) else TreeNode n (Rule r)
      in
        { model | process = List.map updateNode model.process }

view : AppModel -> Html AppMsg
view model =
        Grid.container []
          [ CDN.stylesheet
            , Grid.simpleRow [ Grid.col [] [ button [ type_ "button", class "btn btn-primary", onClick AddRule ] [ text "Add Rule" ] ] ]
            , Grid.simpleRow [ Grid.col [] [ viewRuleList model.process ] ]
          ]

-- http://elm-bootstrap.info/form
viewRuleList : List (TreeNode Rule) -> Html AppMsg
viewRuleList ruleList = 
    let
        viewKeydRule : (TreeNode Rule) -> (String, Html AppMsg)
        viewKeydRule (TreeNode node (Rule rule)) = 
          let
            updateRuleType s = UpdateRuleType node.id (
                case s of
                  "Policy" -> Policy
                  "Limit" -> Limit 
                  "Group" -> Group
                  _ -> Vote
              )
            viewRuleDetails =
              if node.isExpanded then
                [ Form.form [] 
                  [ Form.group []
                      [ Form.label [ Html.Attributes.for "rule-type-selector" ] [ text "Rule type" ]
                      , Select.select [ Select.id "rule-type-selector", Select.onChange updateRuleType]
                          [ Select.item [] [ text "Policy"]
                          , Select.item [] [ text "Limit"]
                          , Select.item [] [ text "Group"]
                          , Select.item [] [ text "Vote"]
                          ]
                      ]
                  , Form.group [ Form.attrs [ Html.Attributes.hidden (List.member rule.type_ [ Policy, Limit ] |> not) ] ]
                      [ Form.label [Html.Attributes.for "rule-condition" ] [ text "Condition"]
                      , Input.text [ Input.id "rule-condition" ]
                      , Form.help [] [ text "Example: Vars.Credit < 1000 && Vars.Age >= 20" ]
                      ]
                  , Form.group []
                      [ Form.label [Html.Attributes.for "rule-projection" ] [ text "Projection"]
                      , Input.text [ Input.id "rule-projection" ]
                      , Form.help [] [ text "Example: Vars.Amount - Vars.Credit" ]
                      ]
                  ]                
                ]
              else [ ]                
          in
              ( String.fromInt node.id
              , Html.li [ class "list-group-item" ] 
                [ Grid.container [] 
                    (
                      [ Grid.simpleRow 
                        [ Grid.col [ Col.xsAuto ] [ button [ class "btn", class "btn-lnk", onClick (ToggleTreeNode node.id) ] [ Html.h4 [] [ text rule.name ] ] ] 
--                          , Grid.col [ ] [ ]
--                          , Grid.col [ Col.xsAuto ] [ button [ onClick (ToggleTreeNode node.id)] [ text (if node.isExpanded then "Collapse" else "Expand") ] ] 
                        ]
                      , Grid.simpleRow 
                        [ Grid.col [] viewRuleDetails ] 
                        ]
                    )
                ]
              )
    in
        Keyed.ul [ class "list-group" ] <|
                List.map viewKeydRule ruleList
