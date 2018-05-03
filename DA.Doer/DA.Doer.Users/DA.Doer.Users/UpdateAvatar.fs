module DA.Doer.Users.UpdateAvatar

open DA.Doer.Users
open DA.Doer.Mongo
open DA.Auth0
open DA.Doer.Users.UpdateAvatar
open DA.FSX.ReaderTask

let request = DA.Http.HttpTask.HttpClient.httpClientRequest

// collide the worlds!

type RegisterOrgConfig = 
    DA.Doer.Mongo.MongoConfig * DA.Auth0.API.Auth0APIConfig * DA.HTTP.Blob.BlobStorageConfig

(*
let getDataAccess mongoConfig blobConfig = {
    UploadAvatar = fun prms -> 
        DA.HTTP.Blob.uploadStreamToStorage 
            blobConfig
            prms.Stream
            prms.BlobName

    UpdateUserDocAvatar = (string * string) -> Task<bool>
}

let getAuth config = {
    registerUser = fun userInfo -> registerUser userInfo config
}

let mapContext = fun (mongoConfig, authConfig) ->
    (getDataAccess mongoConfig), (getAuth authConfig)

let registerOrg info = mapContext >> registerOrg info

module Errors =
    
    open DA.Doer.Users.Errors
    open DA.Doer.Domain.Errors
    open DA.Doer.Mongo.Errors
    open DA.Auth0.Errors

    let inline private map f x = Option.map f x
    
    let getHttpError (ex: exn) =  
        [
            matchValidationError >> (map validation)
            matchNetworkException >> (map networkFail)
            matchRequestException >> (map requestFail)
            matchConnectionError >> (map connectionFail)
            matchUniqueKeyError >> (map uniqueKey)
            matchUserAlreadyExistsError >> (map uniqueKeyUnexpected)
            unexepcted >> Some
        ] 
        |> List.choose(fun x -> x ex)
        |> List.head
        
*)