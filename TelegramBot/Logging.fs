module Logging

let doLog text (value: 'a) =
    printfn "%s: %A" text value
    value