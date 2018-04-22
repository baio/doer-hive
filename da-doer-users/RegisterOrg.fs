module DA.Doer.Users.RegisterOrg


open DA.Doer.Users
open DA.Doer.Mongo
open DA.Auth0
open DA.FSX.ReaderTask

let request = DA.FSX.HttpTask.WebClient.webClientRequest

// collide the worlds!

type RegisterOrgConfig = 
    DA.Doer.Mongo.MongoConfig * DA.Auth0.API.Auth0APIConfig

let getDataAccess config = {
    insertDoc = function
        | User doc -> Users.createUser doc config
        | Org doc -> Orgs.createOrg doc config            
}

let getAuth config = {
    registerUser = fun userInfo -> (managementTokenMem >>= registerUser userInfo) config
}

let registerOrg info = fun (mongoConfig, authConfig) ->
    let context = (getDataAccess mongoConfig), (getAuth authConfig)
    (registerOrg info) context