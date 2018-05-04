module UsersTests

open System
open Xunit

open DA.Auth0

open DA.FSX.ReaderTask
open DA.Doer.Users
open Setup
open System.Text
open System.IO

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
    UpdateAvatar.updateAvatar token url (mongo, auth, blobStorageConfig)

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
                Email = "registered-org@email.com"
                Avatar = "http://avatar.com/1"
            }
            Org = {
                Name = "org"
            }
            Password = "LastPas123"
        }
    
    let stream = new MemoryStream(buffer = Encoding.UTF8.GetBytes("lol")) :> Stream
    
    readerTask {
        let! res = RegisterOrg.registerOrg user
        let! _ = updateAvatar res.tokens.accessToken stream
        return! andRemove res
    } <| context

