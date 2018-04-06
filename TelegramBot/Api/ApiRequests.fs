module ApiRequests

open Newtonsoft.Json
open ApiTypes
open Converter
open FSharp.Data
open System

let getUpdates botKey timeout offset =
    let deserialize = 
        fun x -> JsonConvert.DeserializeObject<Response<Update array>>(x, new OptionConverter())

    let url = sprintf "https://api.telegram.org/bot%s/getupdates" botKey
    try
        Http.RequestString(url, query= 
                [
                    "offset", (offset.ToString());
                    "timeout", (timeout.ToString())
                ], timeout = timeout
            )
            |> deserialize
            |> Some
    with 
        | ex when ex.InnerException.GetType() = typedefof<TimeoutException> -> None

let sendMessage botKey (message: SendMessage) =
    let settings = new JsonSerializerSettings();
    settings.Converters.Add(new OptionConverter());
    settings.NullValueHandling <- NullValueHandling.Ignore;

    let serialize = fun x -> JsonConvert.SerializeObject(x, settings)
    let deserialize = 
        fun x -> JsonConvert.DeserializeObject<Response<Message>>(x, new OptionConverter())

    let url = sprintf "https://api.telegram.org/bot%s/sendMessage" botKey
    let body = message |> serialize |> TextRequest
    printfn "%A" body
    Http.RequestString(
        url, 
        headers = [ HttpRequestHeaders.ContentType HttpContentTypes.Json ], 
        body = body
    )
        |> deserialize