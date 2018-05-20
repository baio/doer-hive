module DA.Doer.Users.EnlistToVerify

open DA.Doer.Users.API.EnlistToVerify
open DA.Doer.Mongo
open DA.FSX.Task
open Microsoft.WindowsAzure.Storage.Blob
open DA.HTTP
open System
open DA.HTTP.Blob
open DA.FacePlusPlus

type EnlistToVerifyContext = {
    Mongo : DA.Doer.Mongo.MongoApi
    Blob  : DA.HTTP.Blob.BlobApi
    FacePP: FacePlusPlusApi
}

let mapContext = fun (context: EnlistToVerifyContext) ->
    {
        IsPrincipalAncestor = fun principalId userId -> returnM true

        GetUserPhotos = fun userId -> getDirectoryBlobs (Some 3) userId context.Blob

        IsPhotoSetExists = fun orgId -> orgHasPhotoLink orgId context.Mongo
        
        CreatePhotoSet = fun setId -> 
            createFaceSet setId context.FacePP |> ``const`` true
            //returnM true

        AddPhotosToSet = fun setId streams -> 
            detectAndAddSinglePersonFaces setId streams context.FacePP |> map( fun (x, _, _) -> x )
            //returnM ["100"]

        MarkAsUploaded = fun x -> 
            addUserPhotoLinks' x.OrgId x.UserId x.FaceTokenIds context.Mongo

    }

// Expects user photos already loaded to storage
let enlistToVerify principal userId = 
    mapContext >> enlistToVerify principal userId
    

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
            // matchUserMustHaveAtLeast5PhotosException >> (map userMustHaveAtLeast5PhotosException)
            unexepcted >> Some
        ] 
        |> List.choose(fun x -> x ex)
        |> List.head
