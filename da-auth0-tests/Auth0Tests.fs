module Auth0Tests

open System
open Xunit

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FSharpx.Task

open System.Net

open DA.FSX.HttpTask.WebClient
open DA.Auth0
open RequestAPI
open API

let request = chainWebClient (new WebClient())

// config

let getConfig () = 
    [
        "auth0:clientDomain"
        "auth0:clientId"
        "auth0:clientSecret"
        "auth0:audience"
    ] 
    |> DA.AzureKeyVault.getConfig "azureKeyVault:name"
    |> map(fun x -> 
        {
            clientDomain = x.[0]
            clientId = x.[1]
            clientSecret = x.[2]
            audience = x.[3]
        }
    )


let configTask = getConfig()
configTask.Wait()
let auth0Config = configTask.Result

// context

let context = request, auth0Config

[<Fact>]
let ``Create user must work`` () =
    (*
        email = "email", "create-user-2@gmail.com" :> obj
        password = "password", "PasLslol123" :> obj
        role = "role", "Owner" :> obj
        avatar = "avatar", "http://avatars.com/1" :> obj
        userId = "userId", "bd296071-f13f-4023-bf91-885ee0729136" :> obj
        orgId = "orgId", "5538ee3c-d332-44a5-ae3b-610e8b015326" :> obj
        name = "name", "signup user" :> obj
    *)
    let userInfo = {
        userId = "bd296071-f13f-4023-bf91-885ee0729136"
        orgId = "5538ee3c-d332-44a5-ae3b-610e8b015326"
        name = "signup user 1"
        email = "signup_user_1@gmail.com"
        password = "PasLslol123"
        avatar = "http://avatars.com/1"
        role = "Owner"
    }

    let assert' = should not' Empty

    assert' <!> createUser userInfo context
