module UsersTests

open System
open Xunit


open DA.Auth0

open DA.Doer.Mongo
open FSharpx.Task
open DA.FSX.ReaderTask
open DA.Doer.Users

let request = DA.FSX.HttpTask.WebClient.webClientRequest

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

let andRemove result (mongo: MongoConfig, auth: Auth0APIConfig) = 
    [
        Orgs.removeOrg result.orgId mongo
        Users.removeUser result.userId mongo       
        (API.removeUser result.authUserId *> returnM "ok") auth
    ]
    |> Seq.map(fun x -> (fun () -> x))
    |> Parallel


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

    (RegisterOrg.registerOrg user >>= andRemove) (mongoConfig, (request, authConfig))

[<Fact>]
let ``Register org from dto must work`` () =
    
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

    (RegisterOrg.registerOrg user >>= andRemove) (mongoConfig, (request, authConfig))
