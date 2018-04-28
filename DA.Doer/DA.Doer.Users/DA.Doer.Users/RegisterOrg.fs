module DA.Doer.Users.RegisterOrg


open DA.Doer.Users
open DA.Doer.Mongo
open DA.Auth0
open DA.Doer.Users.RegisterOrgDTO
open DA.FSX.ReaderTask

let request = DA.Http.HttpTask.HttpClient.httpClientRequest

// collide the worlds!

type RegisterOrgConfig = 
    DA.Doer.Mongo.MongoConfig * DA.Auth0.API.Auth0APIConfig

let getDataAccess config = {
    insertDoc = function
        | User doc -> Users.createUser doc config
        | Org doc -> Orgs.createOrg doc config            
}

let getAuth config = {
    registerUser = fun userInfo -> registerUser userInfo config
}

let mapContext = fun (mongoConfig, authConfig) ->
    (getDataAccess mongoConfig), (getAuth authConfig)

let registerOrg info = mapContext >> registerOrg info

let registerOrgFromBody payload = mapContext >> registerOrgFromBody payload

module Errors =
    
    open DA.Doer.Users.Errors
    open DA.Doer.Domain.Errors
    open DA.Doer.Mongo.Errors
    open DA.Auth0.Errors

    let inline private map f x = Option.map f x
    
    let getHttpError (ex: exn) =  
        [
            matchValidationError >> (map validation)
            matchUniqueKeyError >> (map uniqueKey)
            matchUserAlreadyExistsError >> (map uniqueKeyUnexpected)
            unexepcted >> Some
        ] 
        |> List.choose(fun x -> x ex)
        |> List.head
        
