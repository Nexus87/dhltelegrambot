module DhlParser

open FSharp.Data

let private deliveryProgressClass = "div.mm_deliveryProgress"
let private stepDoneClass = ".mm_deliveryStep-done"
let private stepHtmlElement = "div"
let private deliveryStatusClass = ".mm_shipmentStatus"
let private statusText = "Status"

let findStatusText (x: HtmlNode) =
    x.Descendants ["dd"]
        |> Seq.tryFind (fun y -> y.InnerText().StartsWith(statusText))
        |> Option.map (fun y -> y.InnerText())
        |> Option.map (fun y -> (y.IndexOf(":") + 1 )|> y.Substring)


let getStatus number =
    let url = sprintf "https://nolp.dhl.de/nextt-online-public/set_identcodes.do?lang=en&idc=%s&extendedSearch=true" number
    let result = HtmlDocument.Load(url)
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
                |> Option.map(fun (d, a) -> sprintf "%d/%d" d a)                
    
    let result = [steps; status] 
                |> List.choose id
                |> String.concat ": "

    match result with
        | "" -> None
        | _ -> Some result
    
    
