module Users.DTO.Tests

open System
open Xunit
open DA.Doer.Users.RegisterOrgDTO


[<Fact>]
let ``Parse payload must work`` () =
    
    let payload =  """ 
        { "firstName" : 1 }
        """

    let x = fromPayload payload

    let t = 1

    0

    
