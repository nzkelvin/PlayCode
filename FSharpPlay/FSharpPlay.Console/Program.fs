// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

//todo:
//how to import modules

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    System.Console.ReadKey() |> ignore // ignore to avoid a compile error but looks like no longer needed.
    0 // return an integer exit code
