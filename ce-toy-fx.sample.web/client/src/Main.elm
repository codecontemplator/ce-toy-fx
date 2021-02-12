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

-- https://github.com/evancz/elm-todomvc/blob/master/src/Main.elm

main =
  Browser.sandbox { init = { process = [], nextId = 0 }, update = update, view = view }

type alias AppModel = { process : List (TreeNode Rule), nextId : Int }

type TreeNode a = TreeNode { header : String, isExpanded : Bool, id : Int } a

type Rule = MRuleDef { name : String, condition : String, projection : String }
          | MRuleGroup { name : String }

type AppMsg = AddMRuleDef | ExpandRule Int

update : AppMsg -> AppModel -> AppModel
update msg model =
  case msg of
    AddMRuleDef ->
      let
        name = "new rule"
        mrule = MRuleDef { name = name, condition = "<condition>", projection = "<projection>" }
        node = TreeNode { id = model.nextId, header = name, isExpanded = False } mrule
      in
        { model | nextId = model.nextId + 1, process = model.process ++ [ node ] }
    ExpandRule id -> 
      let
        updateNode (TreeNode n pl) = if id == n.id then TreeNode { n | isExpanded = not n.isExpanded } pl else (TreeNode n pl)
      in
        { model | process = List.map updateNode model.process }

view : AppModel -> Html AppMsg
view model =
        Grid.container []
          [ CDN.stylesheet
            , Grid.simpleRow [ Grid.col [] [ button [ type_ "button", class "btn btn-primary", onClick AddMRuleDef ] [ text "Add Rule" ] ] ]
            , Grid.simpleRow [ Grid.col [] [ viewRuleList model.process ] ]
          ]

-- http://elm-bootstrap.info/form
viewRuleList : List (TreeNode Rule) -> Html AppMsg
viewRuleList ruleList = 
    let
        viewKeydRule : (TreeNode Rule) -> (String, Html AppMsg)
        viewKeydRule rule = 
          let
            viewRuleDetails visible rd =
              if visible then
                [ Form.form [] 
                  [ Form.group []
                      [ Form.label [ Html.Attributes.for "rule-type-selector" ] [ text "Rule type" ]
                      , Select.select [ Select.id "rule-type-selector" ]
                          [ Select.item [] [ text "Policy"]
                          , Select.item [] [ text "Limit"]
                          , Select.item [] [ text "Group"]
                          , Select.item [] [ text "Vote"]
                          ]
                      ]
                  , Form.group []
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
            case rule of
                TreeNode node (MRuleDef rd) -> 
                  ( String.fromInt node.id
                  , Html.li [ class "list-group-item" ] 
                    [ Grid.container [] 
                        (
                          [ Grid.simpleRow 
                            [ Grid.col [ Col.xsAuto ] [ button [ class "btn", class "btn-lnk", onClick (ExpandRule node.id) ] [ Html.h4 [] [ text rd.name ] ] ] 
  --                          , Grid.col [ ] [ ]
  --                          , Grid.col [ Col.xsAuto ] [ button [ onClick (ExpandRule node.id)] [ text (if node.isExpanded then "Collapse" else "Expand") ] ] 
                            ]
                          ] 
                          ++ 
                          [ Grid.simpleRow 
                            [ Grid.col [] (viewRuleDetails node.isExpanded rd) ] 
                            ]
                        )
                    ]
                  )
                _ -> ("", text "")
    in
        Keyed.ul [ class "list-group" ] <|
                List.map viewKeydRule ruleList
