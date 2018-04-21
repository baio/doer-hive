module DA.Doer.Users.RegisterOrg

open DA.Doer.Users.API
open DA.Doer.Users.RegisterUser
open DA.Doer.Mongo
open DA.Auth0.API
open DA.FSX.ReaderTask

// collide the worlds!
type RegisterOrgConfig = {
    mongoConfig: DA.Doer.Mongo.API.MongoConfig
    authConfig: DA.Auth0.API.HttpRequest * DA.Auth0.Auth0Config
}

let getDataAccess config = {
    insertDoc = function
        | User doc -> Users.createUser doc config
        | Org doc -> Orgs.createOrg doc config            
}

let getAuth config token = {
    registerUser = fun userInfo -> registerUser userInfo token config
}

let registerOrg info token = fun (config: RegisterOrgConfig) ->
    let context = (getDataAccess config.mongoConfig), (getAuth config.authConfig token)
    (API.registerOrg info) context