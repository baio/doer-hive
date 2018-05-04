module DA.Doer.Users.UpdateAvatar

open DA.JWT
open DA.Doer.Users
open DA.Doer.Mongo
open DA.Auth0
open DA.Doer.Users.UpdateAvatar
open DA.FSX
open DA.FSX.ReaderTask
open DA.HTTP.Blob
open DA.Doer.Domain.Auth
open DA.Drawing.IamgeSharp


let request = DA.Http.HttpTask.HttpClient.httpClientRequest

let getUserProfile = getClaims >> map(profileFromClaims)

// collide the worlds!

type RegisterOrgConfig = 
    DA.Doer.Mongo.MongoConfig * DA.Auth0.API.Auth0APIConfig * DA.HTTP.Blob.BlobStorageConfig


let getDataAccess mongoConfig blobConfig = {

    UploadBlob = fun stream name -> 
        uploadStreamToStorage blobConfig stream name

    UpdateUserDocAvatar = fun userId url ->
        Users.updateUserAvatar userId url mongoConfig        
}


let getAuth ((http, config): Auth0APIConfig) = {
    GetPrincipalId = fun token ->
        {
            Issuer = config.clientDomain
            Audience = config.audience
            Jwks = ConfigJwksWellKnown

        } 
        |> getUserProfile token 
        |> Task.map(fun x -> x.Id)
    UpdateAvatar = fun userId url -> 
        updateUserAvatar (userId, url) (http, config) |> Task.``const`` true
}

let imageResizer = {
    ResizeImage = resizeImage
}


let mapContext = fun (mongoConfig, authConfig, blobStorageConfig) ->
    (getDataAccess mongoConfig blobStorageConfig), (getAuth authConfig), imageResizer

let updateAvatar token stream = mapContext >> updateAvatar token stream

module Errors =
    
    open DA.Doer.Users.Errors
    open DA.Doer.Domain.Errors
    open DA.Doer.Mongo.Errors
    open DA.Auth0.Errors

    let inline private map f x = Option.map f x
    
    let getHttpError (ex: exn) =  
        [                       
            matchNetworkException >> (map networkFail)
            matchRequestException >> (map requestFail)
            matchConnectionError >> (map connectionFail)                        
            unexepcted >> Some
        ] 
        |> List.choose(fun x -> x ex)
        |> List.head
        
