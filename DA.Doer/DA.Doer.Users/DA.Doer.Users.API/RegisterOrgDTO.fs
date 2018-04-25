module DA.Doer.Users.RegisterOrgDTO

open Newtonsoft.Json

open DA.Doer.Domain.Errors
open DA.Doer.Domain.Validators
open DA.FSX.ValidationResult
open DA.FSX


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
        passord: obj
    }

let private mapPayload (user: RegisterOrgInfoDTO): RegisterOrgInfo  = 
    {
        User = {
            Name = {
                FirstName = user.firstName :?> string
                LastName = user.lastName :?> string
                MiddleName = user.middleName :?> string       
            }
            Phone = user.phone :?> string
            Email = user.email :?> string
            Avatar = user.avatar :?> string
        }
        Org = {
            Name = user.orgName :?> string
        }
        Password = user.passord :?> string
    }

let fromPayload = 
    [
        "firstName", fun x -> x.firstName |> isMidStr 
        "lastName", fun x ->  x.lastName |> isMidStr 
        "middleName", fun x ->  x.middleName |> isMidStr
        "orgName", fun x ->  x.orgName |> isMidStr
        "email", fun x ->  x.email |> isEmail
        "phone", fun x ->  x.phone |> isPhone
        "avatar", fun x ->  x.avatar |> isUrl
        "password", fun x ->  x.passord |> isPassword
    ]
    |> validatePayload mapPayload

open ReaderTask

let registerOrg :(string -> API<_>) = 
    fromPayload 
    >> Result.mapError validationException 
    >> ReaderTask.ofResult 
    // >=> RegisterOrg.registerOrg


