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