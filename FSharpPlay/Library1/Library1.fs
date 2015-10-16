#if INTERACTIVE
#else
module JumpStart
#endif

let x = 42
let hi = "hello"

let SayHiTo me =
    printfn "Hi, %s" me

let Square x = x * x
