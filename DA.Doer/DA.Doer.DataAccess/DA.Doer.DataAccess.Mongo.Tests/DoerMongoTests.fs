module DoerMongoTests


open System
open Xunit
open FsUnit.Xunit
open DA.DataAccess.Domain
open DA.Doer.Mongo
open DA.FSX.ReaderTask

// tests for local
let config = {
    Connection = "mongodb://localhost"
    DbName = "doer-local"
}    

let api = {
    Db = getDb config
}

[<Fact>]
let ``Create org must work`` () =
    
    let org = 
        {
            Name = "new org"
            OwnerEmail = "new_org@gmail.com"
        } 
    
    let assert' = should not' Empty

    (assert' <!> (createOrg org >>= removeOrg)) api

[<Fact>]
let ``ownerEmail restriction must work on orgs`` () =
    
    let org = 
        {
            Name = "new org"
            OwnerEmail = "new_org@gmail.com"
        } 

    let org1 = 
        {
            Name = "new org 1"
            OwnerEmail = "new_org@gmail.com"
        } 
    
    (createOrg org)
    >>= (fun x -> 
        createOrg org1
        |> bind(fun y ->
            removeOrg x *> returnM(Error y)
        )
        |> bindError (fun e ->    
            removeOrg x *> returnM(Ok x)
        )
    )
    |> map (function
        // expected error on createOrg org1
        | Ok x -> 
            x |> should not' Empty
        // unexpected success on createOrg org1
        | Error x -> x |> should be Empty        
    )    
    <| api
    
[<Fact>]
let ``name restriction must work on orgs`` () =
    
    let org = 
        {
            Name = "new org"
            OwnerEmail = "new_org@gmail.com"
        } 

    let org1 = 
        {
            Name = "new org"
            OwnerEmail = "new_org1@gmail.com"
        } 
    
    (createOrg org)
    >>= (fun x -> 
        createOrg org1
        |> bind(fun y ->
            removeOrg x *> returnM(Error y)
        )
        |> bindError (fun _ ->    
            removeOrg x *> returnM(Ok x)
        )
    )
    |> map (function
        // expected error on createOrg org1
        | Ok x -> 
            x |> should not' Empty
        // unexpected success on createOrg org1
        | Error x -> x |> should be Empty        
    )    
    <| api
    

[<Fact>]
let ``email restriction must work on users`` () =
    
    let user = 
        {
            OrgId = "10"
            Role = "Owner"
            FirstName = "first"
            MidName = "mis"
            LastName = "last"
            Email = "first_mid_last@gmail.com"
            Phone = "+79772753595"
            Ancestors = []
            Avatar = ""
        } 

    let user1 = 
        {
            OrgId = "10"
            Role = "Owner"
            FirstName = "first"
            MidName = "mis"
            LastName = "last"
            Email = "first_mid_last@gmail.com"
            Phone = "+79772753595"
            Ancestors = []
            Avatar = ""
        } 
    
    (createUser user)
    >>= (fun x -> 
        createUser user1
        |> bind(fun y ->
            removeUser x *> returnM(Error y)
        )
        |> bindError (fun _ ->    
            removeUser x *> returnM(Ok x)
        )
    )
    |> map (function
        // expected error on createUser user1
        | Ok x -> 
            x |> should not' Empty
        // unexpected success on createUser user1
        | Error x -> x |> should be Empty        
    )    
    <| api


[<Fact>]
let ``Create user must work`` () =
    
    let user = 
        {
            OrgId = "10"
            Role = "Owner"
            FirstName = "first"
            MidName = "mis"
            LastName = "last"
            Email = "first_mid_last@gmail.com"
            Phone = "+79772753595"
            Ancestors = []
            Avatar = ""
        } 
    
    let assert' = should not' Empty

    (assert' <!> (createUser user >>= removeUser)) api

[<Fact>]
let ``Update user avatar must work`` () =
    
    let user = 
        {
            OrgId = "11"
            Role = "Owner"
            FirstName = "first"
            MidName = "mis"
            LastName = "last"
            Email = "first_mid_last_1@gmail.com"
            Phone = "+79772753595"
            Ancestors = []
            Avatar = ""
        } 
    
    let assert' = should not' Empty

    (assert' <!> (
        readerTask {
            let! userId  = createUser user 
            let! _ = updateUserAvatar userId "http://avatar.co/lol.jpg"
            let! _ = removeUser userId
            return userId
        }
    )) api
