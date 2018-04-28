module DA.Doer.Users.Errors

open FSharp.Data
open DA.DataAccess.Domain.Errors

let uniqueKeyUnexpected _ = 
    409, 
    (
        [|
            "userAlreadyExists", JsonValue.Boolean(true)
            "unexpected", JsonValue.Boolean(true)
        |]
        |> JsonValue.Record
    ).ToString()
    

            
let uniqueKey (err: UniqueKeyError) = 
    409, 
    (
        [|
            (
                (if err.collection.Contains("OwnerEmail_") then "userAlreadyExists" else "orgAlreadyExists"), 
                JsonValue.Boolean(true)
            )
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
