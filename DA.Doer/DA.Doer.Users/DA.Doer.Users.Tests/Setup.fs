﻿module Setup

open DA.Auth0

open DA.Doer.Mongo
open FSharpx.Task
open DA.FSX.ReaderTask
open DA.Doer.Users
open DA.HTTP.Blob
open DA.JWT
open DA.Doer.Users.RegisterOrg

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
            Jwks = ConfigJwksConst (x.[5], x.[6])
        }
   
let authConfig, jwtConfig = getConfig()

let mongoConfig = {
    Connection = "mongodb://localhost"
    DbName = "doer-local"
}

let blobStorageConfig: BlobStorageConfig = {
    Uri = "http://127.0.0.1:10000/devstoreaccount1"
    AccountName = "devstoreaccount1"
    AccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="
    ContainerName = "test-images"
}

let mongoApi = {
    Db = getDb mongoConfig
}

let auth0Api = createAuth0Api request authConfig

let andRemove (result: RegisterOrgResult) (api: RegisterOrgApi) = 
    [
        Orgs.removeOrg result.orgId api.Mongo
        Users.removeUser result.userId api.Mongo      
        (API.removeUser result.authUserId *> returnM "ok") api.Auth0
    ]
    |> FSharpx.Task.sequence

