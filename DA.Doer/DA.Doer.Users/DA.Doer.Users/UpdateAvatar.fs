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

let trimBearer (x: string) = x.Split([|' '|]).[1]

let userGuid (x: string) = x.Split([|'|'|]).[2]

let getUserProfile = trimBearer >> getClaims >> map(profileFromClaims)

// collide the worlds!

type RegisterOrgConfig = 
    DA.Doer.Mongo.MongoConfig * DA.Auth0.API.Auth0Api * DA.HTTP.Blob.BlobStorageConfig


let getDataAccess mongoConfig blobConfig = {

    UploadBlob = fun stream name -> 
        uploadStreamToStorage (name + ".jpg") stream blobConfig 

    UpdateUserDocAvatar = fun userId url ->
        Users.updateUserAvatar (userGuid userId) url mongoConfig        
}


let getAuth ((http, config): Auth0Api) (jwt: DA.JWT.Config) = {

    GetPrincipalId = fun token ->
        jwt |> getUserProfile token |> Task.map(fun x -> x.Id)

    UpdateAvatar = fun userId url -> 
        updateUserAvatar (userId, url) (http, config) |> Task.``const`` true
}

let imageResizer = {
    ResizeImage = resizeJpeg
}

let mapContext = fun (mongoConfig, authConfig, blobStorageConfig, jwtConfig) ->
    {
        DataAccess = getDataAccess mongoConfig blobStorageConfig
        Auth = getAuth authConfig jwtConfig
        ImageResizer = imageResizer
    }

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
        
