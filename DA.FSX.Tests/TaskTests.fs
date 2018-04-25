module TaskTests


open Xunit
open System.Threading.Tasks
open FSharpx.Task
open DA.FSX.Task
open FsUnit.Xunit

[<Fact>]
let ``For Task monad associativity low must work`` () =
    
    let am x = returnM x
    let bm x = returnM x
    let cm x = returnM x

    let l = am >=> bm >=> cm
    let r = bm >=> cm >=> am

    let rl = run (fun () -> l 1)
    let rr = run (fun () -> r 1)

    Assert.Equal(rl, rr)

[<Fact>]
let ``For Task monad associativity low must work even if some conatins error result`` () =
    
    let am x = returnM x
    let bm x = returnM x
    let cm _ = Task.FromException<int> (exn "error")

    let l = am >=> bm >=> cm
    let r = bm >=> cm >=> am

    let rl = run (fun () -> l 1)
    let rr = run (fun () -> r 1)

    let z = 0

    match (rl, rr) with
        | Error e1, Error e2 ->
            // exceptions not the same by ref, but all fields the same
            e1.InnerException.Message |> should equal e2.InnerException.Message
            e1.InnerException.StackTrace |> should equal e2.InnerException.StackTrace
        | _ -> Assert.True(false)