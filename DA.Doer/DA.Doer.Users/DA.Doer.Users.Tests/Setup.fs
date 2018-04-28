module Setup

open DA.Auth0

open DA.Doer.Mongo
open FSharpx.Task
open DA.FSX.ReaderTask
open DA.Doer.Users

let request = DA.HTTP.HttpTask.HttpClient.httpClientRequest

let getConfig () = 
    [
        "auth0:clientDomain"
        "auth0:clientId"
        "auth0:clientSecret"
        "auth0:audience"
    ] 
    |> DA.AzureKeyVault.getConfigSync "azureKeyVault:name"
    |> fun x -> 
        {
            clientDomain = x.[0]
            clientId = x.[1]
            clientSecret = x.[2]
            audience = x.[3]
        }
   
let authConfig = getConfig()

let mongoConfig = {
    connection = "mongodb://localhost"
    dbName = "local"
}

let context = (mongoConfig, (request, authConfig))

let andRemove (result: RegisterOrgResult) (mongo: MongoConfig, auth: Auth0APIConfig) = 
    [
        Orgs.removeOrg result.orgId mongo
        Users.removeUser result.userId mongo       
        (API.removeUser result.authUserId *> returnM "ok") auth
    ]
    |> Seq.map(fun x -> (fun () -> x))
    |> Parallel

