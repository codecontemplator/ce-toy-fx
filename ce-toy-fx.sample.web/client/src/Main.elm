module Main exposing (..)

import Browser
import Html exposing (Html, button, div, text)
import Html.Attributes exposing (class, type_)
import Html.Events exposing (onClick)
import Bootstrap
import Html.Keyed as Keyed

-- https://github.com/evancz/elm-todomvc/blob/master/src/Main.elm

main =
  Browser.sandbox { init = { process = [] }, update = update, view = view }

type alias AppModel = { process : List MRule }

type MRule = MRuleDef { id : Int, name : String, condition : String, projection : String }
           | MRuleGroup { name : String }

type AppMsg = AddMRule MRule

update : AppMsg -> AppModel -> AppModel
update msg model =
  case msg of
    AddMRule mrule ->
      { model | process = model.process ++ [mrule] }

view : AppModel -> Html AppMsg
view model =
    let 
        addRule = AddMRule (MRuleDef { id = 1, name = "rule", condition = "", projection = ""})
    in
        div []
            [ Bootstrap.stylesheet
            , button [ type_ "button", class "btn btn-primary", onClick addRule ] [ text "Add Rule" ]
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
