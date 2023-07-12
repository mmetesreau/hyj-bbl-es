module Kanban.Board.Infra.EventStore

open Npgsql
open Npgsql.FSharp
open System.Text.Json
open System.Text.Json.Serialization

let private options = JsonFSharpOptions.Default().ToJsonSerializerOptions()

let getEvents<'event> (createConnection: unit -> NpgsqlConnection) aggregateId aggregateName =
      async {
            use connection = createConnection ()
            connection.Open()
            
            return
                  connection
                  |> Sql.existingConnection
                  |> Sql.query "SELECT data FROM events WHERE aggregate_id = @aggregate_id AND aggregate_name = @aggregate_name ORDER BY event_num ASC"
                  |> Sql.parameters [
                        "aggregate_id", Sql.text aggregateId
                        "aggregate_name", Sql.text aggregateName
                  ]
                  |> Sql.execute (fun read -> JsonSerializer.Deserialize<'event>(read.string "data", options))
      }
      
let saveEvents (createConnection: unit -> NpgsqlConnection) aggregateId aggregateName lastEventNum events =
      async {
            use connection = createConnection ()
            connection.Open()
            
            events
            |> List.iteri (fun eventNum event -> 
                  connection
                        |> Sql.existingConnection
                        |> Sql.query "INSERT INTO events (aggregate_name, aggregate_id, event_num, data) VALUES (@aggregate_name, @aggregate_id, @event_num, @data)"
                        |> Sql.parameters [
                              "aggregate_id", Sql.text aggregateId
                              "aggregate_name", Sql.text aggregateName
                              "event_num", Sql.int (lastEventNum + eventNum + 1)
                              "data", Sql.text (JsonSerializer.Serialize(event, options))
                        ]
                        |> Sql.executeNonQuery
                        |> ignore
                  )
      }