module Kanban.Board.Infra.Factory

open System
open Npgsql
open Kanban.Board

let private cardAggregateName = "Card"

let private formatCardAggregateId (id: Domain.Card.Id) =
     $"{cardAggregateName}-{id}"

type Infra = {
     CardInfra: Service.Card.Infra      
}

let createInfra (createConnection: unit -> NpgsqlConnection) = {
     CardInfra = {
          CreateConnection = createConnection
          GetNow = fun () -> DateTime.Now
          StoreCardEvents = fun cardId lastEventNum events -> async {
               do! EventStore.saveEvents createConnection (formatCardAggregateId cardId) cardAggregateName lastEventNum events
               
               do! events
                    |> List.map (fun e -> PubSub.Card (cardId, e))
                    |> PubSub.handleEvents createConnection 
          } 
          GetEventsOfCard = fun cardId ->
               EventStore.getEvents<Domain.Card.Events> createConnection (formatCardAggregateId cardId) cardAggregateName
     }     
}

