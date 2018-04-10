// Learn more about F# at http://fsharp.org

open DhlParser
open FSharp.Control.Reactive
open System
open TelegramBotApi
open Database

let API_KEY = System.Environment.GetEnvironmentVariable("BOTKEY")

let statusMessage chatId (x: Package) =
    sprintf "%s: (%d/%d): %s" x.trackingNumber x.currentState x.totalState x.statusText
    |> simpleTextMessage chatId


let ignoreUnchanged (previousStatus: DbRecord) (package: Package option) =
    package 
        |> Option.bind (fun x -> if x.currentState <> previousStatus.currentState then Some x else None)

let sendTrackingUpdate api_key chatId package =
    package 
        |> Option.map(fun x -> x |> statusMessage chatId |> sendMessage api_key)
        |> ignore
    package

let updateDbRecord (previousRecord: DbRecord) (package: Package option)=
    package
        |> Option.map(fun x -> {previousRecord with currentState = x.currentState; totalState = x.totalState})

let processTrackingNumbers (previousStatus: DbRecord) = 
    getStatus previousStatus.trackingNumber
    |> Observable.map (ignoreUnchanged previousStatus)
    |> Observable.map (sendTrackingUpdate API_KEY previousStatus.chatId)
    |> Observable.map (updateDbRecord previousStatus)


[<EntryPoint>]
let main _ =
    let update =Observable.timerPeriod DateTimeOffset.Now (TimeSpan.FromMinutes 30.0)
                |> Observable.map (fun _ -> getTrackingNumbers())
                |> Observable.map (Seq.map processTrackingNumbers)
                |> Observable.flatmap (Observable.zipSeq)
                |> Observable.map (Seq.choose id)
                |> Observable.subscribe ignore
            
    AppDomain.CurrentDomain.ProcessExit
        |> Async.AwaitEvent
        |> Async.RunSynchronously
        |> (fun _ -> update.Dispose())
        |> ignore
    0 // return an integer exit code
    