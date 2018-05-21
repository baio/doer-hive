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
        IsPrincipalAncestor = fun principalId userId -> returnM true

        IsPhotoSetExists = fun orgId -> orgHasPhotoLink orgId context.Mongo
        
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

    let inline private map f x = Option.map f x

    (*
    let matchUserMustHaveAtLeast5PhotosException (e: exn) = 
        match e with
        | :? UserMustHaveAtLeast5PhotosException as ex -> Some ex.currentLength
        | _ -> None

    let userMustHaveAtLeast5PhotosException (currentLength: int) = 
        let message = sprintf "Unexpected number of photos to train in storage %i. Must be at least 5" currentLength
        new System.Exception(message) |> unexepcted       
    *)
    
    
    let getHttpError (ex: exn) =  
        [
            matchConnectionError >> (map connectionFail)
            // face not found
            // multiple faces found
            // matchUserMustHaveAtLeast5PhotosException >> (map userMustHaveAtLeast5PhotosException)
            unexepcted >> Some
        ] 
        |> List.choose(fun x -> x ex)
        |> List.head
