module Setup

open FSharpx.Task
open DA.FSX.ReaderTask
open DA.JWT
open DA.Auth0
open FsUnit.Xunit

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
        (request, {
            ClientDomain = x.[0]
            ClientId = x.[1]
            ClientSecret = x.[2]
            Audience = x.[3]
        }),
        {
            Audience = x.[3]
            Issuer = x.[4]            
            Jwks = ConfigJwksConst (x.[5], x.[6]) // ConfigJwks.ConfigJwksWellKnown // ConfigJwksConst jwks
        }

   
let authConfig, jwtConfig = getConfig()
