module Tests

open DA.Doer.Domain.Validators
open System
open Xunit
open FsUnit.Xunit
open FSharpx
open FSharpx.Collections

let fail x = Result.mapError NonEmptyList.singleton x

let inline (<!>) f x = Result.map f x

let ap x f =
    match f,x with
    | Ok f, Ok x -> Ok (f x)
    | Error e, _            -> Error e
    | _           , Error e -> Error e

let inline (<*>) f x = ap x f

let inline lift2 f a b = f <!> a <*> b

let inline ( *>) a b = lift2 (fun _ z -> z) a b


[<Fact>]
let ``My test`` () =
    
    let r1 = Ok 1
    let r2 = Ok 2

    let assert' a b =
        // impossible
        true |> should equal false

    (assert' <!> (r1 |> notNullString |> fail) <*> (r2 |> notNullString |> fail))
    |> function
    | Error err ->
        let x = 0
        err |> should be Empty
    | Ok _ ->
        ()
