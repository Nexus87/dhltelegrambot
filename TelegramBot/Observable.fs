
module Observable

open System.Reactive.Linq

    let withLatestFrom other map stream=  
        Observable.WithLatestFrom(stream, other, map)