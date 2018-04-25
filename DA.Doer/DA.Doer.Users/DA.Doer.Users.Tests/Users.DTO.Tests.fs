module Users.DTO.Tests

open System
open Xunit
open DA.Doer.Users.RegisterOrg
open DA.FSX.ReaderTask

open Setup
open System.Threading.Tasks
open DA.FSX
open DA.Doer.Domain.Errors

[<Fact>]
let ``Register org from dto must work`` () =
    
    let payload =  """ 
        { "firstName" : 1 }
    """
    
    let task = (registerOrgDTO payload) context

    let t = Threading.Tasks.Task.FromException<_> (new Exception "test")

    let task1 = t.ContinueWith(fun (t: Task<_>) -> if t.IsFaulted then raise(t.Exception.InnerException) else t.Result )

    task1.ContinueWith(fun (t: Task<_>) -> 
            Assert.IsType<AggregateException>(t.Exception) |> ignore
            Assert.IsType<ValidationException>(t.Exception.InnerException) |> ignore
            0
        )





    
