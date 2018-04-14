// Learn more about F# at http://fsharp.org

open DhlParser
open FSharp.Control.Reactive
open System
open TelegramBotApi
open Database
open MessageFilter
open GetMessages

let packageToNewRecord chat_id (package: Package) =
    {
        chatId = chat_id;
        currentState = package.currentState;
        totalState = package.totalState;
        trackingNumber = package.trackingNumber
    }
let recordToPackage statusText dbRecord =
    {
        currentState = dbRecord.currentState;
        totalState = dbRecord.totalState;
        trackingNumber = dbRecord.trackingNumber;
        statusText = statusText
    }
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

let create (chat_id: int64) (package: Package) =
    packageToNewRecord chat_id package
        |> addNewTrackingNumber
        |> recordToPackage package.statusText

let handleTrack command message =
    getStatusAync command.argument
        |> Async.RunSynchronously
        |> Option.map ( create message.chat_id  )
        |> Option.map (statusMessage message.chat_id)
        |> Option.defaultValue (sprintf "Could not find number %s" command.argument |> simpleTextMessage message.chat_id)
        |> Some

let handleCheck (command: BotCommand) message =
    getStatusAync command.argument
        |> Async.RunSynchronously
        |> Option.map (statusMessage message.chat_id)
        |> Option.defaultValue (sprintf "Could not find number %s" command.argument |> simpleTextMessage message.chat_id)
        |> Some
    

let handler = switch [ botCommand "/track" handleTrack
                       botCommand "/check" handleCheck ]

let filterErrors (result: Result<'a, 'b>) =
    match result with
    | Ok x -> Observable.result x
    | Error err -> printf "%A" err
                   Observable.neverWitness Unchecked.defaultof<'a>
let filterSome (value: 'a option) =
    match value with
    | Some x -> Observable.result x
    | None -> Observable.neverWitness Unchecked.defaultof<'a>

[<EntryPoint>]
let main _ =
    let update =Observable.timerPeriod DateTimeOffset.Now (TimeSpan.FromMinutes 30.0)
                |> Observable.map (fun _ -> getUnfinishedTrackingNumbers())
                |> Observable.map (Seq.map processTrackingNumbers)
                |> Observable.flatmap (Observable.zipSeq)
                |> Observable.map (Seq.choose id)
                |> Observable.map (Seq.map updateState)
                |> Observable.subscribe (fun x -> 
                    x |> Seq.toList  |> ignore
                    cleanDb()
                )
    let updateListener = getUpdateObserver {DefaultConfig with apiKey = API_KEY}
                        |> Observable.flatmap filterErrors
                        |> Observable.map handler
                        |> Observable.flatmap filterSome
                        |> Observable.subscribe( fun x -> sendMessage API_KEY x |> ignore)

    AppDomain.CurrentDomain.ProcessExit
        |> Async.AwaitEvent
        |> Async.RunSynchronously
        |> (fun _ -> 
                update.Dispose()
                updateListener.Dispose()
            )
        |> ignore
    0 // return an integer exit code
    