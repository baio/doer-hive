module Config

open DA.Auth0
open DA.Doer.Mongo
open DA.JWT
open DA.HTTP.Blob
open DA.FacePlusPlus

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
        "facepp:apiKey"
        "facepp:apiSecret"
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
        },
        {
            ApiKey =    x.[13]
            ApiSecret = x.[14]
        }

   
let authConfig, jwtConfig, mongoConfig, blobConfig, facePPConfig = getConfig()

let mongoApi = {
    Db = getDb mongoConfig
}

let blobApi = {
    Container = blobConfig |> getBlobClient |> getBlobContainer "user-photos"
#if DEBUG_LOCAL_DEVICE
    NormalizeUrl = fun x -> x.Replace("localhost:10000", "192.168.0.100:778")
#else 
    NormalizeUrl = id
#endif
}

let faceppApi = {
    Config = facePPConfig
    Http = request
}

let auth0Api = createAuth0Api request authConfig

(*
let context  = (mongoApi, auth0Api)
let context2 = (mongoApi, auth0Api, blobApi, jwtConfig)
*)


