module Main exposing (..)

import Browser
import Html exposing (Html, button, div, text)
import Html.Attributes exposing (class, type_)
import Html.Events exposing (onClick)
import Html.Keyed as Keyed
import Bootstrap.Grid as Grid
import Bootstrap.Grid.Col as Col
import Bootstrap.CDN as CDN

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
        mrule = MRuleDef { name = name, condition = "", projection = "" }
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

viewRuleList : List (TreeNode Rule) -> Html AppMsg
viewRuleList ruleList = 
    let
        viewKeydRule : (TreeNode Rule) -> (String, Html AppMsg)
        viewKeydRule rule = 
            case rule of
                TreeNode node (MRuleDef rd) -> 
                  ( String.fromInt node.id
                  , Html.li [ class "list-group-item" ] 
                    [ Grid.container [] 
                        [ Grid.simpleRow 
                          [ Grid.col [ Col.xsAuto ] [ Html.h4 [] [ text rd.name ] ] 
                          , Grid.col [ ] [ ]
                          , Grid.col [ Col.xsAuto ] [ button [ onClick (ExpandRule node.id)] [ text (if node.isExpanded then "Collapse" else "Expand") ] ] 
                          ]
                        ]
                    ]
                  )
                _ -> ("", text "")
    in
        Keyed.ul [ class "list-group" ] <|
                List.map viewKeydRule ruleList
