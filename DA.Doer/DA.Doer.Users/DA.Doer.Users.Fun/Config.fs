module Config

open DA.Auth0

open DA.Doer.Mongo

let request = DA.FSX.HttpTask.WebClient.webClientRequest

let getConfig () = 
    [
        "auth0:clientDomain"
        "auth0:clientId"
        "auth0:clientSecret"
        "auth0:audience"
        "mongo:connection"
        "mongo:dbName"
    ] 
    |> DA.AzureKeyVault.getConfigSync "azureKeyVault:name"
    |> fun x -> 
        {
            clientDomain = x.[0]
            clientId = x.[1]
            clientSecret = x.[2]
            audience = x.[3]
        },
        {
            connection = x.[4]
            dbName = x.[5]
        }
   
let authConfig, mongoConfig = getConfig()

let context = (mongoConfig, (request, authConfig))


