module UsersTests

open System
open Xunit
open DA.Auth0
open DA.Doer.Mongo.API
open DA.Doer.Users.RegisterUser.API

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
   
let authConfig = getConfig()

let mongoConfig = {
    connection = "mongodb://localhost"
    dbName = "local"
}

[<Fact>]
let ``Register org must work`` () =
    
    let user =  {
            User = {
                Name = {
                    FirstName = "first"
                    LastName = "last"
                    MiddleName = "middle"
                }
                Phone = "+777777777"
                Email = "registered-org@email.com"
                Avatar = "http://avatar.com/1"
            }
            Org = {
                Name = "org"
            }
            Password = "LastPas123"
        }

    (DA.Doer.Users.RegisterOrg.registerOrg user) (mongoConfig, authConfig)
