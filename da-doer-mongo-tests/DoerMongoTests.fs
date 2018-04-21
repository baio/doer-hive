module DoerMongoTests


open System
open Xunit
open FsUnit.Xunit
open FSharpx.Task
open DA.Doer.Mongo
open DA.Doer.Orgs
open DA.Doer.Users

// tests for local
let config = {
    connection = "mongodb://localhost"
    dbName = "local"
}

[<Fact>]
let ``Create org must work`` () =
    
    let org = 
        {
            Name = "new org"
            OwnerEmail = "new_org@gmail.com"
        } 
    
    let assert' = should not' Empty

    assert' <!> (createOrg org) config

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

    assert' <!> (createUser user) config
