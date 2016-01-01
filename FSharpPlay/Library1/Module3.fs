#if INTERACTIVE
#else
module Module3
#endif

let arr = [|1; 2; 3|]

let fruits = 
    [|
        "apple"
        "orange"
        "pear"
    |]

let numbers = [|0..4..12|]

let squares = [| for i in 1..99 do yield i*i|]
let IsEven n = 
    n % 2 = 0
let evenSquares = Array.filter (fun i -> IsEven i) squares

let RandomFruits count = 
    let r = System.Random()
    let fruits = [|"apple"; "orange"; "pear"|]
    [|
        for i in 1..count do
            let index = r.Next(3)
            yield fruits.[index]
    |]

let RandomFruits2 count = 
    let r = System.Random()
    let fruits = [|"apple"; "orange"; "pear"|]
    Array.init count (fun i ->
        let index = r.Next(3)
        fruits.[index]
    )

open System.IO
let files = 
    Directory.EnumerateFiles(@"c:\windows")
    |> Seq.map (fun path -> FileInfo path)
    |> Seq.filter(fun f -> f.Length > 1000000L)
    |> Seq.map (fun f -> f.Name)