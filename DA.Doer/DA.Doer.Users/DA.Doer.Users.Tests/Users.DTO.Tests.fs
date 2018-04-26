module Users.DTO.Tests

open System
open Xunit
open DA.Doer.Users.RegisterOrg
open DA.FSX.ReaderTask

open Setup
open System.Threading.Tasks
open DA.FSX
open DA.Doer.Domain.Errors
open FsUnit.Xunit

[<Fact>]
let ``Register org with invalid dto must give correct error`` () =

    let payload =  """
        { "firstName" : 1 }
    """

    let assert' = 
        should equal
            [
                "firstName", "NOT_STRING_ERROR"
                "lastName", "NULL_ERROR"
                "orgName", "NULL_ERROR"
                "email", "NULL_ERROR"
                "phone", "NULL_ERROR"
                "password", "NULL_ERROR"
            ]

    let task = (registerOrgFromBody payload) context

    task.ContinueWith(fun (t: Task<_>) ->
            Assert.IsType<AggregateException>(t.Exception) |> ignore
            Assert.IsType<ValidationException>(t.Exception.InnerException) |> ignore
            assert' (t.Exception.InnerException :?> ValidationException).Errors
            0
        )

[<Fact>]
let ``Register org with correct dto must work`` () =

    let payload =  """
        {
            "firstName" : "first",
            "lastName" : "last",
            "middleName" : "mid",
            "orgName" : "first",
            "email" : "dto_registered@gmail.com",
            "phone" : "+777777777",
            "password" : "LastPas123"
        }
    """

    (registerOrgFromBody payload >>= andRemove) context







