module DA.Doer.Users.CreateWorker

open DA.Doer.Users.API.CreateWorker
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
        PrincipalAncestors = fun principalId -> 
            getUserAncestors principalId context.Mongo

        Insert = fun doc -> 
            createUser doc context.Mongo
    }

let createWorker principal stream = 
    mapContext >> createWorker principal stream
    
module Errors =
        
    let getHttpError e = e |> getHttpErrorWithDefaults []
