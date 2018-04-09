module DhlParser

open FSharp.Data
open FSharp.Control.Reactive

let private deliveryProgressClass = "div.mm_deliveryProgress"
let private stepDoneClass = ".mm_deliveryStep-done"
let private stepHtmlElement = "div"
let private deliveryStatusClass = ".mm_shipmentStatus"
let private statusText = "Status"

type Package = {
    currentState: int;
    totalState: int;
    statusText: string;
    trackingNumber: string;
}

let findStatusText (x: HtmlNode) =
    x.Descendants ["dd"]
        |> Seq.tryFind (fun y -> y.InnerText().StartsWith(statusText))
        |> Option.map (fun y -> y.InnerText())
        |> Option.map (fun y -> (y.IndexOf(":") + 1 )|> y.Substring)


let getStatusAync number = async {
    let url = sprintf "https://nolp.dhl.de/nextt-online-public/set_identcodes.do?lang=en&idc=%s&extendedSearch=true" number
    let! result = HtmlDocument.AsyncLoad(url)
    let status = result.CssSelect deliveryStatusClass
                |> List.tryHead
                |> Option.map findStatusText
                |> Option.flatten

    let steps = result.CssSelect deliveryProgressClass
                |> Seq.tryItem 0
                |> Option.map (fun x -> (
                                        x.CssSelect stepDoneClass |> Seq.length,  
                                        x.Descendants [stepHtmlElement] |> Seq.length)
                    )
    
    return Option.map2 (fun stat (a, b) -> {
                                            currentState = a;
                                            totalState = b;
                                            trackingNumber = number;
                                            statusText = stat

            }) status steps
}
let getStatus number =
    getStatusAync number
    |> Observable.ofAsync