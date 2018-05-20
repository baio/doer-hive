module Auth0Tests

open System
open Xunit


open FsUnit.Xunit

open DA.FSX.ReaderTask
open DA.FSX.HttpTask.WebClient
open DA.Auth0
open DA.Auth.Domain
open DA.FSX

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
            ClientDomain = x.[0]
            ClientId = x.[1]
            ClientSecret = x.[2]
            Audience = x.[3]
        }
   
let auth0Config = getConfig()

// context

let context = createAuth0Api request auth0Config 

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

    let task = ((fun x -> x.UserId) <!> registerUser userInfo) |> andRemove

    task context

[<Fact>]
let ``Update user avatar work`` () =
    
    let userInfo = {
        UserId = "bd296071-f13f-4023-bf91-885ee0729135"
        OrgId = "5538ee3c-d332-44a5-ae3b-610e8b015325"
        Name = "uspdate user avatar"
        Email = "uspdate_user_avatar@gmail.com"
        Password = "PasLslol123"
        Avatar = null
        Role = "Owner"
    }

    let task = 
        readerTask {
            let! res = registerUser userInfo
            return! updateUserAvatar (res.UserId, "http://avatar.co/7") 
                |> bindError(fun ex -> 
                    ((returnM res.UserId) |> andRemove) >>= (fun _ -> ofException ex)                    
                )           
        } |> andRemove

    task context
