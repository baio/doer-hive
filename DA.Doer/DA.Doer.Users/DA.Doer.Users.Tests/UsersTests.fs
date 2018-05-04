module UsersTests

open System
open Xunit

open DA.Auth0

open DA.FSX.ReaderTask
open DA.Doer.Users
open Setup
open System.Text
open System.IO
open DA.FSX

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

    (RegisterOrg.registerOrg user >>= andRemove) context


let updateAvatar token url (mongo, auth) = 
    UpdateAvatar.updateAvatar token url (mongo, auth, blobStorageConfig, jwtConfig)

[<Fact>]
let ``Update user avatar must work`` () =

    let user =  {
            User = {
                Name = {
                    FirstName = "first"
                    LastName = "last"
                    MiddleName = "middle"
                }
                Phone = "+777777777"
                Email = "update-user-avatar-org@email.com"
                Avatar = "http://avatar.com/1"
            }
            Org = {
                Name = "update-user-avatar-org"
            }
            Password = "LastPas123"
        }
    
    let stream = new MemoryStream(buffer = Encoding.UTF8.GetBytes("lol")) :> Stream
    
    RegisterOrg.registerOrg user
    >>= (fun res -> 
        updateAvatar res.tokens.accessToken stream
        |> bindError(fun ex -> 
            andRemove res >>= (fun _ -> ofException ex)
        )
        |> mapc res
    )
    >>= andRemove
    <| context

