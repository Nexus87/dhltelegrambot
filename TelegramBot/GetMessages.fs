module GetMessages

open ApiTypes
open System
open ApiRequests

type MessageType =
    | Message
    | EditedMessage
    | ChannelPost
    | EditedChannelPost


type MessageContent =
    | Sticker of Sticker
    | Text of string
    | Location of Location

type Message = {
    message_id: int;
    messageType: MessageType;
    from: User option;
    date: DateTimeOffset;
    content: MessageContent
    entities: MessageEntity array option;
    chat_id: int64
}


let parseMessage (message: ApiTypes.Message, messageType: MessageType) =
    match message with
            | {location = Some x} -> Some (Location x)
            | {sticker = Some x} -> Some (Sticker x)
            | {text = Some x} -> Some (Text x)
            | _ -> None
    |> Option.map(fun x -> 
    {
        message_id = message.message_id;
        messageType = messageType;
        from = message.from;
        date = DateTimeOffset.FromUnixTimeSeconds(message.date);
        content = x;
        entities = message.entities;
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

let getMessages config offset = 
    getUpdates config.apiKey config.timeout offset 
        |> Option.map(fun x -> x.result)
        |> Option.map(fun updates ->
            let update_id = updates
                            |> Array.tryLast 
                            |> Option.map (fun x -> x.update_id)
            let messages = updates
                            |> Array.collect parseUpdate
                            |> Array.map parseMessage
                            |> Array.choose id

            (update_id, messages)
        )