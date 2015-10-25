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

let Square x = x * x
