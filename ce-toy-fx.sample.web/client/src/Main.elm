module Main exposing (..)

import Browser
import Html exposing (Html, button, div, text)
import Html.Attributes exposing (class, type_)
import Html.Events exposing (onClick)
import Bootstrap
import Html.Keyed as Keyed

-- https://github.com/evancz/elm-todomvc/blob/master/src/Main.elm

main =
  Browser.sandbox { init = { process = [], nextId = 0 }, update = update, view = view }

type alias AppModel = { process : List MRule, nextId : Int }

type MRule = MRuleDef { id : Int, name : String, condition : String, projection : String }
           | MRuleGroup { name : String }

type AppMsg = AddMRuleDef

update : AppMsg -> AppModel -> AppModel
update msg model =
  case msg of
    AddMRuleDef ->
      let
        mrule = MRuleDef { id = model.nextId, name = "new rule", condition = "", projection = "" }
      in
        { model | nextId = model.nextId + 1, process = model.process ++ [mrule] }

view : AppModel -> Html AppMsg
view model =
        div []
            [ Bootstrap.stylesheet
            , button [ type_ "button", class "btn btn-primary", onClick AddMRuleDef ] [ text "Add Rule" ]
            , div [] [ viewRuleList model.process ]
            ]

viewRuleList : List MRule -> Html AppMsg
viewRuleList ruleList = 
    let
        viewKeydRule : MRule -> (String, Html AppMsg)
        viewKeydRule rule = 
            case rule of
                MRuleDef rd -> (String.fromInt rd.id, Html.li [ class "list-group-item" ] [text (rd.name)])
                _ -> ("", text "")
    in
        Keyed.ul [ class "list-group" ] <|
                List.map viewKeydRule ruleList
