module FSharpx.Tests.ValidationExample

// ported from original in Scalaz: https://gist.github.com/970717

open FsUnit

open FSharpx
open FsUnit.Xunit
open FSharpx.Functional
open FSharpx.Collections
open FSharpx.Choice
open FSharpx.Validation
open Xunit

// First let's define a domain.

type Sobriety = Sober | Tipsy | Drunk | Paralytic | Unconscious

type Gender = Male | Female

type Person = {
    Gender: Gender
    Age: int
    Clothes: string Set
    Sobriety: Sobriety
}

let Success = Choice1Of2
let Failure = Choice2Of2

// Let's define the checks that *all* nightclubs make!
module Club =
    let checkAge (p: Person) =
        if p.Age < 18 then 
            Failure "Too young!"
        elif p.Age > 40 then
            Failure "Too old!"
        else
            Success p

    let checkClothes (p: Person) =
        if p.Gender = Male && not (p.Clothes.Contains "Tie") then
            Failure "Smarten up!"
        elif p.Gender = Female && p.Clothes.Contains "Trainers" then
            Failure "Wear high heels"
        else
            Success p

    let checkSobriety (p: Person) =
        match p.Sobriety with
        | Drunk | Paralytic | Unconscious -> Failure "Sober up!"
        | _ -> Success p

// Now let's compose some validation checks

module ClubbedToDeath =
    open Club
    // PERFORM THE CHECKS USING Monadic "computation expression" SUGAR
    let either = EitherBuilder()
    let costToEnter p =
        either {
            let! a = checkAge p
            let! b = checkClothes a
            let! c = checkSobriety b
            return 
                match c.Gender with
                | Female -> 0m
                | Male -> 5m
        }

    // or composing functions:

    let costToEnter2 =
        let costByGender (p: Person) = 
            match p.Gender with
            | Female -> 0m
            | Male -> 5m
        let checkAll = checkAge >=> checkClothes >=> checkSobriety // kleisli composition
        checkAll >> Choice.map costByGender

// Now let's see these in action

let Ken = { Person.Gender = Male; Age = 28; Clothes = set ["Tie"; "Shirt"]; Sobriety = Tipsy }
let Dave = { Person.Gender = Male; Age = 41; Clothes = set ["Tie"; "Jeans"]; Sobriety = Sober }
let Ruby = { Person.Gender = Female; Age = 25; Clothes = set ["High heels"]; Sobriety = Tipsy }

// let's go clubbing!

module ClubTropicana =
    open Club
    let failToList x = Choice.mapSecond NonEmptyList.singleton x
    let costByGender (p: Person) =
        match p.Gender with
        | Female -> 0m
        | Male -> 7.5m

    //PERFORM THE CHECKS USING applicative functors, accumulating failure via a monoid

    let costToEnter p =
        costByGender <!> (checkAge p |> failToList) *> (checkClothes p |> failToList) *> (checkSobriety p |> failToList)


// And the use? Dave tried the second nightclub after a few more drinks in the pub
[<Fact>]
let part2() =
    let z = ClubTropicana.costToEnter { Dave with Sobriety = Paralytic } 
    
    ClubTropicana.costToEnter Ruby |> should equal (Success 0m)

(**
 *
 * So, what have we done? Well, with a *tiny change* (and no changes to the individual checks themselves),
 * we have completely changed the behaviour to accumulate all errors, rather than halting at the first sign
 * of trouble. Imagine trying to do this using exceptions, with ten checks.
 *
 *)

(**
 *
 * Part Three : Gay bar
 *
 * And for those wondering how to do this with a *very long list* of checks.
 *
 *)

 (*
module GayBar =
    open Club
    let checkGender (p: Person) =
        match p.Gender with
        | Male -> Success p
        | _ -> Failure "Men only"

    let costToEnter p =
        [checkAge; checkClothes; checkSobriety; checkGender]
        |> Validation.mapM (fun check -> check p |> Choice.mapSecond NonEmptyList.singleton)
        |> Choice.map (function x::_ -> decimal x.Age + 1.5m | [] -> failwith "costToEnter")

[<Test>]
let part3() =
    GayBar.costToEnter { Person.Gender = Male; Age = 59; Clothes = set ["Jeans"]; Sobriety = Paralytic } 
    |> shouldEqual (Failure (NonEmptyList.create "Too old!" ["Smarten up!"; "Sober up!"]))

    GayBar.costToEnter { Person.Gender = Male; Age = 25; Clothes = set ["Tie"]; Sobriety = Sober } |> shouldEqual (Success 26.5m)
*)