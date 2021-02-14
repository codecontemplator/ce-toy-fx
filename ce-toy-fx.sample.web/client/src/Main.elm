module Main exposing (..)

import Browser
import Html exposing (Html, button, text)
import Html.Attributes exposing (class, type_)
import Html.Events exposing (onClick)
import Html.Keyed as Keyed
import Bootstrap.Grid as Grid
import Bootstrap.Grid.Col as Col
import Bootstrap.Grid.Row as Row
import Bootstrap.CDN as CDN
import Bootstrap.Form as Form
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Select as Select
import Bootstrap.Form.Checkbox as Checkbox
-- import Bootstrap.General.HAlign as HAlign
-- import Bootstrap.Form.Radio as Radio
-- import Bootstrap.Form.Textarea as Textarea
-- import Bootstrap.Form.Fieldset as Fieldset
import Html.Attributes exposing (hidden)
import Html.Attributes exposing (selected)
import Html.Attributes
import Html.Attributes exposing (style)
import Html.Events exposing (onDoubleClick)
import Json.Decode as Decode
import Html
import Url exposing (Url)
import Model exposing (..)
import Serialize exposing (..)
import Utils exposing (onEnter)

main : Program () AppModel AppMsg
main = Browser.element { 
         init = \flags -> ({ process = [], processView = UI, nextId = 0 }, Cmd.none), 
         view = view, 
         update = update, 
         subscriptions = subscriptions
       }

type AppMsg = AddRule | ToggleTreeNode Int | UpdateRuleType Int RuleType | UpdateRuleScope Int RuleScope | AddSubRule Int | ToggleEditHeader Int | NewHeaderValue Int String | ToggleProcessView | RuleConditionUpdated Int String | MakeHttpRequest
    | RuleProjectionUpdated Int String

subscriptions : AppModel -> Sub AppMsg
subscriptions model = Sub.none

update : AppMsg -> AppModel -> (AppModel, Cmd AppMsg)
update msg model =
  let
    mapTree f (TreeNode n pl) = f (TreeNode { n | children = List.map (mapTree f) n.children } pl)
    updateProcess f = { model | process = List.map (mapTree f) model.process }
    updateNodeWithId id f = updateProcess (\(TreeNode n pl) -> if id == n.id then f (TreeNode n pl) else TreeNode n pl)
    increaseId appModel = { appModel | nextId = appModel.nextId + 1}
    mkRuleNode () = let name = ("Rule " ++ String.fromInt model.nextId) in TreeNode { id = model.nextId, header = name, isExpanded = False, children = [], isHeaderEditEnabled = False } (Rule { type_ = Limit, name = name, condition = "<condition>", projection = "<projection>", scope = AllApplicants })
    noCmd m = (m, Cmd.none)
  in
    case msg of
      AddRule ->
        { model | nextId = model.nextId + 1, process = model.process ++ [ mkRuleNode () ] } |> noCmd
      ToggleTreeNode id -> 
        updateNodeWithId id (\(TreeNode n pl) -> TreeNode { n | isExpanded =  not n.isExpanded } pl) |> noCmd
      UpdateRuleType id newType ->
        updateNodeWithId id (\(TreeNode n (Rule r)) -> TreeNode n (Rule { r | type_ = newType })) |> noCmd
      UpdateRuleScope id newScope ->  
        updateNodeWithId id (\(TreeNode n (Rule r)) -> TreeNode n (Rule { r | scope = newScope })) |> noCmd
      AddSubRule id ->
        updateNodeWithId id (\(TreeNode n pl) -> TreeNode { n | children = n.children ++ [mkRuleNode ()] } pl) |> increaseId |> noCmd
      ToggleEditHeader id ->
        updateNodeWithId id (\(TreeNode n pl) -> TreeNode { n | isHeaderEditEnabled = not n.isHeaderEditEnabled } pl) |> noCmd
      NewHeaderValue id value ->
        updateNodeWithId id (\(TreeNode n (Rule r)) -> TreeNode { n | header = value } (Rule { r | name = value })) |> noCmd
      ToggleProcessView -> 
        { model | processView = if model.processView == UI then Raw else UI } |> noCmd
      RuleConditionUpdated id newCondition ->
        updateNodeWithId id (\(TreeNode n (Rule r)) -> TreeNode n (Rule { r | condition = newCondition })) |> noCmd
      RuleProjectionUpdated id newProjection ->
        updateNodeWithId id (\(TreeNode n (Rule r)) -> TreeNode n (Rule { r | projection = newProjection })) |> noCmd
      MakeHttpRequest -> (model, mkHttpRequest model)

mkHttpRequest : AppModel -> Cmd AppMsg
mkHttpRequest _ = Cmd.none

view : AppModel -> Html AppMsg
view model =
        Grid.container []
          [ CDN.stylesheet
            , Grid.simpleRow [ Grid.col [] [ viewProcessHeader model  ] ]
            , Grid.simpleRow [ Grid.col [] [ viewProcessDetails model ] ]
          ]

viewProcessHeader : AppModel -> Html AppMsg
viewProcessHeader model = 
  Grid.container [ style "margin-top" "20px", style "margin-bottom" "20px" ] 
    [ Grid.row [ ] 
        [ Grid.col [ Col.xsAuto ] [ Html.h2 [] [ text "Process" ] ]
        , Grid.col [ ] []
        , Grid.col [ Col.xsAuto ] [ button [ type_ "button", class "btn btn-primary", onClick AddRule ] [ text "Add Rule" ] ]
        , Grid.col [ Col.xsAuto ] [ button [ type_ "button", class "btn btn-primary", onClick ToggleProcessView ] [ text (if model.processView == UI then "View Raw" else "View UI") ] ]
        ]
    ]

viewProcessDetails : AppModel -> Html AppMsg
viewProcessDetails model = if model.processView == UI then viewProcessDetailsUI model.process else viewProcessDetailsRaw model.process 

viewProcessDetailsRaw : List (TreeNode Rule) -> Html AppMsg
viewProcessDetailsRaw process = 
  let 
    --json : String
    json =  toJson process --Serialize.encodeRuleList Nothing process |> Encode.encode 4
  in
    Html.pre [] [ text json ]

-- http://elm-bootstrap.info/form
viewProcessDetailsUI : List (TreeNode Rule) -> Html AppMsg
viewProcessDetailsUI process = 
    let
        viewKeydRule : (TreeNode Rule) -> (String, Html AppMsg)
        viewKeydRule (TreeNode node (Rule rule)) = 
          let
            updateRuleType s = UpdateRuleType node.id (
                case s of
                  "Policy" -> Policy
                  "Limit" -> Limit 
                  _ -> Group
              )
            viewRuleDetails =
              if node.isExpanded then
                [ Form.form [ style "margin-top" "20px" ]
                  [ Form.group []
                      [ Form.label [ Html.Attributes.for "rule-type-selector" ] [ text "Rule type" ]
                      , Select.select [ Select.id "rule-type-selector", Select.onChange updateRuleType]
                          [ Select.item [ selected (rule.type_ == Policy) ] [ text "Policy"]
                          , Select.item [ selected (rule.type_ == Limit) ] [ text "Limit"]
                          , Select.item [ selected (rule.type_ == Group) ] [ text "Group"]
                          ]
                      ]
                  , Form.group [ Form.attrs [ Html.Attributes.hidden (List.member rule.type_ [ Policy, Limit ] |> not) ] ]
                      [ Form.label [Html.Attributes.for "rule-condition" ] [ text "Condition"]
                      , Input.text [ Input.id "rule-condition", Input.onInput (RuleConditionUpdated node.id), Input.value rule.condition ]
                      , Form.help [] [ text "Example: Vars.Credit < 1000 && Vars.Age >= 20" ]
                      ]
                  , Form.group [ Form.attrs [ Html.Attributes.hidden (List.member rule.type_ [ Limit ] |> not) ] ]
                      [ Form.label [Html.Attributes.for "rule-projection" ] [ text "Projection"]
                      , Input.text [ Input.id "rule-projection", Input.onInput (RuleProjectionUpdated node.id), Input.value rule.projection ]
                      , Form.help [] [ text "Example: Vars.Amount - Vars.Credit" ]
                      ]
                  , Checkbox.checkbox 
                      [ Checkbox.id "rule-scope-all-applicants"
                      , Checkbox.onCheck (\b -> UpdateRuleScope node.id (if b then AllApplicants else AnyApplicant))
                      , Checkbox.attrs [ Html.Attributes.disabled (List.member rule.type_ [ Policy ] |> not) ]
                      , Checkbox.checked (rule.scope == AllApplicants)
                      ] "Applies to all applicants"
                  ]
                , Grid.container [ style "margin-top" "20px", style "margin-bottom" "20px" ]
                  [ Grid.simpleRow 
                      [ Grid.col [] 
                          [ button 
                              [ class "btn"
                              , class "btn-primary"
                              , Html.Attributes.hidden (List.member rule.type_ [ Group ] |> not)
                              , onClick (AddSubRule node.id)
                              ] [ text "Add Sub Rule"]                
                          ]
                      ]
                  ]
                ]
              else [ ]         
            ruleHeader = 
              if node.isHeaderEditEnabled then
                [ Input.text 
                    [ Input.id "node-header"
                    , Input.value node.header
                    , Input.attrs [ style "margin-bottom" "10px", style "margin-top" "10px", style "font-size" "24px", style "font-weight" "500" ]
                    , Input.attrs [ onEnter (ToggleEditHeader node.id) ] 
                    , Input.onInput (NewHeaderValue node.id)
                    ] 
                ] 
              else
                [ Html.label [ class "btn", class "btn-lnk", onClick (ToggleTreeNode node.id), style "margin-top" "10px" ] [ Html.h4 [] [ text rule.name ] ] ]
          in
              ( String.fromInt node.id
              , Html.li [ class "list-group-item" ] 
                [ Grid.container [] 
                    (
                      [ Grid.row [ Row.attrs [ onDoubleClick (ToggleEditHeader node.id), style "background" "lightgrey" ] ] [ Grid.col [ Col.xsAuto ] ruleHeader ]
                      , Grid.simpleRow [ Grid.col [] viewRuleDetails ] 
                      , Grid.row 
                          [ Row.attrs 
                              [ Html.Attributes.hidden (not (List.member rule.type_ [ Group ]) || not node.isExpanded) ]
                          ] 
                          [ Grid.col [] [ viewProcessDetailsUI node.children ] ]
                      ]
                    )
                ]
              )
    in
        Keyed.ul [ class "list-group" ] <|
                List.map viewKeydRule process
