module Kanban.Board.Infra.PubSub

open Npgsql
open Kanban.Board.Domain

type Events =
      | Card of Card.Id * Card.Events
      
let handleEvents (createConnection: unit -> NpgsqlConnection) events =
      async {
            use connection = createConnection ()
            
            for event in events do
                  match event with
                  | Card (cardId, event) ->
                        do! ReadModel.Board.handleEvent connection cardId event
      }