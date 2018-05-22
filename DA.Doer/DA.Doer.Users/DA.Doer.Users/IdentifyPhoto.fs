module DA.Doer.Users.IdentifyPhoto

open DA.Doer.Users.API.IdentifyPhoto
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
        IdentifyPhoto = fun orgId stream -> 
            searchSingleFace orgId stream context.FacePP

        FindUser = fun faceTokenId -> 
            getUserByPhotoId faceTokenId context.Mongo

        StorePhoto = fun userId stream -> 
            uploadStreamToStorageDirectoty userId stream context.Blob |> ``const`` ()
    }

let identifyPhoto principal stream = 
    mapContext >> identifyPhoto principal stream
    
module Errors =
        
    let getHttpError e =  
        e |>
        getHttpErrorWithDefaults 
            [
                matchFaceNotFoundException >> (mapOpt mapFaceNotFoundException)
                matchMultipleFacesFoundException >> (mapOpt mapMultipleFacesFoundException)
            ]
