module Kanban.Board.Service.Card

open System
open Kanban.Board.Domain
open Npgsql

type Infra = {
      CreateConnection: unit -> NpgsqlConnection
      GetNow: unit -> DateTime
      GetEventsOfCard: Card.Id -> Async<Card.Events list>
      StoreCardEvents: Card.Id -> LastEventNum -> Card.Events list -> Async<unit>
}
and LastEventNum = int

let private handleCommand infra cardId command =
      async {
            let! history = infra.GetEventsOfCard cardId
            
            let result = Card.handleCommand history command
            
            match result with
            | Ok events -> 
                  do! infra.StoreCardEvents cardId history.Length events
                  return Ok ()
            | Error error ->
                  return Error $"%A{error}"
      }

let addToBoard infra title description =
      async {
            let newCardId = $"{Guid.NewGuid()}"
            
            let! result =
                  Card.AddToBoard {
                        Title = title
                        Description = description 
                        At = infra.GetNow()
                  } |> handleCommand infra newCardId
            return
                  match result with
                  | Ok _ -> Ok newCardId
                  | Error e -> Error e
      }

let moveToInProgressColumn infra cardId =
      Card.MoveToInProgressColumn {
            At = infra.GetNow()
      } |> handleCommand infra cardId 

let moveToDoneColumn infra cardId =
      Card.MoveToDoneColumn {
            At = infra.GetNow()
      } |> handleCommand infra cardId 

let updateContent infra cardId title description =
      Card.UpdateContent {
            Title = title
            Description = description 
      } |> handleCommand infra cardId 
