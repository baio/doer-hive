module DA.Doer.Users.RegisterOrg

open DA.Doer.Users.API
open DA.Doer.Users.RegisterUser
open DA.Doer.Mongo
open DA.Auth0.API
open DA.FSX.ReaderTask

let request = DA.FSX.HttpTask.WebClient.chainWebClient (new System.Net.WebClient())
let token = managementTokenMem

// collide the worlds!

type RegisterOrgConfig = 
    DA.Doer.Mongo.API.MongoConfig * DA.Auth0.Auth0Config

let getDataAccess config = {
    insertDoc = function
        | User doc -> Users.createUser doc config
        | Org doc -> Orgs.createOrg doc config            
}

let getAuth config = {
    registerUser = fun userInfo -> (token >>= registerUser userInfo) (request, config)
}

let registerOrg info = fun (mongoConfig, authConfig) ->
    let context = (getDataAccess mongoConfig), (getAuth authConfig)
    (API.registerOrg info) context