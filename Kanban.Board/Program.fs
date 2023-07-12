open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Npgsql
open Npgsql.FSharp

open Saturn
open Giraffe

open Kanban.Board

let getInfra (ctx: HttpContext) =
     ctx.RequestServices.GetService<Infra.Factory.Infra>()

let api = pipeline {
    plug acceptJson
    set_header "x-pipeline-type" "Api"
}

let router = router {
    not_found_handler (setStatusCode 404 >=> text "404")
    pipe_through api
    
    forward "/api/card" (Api.Card.router (getInfra >> (fun i -> i.CardInfra)))
}

let createConnection () =
    let connectionString =
        Sql.host "localhost"
        |> Sql.database "postgres"
        |> Sql.username "postgres"
        |> Sql.password "password"
        |> Sql.port 5432
        |> Sql.formatConnectionString

    new NpgsqlConnection(connectionString)

let configureServices (services : IServiceCollection) =
    services.AddScoped<Infra.Factory.Infra>(fun _ -> Infra.Factory.createInfra createConnection)
    
let app = application {
    use_router router
    service_config configureServices
}

run app