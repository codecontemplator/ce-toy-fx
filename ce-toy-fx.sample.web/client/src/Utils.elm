module Utils exposing (..)

import Html
import Html.Events
import Json.Decode as Decode

onEnter : msg -> Html.Attribute msg
onEnter msg =
    let
        isEnter code =
            if code == 13 then
                Decode.succeed msg
            else
                Decode.fail "not ENTER"
    in
        Html.Events.on "keydown" (Decode.andThen isEnter Html.Events.keyCode)