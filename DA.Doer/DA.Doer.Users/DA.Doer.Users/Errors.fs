[<AutoOpen>]
module internal DA.Doer.Users.Exceptions
    
    open DA.Doer.Users.Errors
    open DA.Doer.Domain.Errors
    open DA.Doer.Mongo.Errors
    open FSharp.Data
    open DA.FacePlusPlus

    let inline mapOpt f x = Option.map f x
    
    let matchFaceNotFoundException (e: exn) = 
        match e with
        | :? FaceNotFoundException as ex -> Some ()
        | _ -> None    

    let matchMultipleFacesFoundException (e: exn) = 
        match e with
        | :? MultipleFacesFoundException as ex -> Some ()
        | _ -> None   

    let mapFaceNotFoundException () = 
        401, 
        (
            [|
                ("code", JsonValue.String "FACE_NOT_FOUND") 
                ("message", JsonValue.String "FACE_NOT_FOUND") 
            |]
            |> JsonValue.Record
        ).ToString()

    let mapMultipleFacesFoundException () = 
        401, 
        (
            [|
                ("code", JsonValue.String "MULTIPLE_FACES_FOUND") 
                ("message", JsonValue.String "MULTIPLE_FACES_FOUND") 
            |]
            |> JsonValue.Record
        ).ToString()


    let getHttpError (ex: exn) =  List.choose(fun x -> x ex) >> List.head

    let getHttpErrorWithDefaults (ex: exn) l =  
        l@[
            matchConnectionError >> (mapOpt connectionFail)
            unexepcted >> Some
        ]
        |> getHttpError ex
