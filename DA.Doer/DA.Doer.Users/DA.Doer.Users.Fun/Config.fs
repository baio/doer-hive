module Config

open DA.Auth0

open DA.Doer.Mongo

open DA.JWT

open DA.HTTP.Blob

let request = DA.Http.HttpTask.HttpClient.httpClientRequest

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
            ClientDomain = x.[0]
            ClientId = x.[1]
            ClientSecret = x.[2]
            Audience = x.[3]
        },
        {   
            Audience = x.[3]
            Issuer = x.[4]
            Jwks = ConfigJwksConst(x.[5], x.[6])                
        },
        {
            Connection = x.[7]
            DbName = x.[8]
        },
        {
            Uri = x.[9]
            AccountName = x.[10]
            AccountKey = x.[11]
            ContainerName = x.[12]
        }
   
let authConfig, jwtConfig, mongoConfig, blobConfig = getConfig()

let mongoApi = {
    Db = getDb mongoConfig
}

let blobApi = {
    Container = blobConfig |> getBlobClient |> getBlobContainer "user-photos"
}

let context  = (mongoApi, (request, authConfig))
let context2 = (mongoApi, (request, authConfig), blobApi, jwtConfig)


