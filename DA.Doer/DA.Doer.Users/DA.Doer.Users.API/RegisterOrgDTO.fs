﻿module DA.Doer.Users.RegisterOrgDTO



open DA.Doer.Domain.Errors
open DA.Doer.Domain.Validators
open DA.FSX.ValidationResult
open DA.FSX
open FSharpx.Reader

let mf = mapFailLabeled

type RegisterOrgInfoDTO =
    { 
        firstName: obj
        middleName: obj
        lastName: obj
        orgName: obj
        email: obj
        phone: obj
        avatar: obj
        password: obj
    }

// TODO : Domain object string opts !
let private norm (x: string) = if isNull x then null else x.ToLower();

let private mapPayload (user: RegisterOrgInfoDTO): RegisterOrgInfo  = 
    {
        User = {
            Name = {
                FirstName = user.firstName :?> string |> norm
                LastName = user.lastName :?> string |> norm
                MiddleName = user.middleName :?> string |> norm      
            }
            Phone = user.phone :?> string |> norm
            Email = user.email :?> string |> norm
            Avatar = user.avatar :?> string |> norm
        }
        Org = {
            Name = user.orgName :?> string
        }
        Password = user.password :?> string
    }

let fromPayload = 
    [
        "firstName", fun x -> x.firstName |> isMidStr 
        "lastName", fun x ->  x.lastName |> isMidStr 
        "middleName", fun x -> lift2(<|>) isNull' isMidStr x.middleName
        "orgName", fun x ->  x.orgName |> isMidStr
        "email", fun x ->  x.email |> isEmail
        "phone", fun x ->  x.phone |> isPhone
        "avatar", fun x ->  lift2(<|>) isNull' isUrl x.avatar
        "password", fun x ->  x.password |> isPassword
    ]
    |> validatePayload mapPayload

open ReaderTask

let registerOrgFromBody :(string -> API<_>) = 
    fromPayload 
    >> Result.mapError validationException 
    >> ReaderTask.ofResult 
    >=> RegisterOrg.registerOrg


