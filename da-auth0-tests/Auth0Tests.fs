module Auth0Tests

open System
open Xunit


open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FSharpx.Task

open System.Net

open DA.FSX
open DA.FSX.ReaderTask
open DA.FSX.HttpTask.WebClient
open DA.Auth0
open RequestAPI
open API

let request = chainWebClient (new WebClient())
let token = managementTokenMem

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

[<Fact>]
let ``Create user must work`` () =
    
    let userInfo = {
        userId = "bd296071-f13f-4023-bf91-885ee0729136"
        orgId = "5538ee3c-d332-44a5-ae3b-610e8b015326"
        name = "signup user 1"
        email = "signup_user_1@gmail.com"
        password = "PasLslol123"
        avatar = "http://avatars.com/1"
        role = "Owner"
    }

    let task = fun token -> readerTask {
        let! userToken = createUser userInfo token
        userToken |> should not' Empty 
        let! removeResult = removeUser ("doer|" + userInfo.userId) token
        removeResult |> should equal true
    }

    (token >>= task) context
