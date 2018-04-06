// Learn more about F# at http://fsharp.org

open GetMessages
open ApiRequests
open ApiTypes
open DhlParser
open FSharp.Control.Reactive
open System

let API_KEY = ""
let trackingNumber = ""
let chatId = 0

let rec listen config lastId messageHandler = 
    match getMessages config lastId with
        | Some (updateId, messages) ->
            let nextId = (updateId |> Option.defaultValue -1) + 1
    
            for m in messages do
                match messageHandler m with
                | Some x -> x
                                    |> sendMessage API_KEY
                                    |> printfn "%A"
                | _ -> ()
    
            listen config nextId messageHandler
        | None -> listen config lastId messageHandler
    

let startListen config messageHandler = listen config 0 messageHandler

let echo (x: GetMessages.Message): SendMessage = 
    printfn "%A" x
    {
        chat_id = x.chat_id;
        text = sprintf "%A" x.content ;
        parse_mode = None;
        disable_web_page_preview = None;
        disable_notification = None;
        reply_to_message_id = Some x.message_id
    }

let textMessage x =
    {
        chat_id = int64(chatId);
        text = x;
        parse_mode = None;
        disable_web_page_preview = None;
        disable_notification = None;
        reply_to_message_id = None;
    }

[<EntryPoint>]
let main _ =
    let messageHandler = 
        fun x -> 
            printfn "%A" x
            None

    let config = { DefaultConfig with apiKey = API_KEY }
    Observable.timerPeriod DateTimeOffset.Now (TimeSpan.FromMinutes 30.0)
        |> Observable.map (fun _ -> getStatus trackingNumber |> Option.defaultValue "")
        |> Observable.filter (fun x -> String.IsNullOrWhiteSpace(x) |> not)
        |> Observable.distinctUntilChanged
        |> Observable.subscribe (fun x -> x |> textMessage |> sendMessage API_KEY |> ignore)
        |> ignore
    
    startListen config messageHandler

    0 // return an integer exit code
    