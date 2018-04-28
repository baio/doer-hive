module DA.Doer.Users.Errors

open FSharp.Data

let uniqueKeyUnexpected _ = 
    409, 
    (
        [|
            "userAlreadyExists", JsonValue.Boolean(true)
            "unexpected", JsonValue.Boolean(true)
        |]
        |> JsonValue.Record
    ).ToString()
    
            
let uniqueKey _ = 
    409, 
    (
        [|
            "userAlreadyExists", JsonValue.Boolean(true)
        |]
        |> JsonValue.Record
    ).ToString()

let validation e = 
    400, 
    (
        [|
            (
                "validationErrors", 
                e |> List.map(fun (f, s) -> f, JsonValue.String(s)) |> List.toArray |> JsonValue.Record
            )
        |]
        |> JsonValue.Record    
    ).ToString()


let unexepcted (_: exn) = 500, ""
