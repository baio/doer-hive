module DA.Doer.Users.RegisterOrg

open DA.Doer.Users
open DA.Doer.Mongo
open DA.Auth0
open DA.Doer.Users.RegisterOrgDTO
open DA.FSX.ReaderTask

let request = DA.Http.HttpTask.HttpClient.httpClientRequest

// collide the worlds!

type RegisterOrgConfig = {
    Mongo: DA.Doer.Mongo.MongoAPI
    Auth0: DA.Auth0.API.Auth0Api
}

let getDataAccess config = {
    InsertDoc = function
        | User doc -> Users.createUser doc config
        | Org doc -> Orgs.createOrg doc config            
}

let getAuth config = {
    RegisterUser = fun userInfo -> registerUser userInfo config
}

let mapContext = fun (config: RegisterOrgConfig) ->
    {
        DataAccess = getDataAccess config.Mongo
        Auth = getAuth config.Auth0
    }

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
            matchNetworkException >> (map networkFail)
            matchRequestException >> (map requestFail)
            matchConnectionError >> (map connectionFail)
            matchUniqueKeyError >> (map uniqueKey)
            matchUserAlreadyExistsError >> (map uniqueKeyUnexpected)
            unexepcted >> Some
        ] 
        |> List.choose(fun x -> x ex)
        |> List.head
        
