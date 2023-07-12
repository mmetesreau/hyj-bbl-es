module Kanban.Board.Api.Card

open Microsoft.AspNetCore.Http
open Saturn
open Giraffe

open Kanban.Board

let private toJson<'result> (result: Result<'result, _>) =
      match result with
      | Ok x -> json x
      | Error error -> RequestErrors.badRequest (text error)

module Commands = 
      type AddToBoardDto = {
            title: string
            description: string
      }

      let addToBoard getInfra next (ctx: HttpContext) = task {
            let! cmd = ctx.BindJsonAsync<AddToBoardDto>() 
            
            let! result = Service.Card.addToBoard (getInfra ctx) cmd.title cmd.description
            
            return! toJson result next ctx 
      }

      let moveToInProgressColumn cardId getInfra next (ctx: HttpContext) = task {
            let! result = Service.Card.moveToInProgressColumn (getInfra ctx) cardId
            
            return! toJson result next ctx 
      }

      let moveToDoneColumn cardId getInfra next (ctx: HttpContext) = task {
            let! result = Service.Card.moveToDoneColumn (getInfra ctx) cardId
            
            return! toJson result next ctx 
      }
            
      type UpdateContentDto = {
            title: string
            description: string
      }
      
      let updateContent cardId getInfra next (ctx: HttpContext) = task {
            let! cmd = ctx.BindJsonAsync<UpdateContentDto>()
            
            let! result = Service.Card.updateContent (getInfra ctx) cardId cmd.title cmd.description
            
            return! toJson result next ctx 
      }

module Queries =
      open Npgsql.FSharp
      
      let getBoard getInfra  next (ctx: HttpContext) =
            task {
                  let infra: Service.Card.Infra = getInfra ctx
                  
                  use connection = infra.CreateConnection ()
                  connection.Open()
            
                  let! result =
                        connection
                        |> Sql.existingConnection
                        |> Sql.query "SELECT * FROM readmodel_boards"
                        |> Sql.executeAsync(fun read -> {|
                              card_id = read.string "card_id"
                              title = read.string "title"
                              description = read.string "description"
                              status = read.string "status"
                        |})
                  
                  return! toJson (Ok result) next ctx
            }

let router getInfra =
      router {
            post "/add-to-board" (Commands.addToBoard getInfra)
            putf "/move-to-inprogress-column/%s" (fun cardId -> Commands.moveToInProgressColumn cardId getInfra)
            putf "/move-to-done-column/%s" (fun cardId -> Commands.moveToDoneColumn cardId getInfra)
            putf "/update-content/%s" (fun cardId -> Commands.updateContent cardId getInfra)
            
            get "/board" (Queries.getBoard getInfra)
      }

