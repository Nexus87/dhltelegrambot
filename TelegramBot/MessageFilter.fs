module MessageFilter

open GetMessages

type MessageFilter = Message -> (Message -> ApiTypes.SendMessage option) option

let private getBotCommand commandName command =
    match command with
    | BotCommand x when x.command = commandName -> Some x
    | _ -> None

let botCommand commandName (handler: CommandMessageHandler) (message: Message) =
    let content = message.content
    match content with
    | Text x -> x.entities 
                |> Seq.choose (getBotCommand commandName) 
                |> Seq.tryHead 
                |> Option.map (fun x -> handler x)
    | _ -> None

let switch (handlers: MessageFilter seq) (message: Message) = 
    let handler = handlers 
                |> Seq.choose (fun x -> x message) 
                |> Seq.tryHead
                |> Option.defaultValue (fun _ -> None)
    handler message