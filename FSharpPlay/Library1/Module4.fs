#if INTERACTIVE
#else
module Module4
#endif

type Person = 
    {
        FirstName: string
        LastName: string
    }

let person = { FirstName = "Kit"; LastName = "Shen" }
printfn "%s, %s" person.FirstName person.LastName

type Company = 
    {
        Name: string
        SalesTaxNumber: int option
    }