module Kanban.Board.Tests.Domain.Card

open System
open Swensen.Unquote
open Xunit
open Kanban.Board.Domain

let now = DateTime.Now

module ``Given the card is not in the board`` =
      let history = []
      
      [<Fact>]
      let ``when we add it to the board then the card is added to the board`` () =
            let result =
                  Card.AddToBoard {
                        Title = "Some title"
                        Description = "Some description"
                        At = now
                  }
                  |> Card.handleCommand history
                  
            let expected = [
                  Card.ContentUpdated {
                        Title = "Some title"
                        Description = "Some description"
                  }
                  Card.MovedToTodoColumn {
                        At = now
                  }
            ]
            
            test <@ result = Ok expected @>

module ``Given the card has been added to the board`` =
      let history = [
            Card.ContentUpdated {
                  Title = "Some title"
                  Description = "Some description"
            }
            Card.MovedToTodoColumn {
                  At = now
            }
      ]
      
      [<Fact>]
      let ``when we add it to the board then we get an error`` () =
            let result =
                  Card.AddToBoard {
                        Title = "Some title"
                        Description = "Some description"
                        At = now
                  }
                  |> Card.handleCommand history
            
            test <@ result = Error "already added" @>
