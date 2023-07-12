module Kanban.Board.Domain.Card

open System

type Id = string

type Events =
      | ContentUpdated of ContentUpdated
      | MovedToTodoColumn of MovedToTodoColumn
      | MovedToInProgressColumn of MovedToInProgressColumn
      | MovedToDoneColumn of MovedToDoneColumn
and ContentUpdated = {
      Title: string
      Description: string
}
and MovedToTodoColumn = {
      At: DateTime
}
and MovedToInProgressColumn = {
      At: DateTime
}
and MovedToDoneColumn = {
      At: DateTime
}

type Commands =
      | AddToBoard of AddToBoard
      | MoveToInProgressColumn of MoveToInProgressColumn
      | MoveToDoneColumn of MoveToDoneColumn
      | UpdateContent of UpdateContent
and AddToBoard = {
      Title: string
      Description: string
      At: DateTime
}
and MoveToInProgressColumn = {
      At: DateTime
}
and MoveToDoneColumn = {
      At: DateTime
}
and UpdateContent = {
      Title: string
      Description: string 
}

type private State = {
      Status: Status option
}
and Status =
      | Todo
      | InProgress
      | Done

let private initial = {
      Status = None
}
      
let private apply state event =
      match event with
      | MovedToTodoColumn _ -> { state with Status = Some Status.Todo }
      | MovedToInProgressColumn _ -> { state with Status = Some Status.InProgress }
      | MovedToDoneColumn _ -> { state with Status = Some Status.Done }
      | ContentUpdated _ -> state

let private decide state command =
      match state, command with
      | AddToBoard cmd, { Status = None } ->
            Ok [
                  ContentUpdated { Title = cmd.Title; Description = cmd.Description }
                  MovedToTodoColumn { At = cmd.At }
            ]
      | AddToBoard _, { Status = Some _ } ->
            Error "already added"
      | _, { Status = None } ->
            Error "does not exist"
      | MoveToInProgressColumn cmd, _ ->
            Ok [
                  MovedToInProgressColumn { At = cmd.At } 
            ]
      | MoveToDoneColumn cmd, _ -> 
            Ok [
                  MovedToDoneColumn { At = cmd.At }
            ]
      | UpdateContent cmd, _ ->
            Ok [
                  ContentUpdated {
                        Title = cmd.Title
                        Description = cmd.Description 
                  }
            ]
            
let handleCommand history command =
      history
      |> List.fold apply initial
      |> decide command 
