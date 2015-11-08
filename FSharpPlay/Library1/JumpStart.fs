#if INTERACTIVE
#else
module JumpStart
#endif

let x = 42
let hi = "hello"

let Greeting name = 
    if System.String.IsNullOrWhiteSpace(name) then
        "whoever you are"
    else
        name

let SayHiTo me =
    printfn "Hi, %s" (Greeting me)

//let Square x = x * x

let PrintNumbers min max =
    let square x =
        x * x
    for x in min..max do
        printfn "%i %i" x (square x)

let RandomPosition() = 
    let r = new System.Random()
    r.NextDouble(), r.NextDouble()

open System.IO
let files = Directory.GetFiles(@"c:\windows", "*.exe")