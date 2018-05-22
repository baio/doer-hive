module DA.Doer.Users.EnlistPhoto

open DA.Doer.Users.API.EnlistPhoto
open DA.Doer.Mongo
open DA.FSX.Task
open Microsoft.WindowsAzure.Storage.Blob
open DA.HTTP
open System
open DA.HTTP.Blob
open DA.FacePlusPlus

type Context = {
    Mongo : DA.Doer.Mongo.MongoApi
    Blob  : DA.HTTP.Blob.BlobApi
    FacePP: FacePlusPlusApi
}

let mapContext = fun (context: Context) ->
    {
        IsPrincipalAncestor = fun principalId userId -> 
            Auth.isPrincipalAncestor principalId userId context.Mongo

        IsPhotoSetExists = fun orgId -> 
            orgHasPhotoLink orgId context.Mongo
        
        CreatePhotoSet = fun setId -> 
            createFaceSet setId context.FacePP |> ``const`` true
            //returnM true

        AddPhotoToSet = fun setId stream -> 
            detectAndAddSinglePersonFaces setId [stream] context.FacePP |> map( fun (x, _, _) -> x.[0] )
            //returnM ["100"]

        StorePhoto = fun x stream -> 
            uploadStreamToStorageDirectoty x.UserId stream context.Blob |> ``const`` true

        MarkAsUploaded = fun x -> 
            addUserPhotoLinks' x.OrgId x.UserId [x.FaceTokenId] context.Mongo

    }

// Expects user photos already loaded to storage
let enlistPhoto principal userId stream = 
    mapContext >> enlistPhoto principal userId stream
    
module Errors =
    
    open DA.Doer.Users.Errors
    open DA.Doer.Domain.Errors
    open DA.Doer.Mongo.Errors
    open FSharp.Data

    let inline private map f x = Option.map f x
    
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

    
    let getHttpError (ex: exn) =  
        [
            matchConnectionError >> (map connectionFail)
            matchFaceNotFoundException >> (map mapFaceNotFoundException)
            matchMultipleFacesFoundException >> (map mapMultipleFacesFoundException)
            unexepcted >> Some
        ] 
        |> List.choose(fun x -> x ex)
        |> List.head
