module DA.Doer.Users.Errors

open FSharp.Data
open DA.DataAccess.Domain.Errors
open DA.Auth.Domain.Errors

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

let connectionFail (err: ConnectionError) = 
    500, 
    (
        [|
            ("connectionFail", JsonValue.String err.message)
        |]
        |> JsonValue.Record
    ).ToString()

let networkFail (err: NetworkError) = 
    500, 
    (
        [|
            ("networkFail", JsonValue.String err.Message)
        |]
        |> JsonValue.Record
    ).ToString()


let requestFail (err: NetworkError) = 
    500, 
    (
        [|
            ("requestFail", 
                [| 
                    ("message", JsonValue.String err.Message) 
                    (
                        "responseUri", 
                        match err.Response with 
                            | Some x -> JsonValue.String (x.Uri.ToString()) 
                            | None _ -> JsonValue.Null
                    )
                |] |> JsonValue.Record
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


let unexepcted (e: exn) = 500, ([| ("message", JsonValue.String(e.Message)) |] |> JsonValue.Record).ToString()
