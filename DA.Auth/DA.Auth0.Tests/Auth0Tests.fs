module Auth0Tests

open System
open Xunit


open FsUnit.Xunit

open DA.FSX.ReaderTask
open DA.FSX.HttpTask.WebClient
open DA.Auth0
open DA.Auth.Domain

let request = DA.Http.HttpTask.HttpClient.httpClientRequest

// config

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
   
let auth0Config = getConfig()

// context

let context = request, auth0Config

let andRemove f = readerTask {
        let! userId = f 
        userId |> should not' Empty 
        let! removeResult = removeUser userId
        removeResult |> should equal true
    }

[<Fact>]
let ``Create user must work`` () =
    
    let userInfo = {
        UserId = "bd296071-f13f-4023-bf91-885ee0729136"
        OrgId = "5538ee3c-d332-44a5-ae3b-610e8b015326"
        Name = "create user 1"
        Email = "create_user_1@gmail.com"
        Password = "PasLslol123"
        Avatar = "http://avatars.com/1"
        Role = "Owner"
    }

    let task = createUser userInfo |> andRemove 

    task context

[<Fact>]
let ``Register user must work`` () =
    
    let userInfo = {
        UserId = "bd296071-f13f-4023-bf91-885ee0729136"
        OrgId = "5538ee3c-d332-44a5-ae3b-610e8b015326"
        Name = "register user 1"
        Email = "regsiter_user_1@gmail.com"
        Password = "PasLslol123"
        Avatar = "http://avatars.com/1"
        Role = "Owner"
    }

    let task = ((fun x -> x.userId) <!> registerUser userInfo) |> andRemove

    task context
