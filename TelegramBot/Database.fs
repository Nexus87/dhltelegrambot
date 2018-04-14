module Database

open Npgsql.FSharp
open System
open Logging

type NewDbRecord = {
    currentState: int;
    totalState: int;
    trackingNumber: string;
    chatId: int64;
}
type DbRecord = {
    id: Guid;
    currentState: int;
    totalState: int;
    trackingNumber: string;
    chatId: int64;
}

//let conUrl = "postgresql://[user[:password]@][netloc][:port][/dbname][?param1=value1&...]
let toConnectionString = function
    | username::password::host::port::dbname::_ -> sprintf "Host=%s;Username=%s;Password=%s;Database=%s;Port=%s;sslmode=Require;Trust Server Certificate=true" host username password dbname port 
    | _ -> ""

let defaultConnection = System.Environment.GetEnvironmentVariable("DATABASE_URL").Split([|"postgres://"|], StringSplitOptions.RemoveEmptyEntries)
                        |> Seq.exactlyOne
                        |> (fun x -> x.Split([|':'; '@'; '/'|]))
                        |> Seq.toList
                        |> toConnectionString

let mapRecord = function
            | [
                ("id", Uuid id);
                ("currentstate", Int currentState);
                ("totalstate", Int totalState);
                ("chatid", Sql.Long chatId);
                ("trackingnumber", Sql.String trackingnumber)
                ] -> 
                Some { 
                    id = id; 
                    currentState = currentState; 
                    totalState = totalState; 
                    trackingNumber = trackingnumber; 
                    chatId = chatId
                }
            | _ -> None

let getUnfinishedTrackingNumbers () =
    defaultConnection
            |> Sql.connect
            |> Sql.query "SELECT * FROM \"trackingnumbers\" WHERE totalstate = 0 OR currentState <> totalState"
            |> Sql.executeTable
            |> Sql.mapEachRow mapRecord

let addNewTrackingNumber (record: NewDbRecord) =
    defaultConnection
        |> Sql.connect
        |> Sql.query "INSERT INTO trackingnumbers (chatid, trackingnumber, currentstate, totalstate) VALUES (@chatId, @trackingNumber, @currentstate, @totalstate) RETURNING *"
        |> Sql.parameters 
            [ "chatId", Sql.Long record.chatId
              "trackingNumber", Sql.String record.trackingNumber
              "currentstate", Sql.Int record.currentState
              "totalstate", Sql.Int record.totalState
              ]
        |> Sql.executeTable
        |> Sql.mapEachRow mapRecord
        |> Seq.head

let updateState record =
    defaultConnection
        |> Sql.connect
        |> Sql.query  @"UPDATE trackingnumbers 
                        SET currentstate = @currentState, totalstate = @totalState
                        WHERE id = @id"
        |> Sql.parameters
            [ "currentState", Int record.currentState
              "totalState", Int record.totalState
              "id", Uuid record.id
            ]
        |> Sql.executeNonQuery

let cleanDb () =
    defaultConnection
        |> Sql.connect
        |> Sql.query "DELETE FROM trackingnumbers WHERE totalstate <> 0 AND currentState = totalState"
        |> Sql.executeNonQuery
        |> doLog "Deleted Lines"
        |> ignore