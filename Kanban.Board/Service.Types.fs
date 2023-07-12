module Kanban.Board.Service.Types

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