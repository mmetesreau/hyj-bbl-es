module Kanban.Board.Infra.ReadModel.Board

open Kanban.Board.Domain
open Npgsql.FSharp

let private execute connection query parameters =
      connection
      |> Sql.existingConnection
      |> Sql.query query
      |> Sql.parameters parameters
      |> Sql.executeNonQuery
      |> ignore
      
let handleEvent connection cardId event =
      async {
            match event with
            | Card.MovedToTodoColumn _ ->
                  execute connection "UPDATE readmodel_boards SET status = @status WHERE card_id = @card_id" [
                        "card_id", Sql.text cardId
                        "status", Sql.text "todo"
                  ]
            | Card.MovedToInProgressColumn _ ->
                  execute connection "UPDATE readmodel_boards SET status = @status WHERE card_id = @card_id" [
                        "card_id", Sql.text cardId
                        "status", Sql.text "in progress"
                  ]
            | Card.MovedToDoneColumn _ -> 
                  execute connection "UPDATE readmodel_boards SET status = @status WHERE card_id = @card_id" [
                        "card_id", Sql.text cardId
                        "status", Sql.text "done"
                  ]
            | Card.ContentUpdated evt -> 
                  execute connection
                        "INSERT INTO readmodel_boards (card_id, title, description) 
                              VALUES (@card_id, @title, @description)
                              ON CONFLICT (card_id) DO
                                    UPDATE SET title = @title, description = @description" [
                        "card_id", Sql.text cardId
                        "title", Sql.text evt.Title
                        "description", Sql.text evt.Description
                  ]
      }
