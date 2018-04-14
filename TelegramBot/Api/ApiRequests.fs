module ApiRequests

module Async =
    let map f x = async {
        let! result = x
        return f result
    }
open Newtonsoft.Json
open ApiTypes
open Converter
open FSharp.Data
open System

let rec getUpdatesAsync botKey timeout offset = async{
    let deserialize = 
        fun x -> JsonConvert.DeserializeObject<Response<Update array>>(x, new OptionConverter())

    let url = sprintf "https://api.telegram.org/bot%s/getupdates" botKey
    let! result = Http.AsyncRequestString(url, query= 
                                                    [
                                                        "offset", (offset.ToString());
                                                        "timeout", (timeout.ToString())
                                                    ], timeout = timeout
                    )
                    |> Async.map deserialize
                    |> Async.Catch

    match result with
    | Choice1Of2 x -> return Result.Ok x
    | Choice2Of2 ex when ex.InnerException.GetType() = typedefof<TimeoutException> -> return! (getUpdatesAsync botKey timeout offset)
    | Choice2Of2 ex -> return Result.Error ex
}

let getUpdates botkey timeout offset =
    getUpdatesAsync botkey timeout offset
    |> Async.RunSynchronously

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