module Config

open DA.Auth0

open DA.Doer.Mongo

open DA.JWT

open DA.HTTP.Blob

let request = DA.FSX.HttpTask.WebClient.webClientRequest

let getConfig () = 
    [
        "auth0:clientDomain"
        "auth0:clientId"
        "auth0:clientSecret"
        "auth0:audience"
        "auth0:issuer"
        "jwks:configuration"
        "jwks:keys"
        "mongo:connection"
        "mongo:dbName"
        "blob:uri"
        "blob:accountName"
        "blob:accountKey"
        "blob:containerName"
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
            Audience = x.[3]
            Issuer = x.[4]
            Jwks = ConfigJwksConst(x.[5], x.[6])                
        },
        {
            connection = x.[7]
            dbName = x.[8]
        },
        {
            Uri = x.[9]
            AccountName = x.[10]
            AccountKey = x.[11]
            ContainerName = x.[12]
        }
   
let authConfig, jwtConfig, mongoConfig, blobConfig = getConfig()

let context  = (mongoConfig, (request, authConfig))
let context2 = (mongoConfig, (request, authConfig), blobConfig, jwtConfig)


