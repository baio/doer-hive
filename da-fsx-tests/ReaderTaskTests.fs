module Tests

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FSharpx.Task
open DA.FSX.ReaderTask

[<Fact>]
let ``Create ReaderTask from success Result must work`` () =
    (1 |> returnM) 0

[<Fact>]
let ``Create ReaderTask from exception must fail with exception`` () =
    (new Exception("some exception") |> ofException) 0 
    |> fun x -> x.ContinueWith(fun (t: Task<_>) -> 
        Assert.IsType<AggregateException>(t.Exception) |> ignore
        t.Exception.InnerException.Message |> should equal "some exception"
        0
    )
    
[<Fact>]
let ``mapError for ReaderTask must work`` () =
    let me (e: exn) = new Exception(e.Message + "!")

    (new Exception("some exception") |> ofException |> mapError me) 0 
    |> fun x -> x.ContinueWith(fun (t: Task<_>) -> 
        Assert.IsType<AggregateException>(t.Exception) |> ignore
        t.Exception.InnerException.Message |> should equal "some exception!"
        0
    )
    
    

