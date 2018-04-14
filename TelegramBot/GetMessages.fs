module GetMessages

open ApiTypes
open System
open ApiRequests
open System.Reactive.Subjects
open FSharp.Control.Reactive

type MessageType =
    | Message
    | EditedMessage
    | ChannelPost
    | EditedChannelPost


type BotCommand = {
    command: string
    argument: string
}

type MessageEntity =
    | BotCommand of BotCommand
    | Other of ApiTypes.MessageEntity


type TextContent = {
    text: string;
    entities: MessageEntity seq
}
    
type MessageContent =
    | Sticker of Sticker
    | Text of TextContent
    | Location of Location
type MessageBase = {
    message_id: int;
    messageType: MessageType;
    from: User option;
    date: DateTimeOffset;
    content: MessageContent
    chat_id: int64
}

type TextMessage = {
    message_id: int;
    messageType: MessageType;
    from: User option;
    date: DateTimeOffset;
    content: string
    entities: MessageEntity seq
    chat_id: int64
}
type StickerMessage = {
    message_id: int;
    messageType: MessageType;
    from: User option;
    date: DateTimeOffset;
    content: Sticker;
    chat_id: int64
}
type LocationMessage = {
    message_id: int;
    messageType: MessageType;
    from: User option;
    date: DateTimeOffset;
    content: Location;
    chat_id: int64
}

type Message = {
    message_id: int;
    messageType: MessageType;
    from: User option;
    date: DateTimeOffset;
    content: MessageContent
    chat_id: int64
}

let private parseTextMessage (message: ApiTypes.Message) (text: string) =
    let entites = message.entities 
                |> Option.defaultValue Array.empty
                |> Seq.map (fun x ->  match x.``type`` with
                                        | "bot_command" -> BotCommand {
                                                command = text.Substring(x.offset, x.length)
                                                argument = text.Substring(x.offset + x.length)
                                            }
                                        | _ -> Other x
                )
    
    Text { text = text; entities = entites }

type CommandMessageHandler = BotCommand -> Message -> SendMessage option

let parseMessage (message: ApiTypes.Message, messageType: MessageType) =
    match message with
            | {location = Some x} -> Some (Location x)
            | {sticker = Some x} -> Some (Sticker x)
            | {text = Some x} -> Some (parseTextMessage message x)
            | _ -> None
    |> Option.map(fun x -> 
    {
        message_id = message.message_id;
        messageType = messageType;
        from = message.from;
        date = DateTimeOffset.FromUnixTimeSeconds(message.date);
        content = x;
        chat_id = message.chat.id;
    })

let parseUpdate (update: ApiTypes.Update) =
    let setMessageType y = Option.map (fun x -> (x, y))
    [|
        update.message |> setMessageType MessageType.Message;
        update.edited_message |> setMessageType MessageType.EditedMessage;
        update.channel_post |> setMessageType MessageType.ChannelPost;
        update.edited_channel_post |> setMessageType MessageType.EditedChannelPost
    |]
    |> Array.choose id

type RequestConfig = {
    apiKey: string;
    timeout: int
}

let DefaultConfig = {
    apiKey = "";
    timeout = (60 * 60 * 1000)
}

let getMessagesAsync config offset = async {
    let mapMessages response =
        response.result
        |> (fun updates ->
            let update_id = updates
                            |> Array.tryLast 
                            |> Option.map (fun x -> x.update_id)
                            |> Option.defaultValue 0
            let messages = updates
                            |> Array.collect parseUpdate
                            |> Array.map parseMessage
                            |> Array.choose id

            (update_id, messages)
        )
    return! getUpdatesAsync config.apiKey config.timeout offset
    |> Async.map (Result.map mapMessages)
}


    

let getUpdateObserver config =
    let trigger = new Subject<Result<Message[], exn>>()

    let split resultList = match resultList with
                            | Ok list -> list |> Seq.map Ok
                            | Error ex -> seq [ Error ex ]
    let rec loop offset = async {
        let! res = getMessagesAsync config offset
        res |> Result.map (fun (_, res) -> res) |> trigger.OnNext
        return! match res with
                | Ok (offset, _) -> loop (offset + 1)
                | _ -> loop offset
    }
    loop 0 |> Async.StartAsTask |> ignore
    trigger
        |> Observable.flatmapSeq split
