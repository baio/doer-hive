module DA.Doer.Users.Errors

open FSharp.Data
open DA.DataAccess.Domain.Errors
open DA.Auth.Domain.Errors

let uniqueKeyUnexpected _ = 
    409, 
    (
        [|
            "message", JsonValue.String "Unique key contsraint violation"
            "userAlreadyExists", JsonValue.Boolean(true)
            "unexpected", JsonValue.Boolean(true)
        |]
        |> JsonValue.Record
    ).ToString()
    
                
let uniqueKey (err: UniqueKeyError) = 
    409, 
    (
        [|
            ("message", JsonValue.String "Unique key contsraint violation") 
            (
                (if err.Collection.Contains("OwnerEmail_") then "userAlreadyExists" else "orgAlreadyExists"), 
                JsonValue.Boolean(true)
            )
        |]
        |> JsonValue.Record
    ).ToString()

let connectionFail (err: ConnectionError) = 
    500, 
    (
        [|
            ("message", JsonValue.String err.Message) 
            ("connectionFail", JsonValue.String err.Message)
        |]
        |> JsonValue.Record
    ).ToString()

let networkFail (err: NetworkError) = 
    500, 
    (
        [|
            ("message", JsonValue.String err.Message) 
            ("networkFail", JsonValue.String err.Message)
        |]
        |> JsonValue.Record
    ).ToString()


let requestFail (err: NetworkError) = 
    ((int)err.Code) , 
    (
        [|
            ("message", JsonValue.String err.Message) 
            ("requestFail", 
                [| 
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
            ("message", JsonValue.String "Validation failed") 
            (
                "validationErrors", 
                e |> List.map(fun (f, s) -> f, JsonValue.String(s)) |> List.toArray |> JsonValue.Record
            )
        |]
        |> JsonValue.Record    
    ).ToString()


let unexepcted (e: exn) = 500, ([| ("message", JsonValue.String(e.Message)) |] |> JsonValue.Record).ToString()
