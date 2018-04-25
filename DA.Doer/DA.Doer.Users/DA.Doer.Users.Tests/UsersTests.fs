module UsersTests

open System
open Xunit

open DA.Auth0

open DA.FSX.ReaderTask
open DA.Doer.Users
open Setup

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

