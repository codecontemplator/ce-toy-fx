module Bootstrap exposing (..)

import Html exposing (Html, node)
import Html.Attributes exposing (href, rel)

-- Thanks to Elm Boostrap for inspiration. 
-- Copyright (c) 2017, Magnus Rundberget
-- All rights reserved.
-- https://github.com/rundis/elm-bootstrap/blob/5.2.0/LICENSE

stylesheet : Html msg
stylesheet =
    node "link"
        [ rel "stylesheet"
        , href "https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css"
        ]
        []


{-| Font Awesome can also be conveniently included as an inline node. Font Awesome is not a dependency for `elm-bootstrap`.
-}
fontAwesome : Html msg
fontAwesome =
    node "link"
        [ rel "stylesheet"
        , href "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css"
        ]
        []