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

let mapPayload'<'a, 'b> (f: 'a -> 'b) validators payload = 
    let x = JsonConvert.DeserializeObject<'a>(payload)
    (fun _ -> f x) <!> 
        (
            (validators |> List.map(fun (a, b) -> x |> b  |> mf a))
            |> sequence
        )


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
    |> mapPayload' mapPayload


    (*
    let x = JsonConvert.DeserializeObject<UserOrgDTO>(payload)
    (fun _ -> mapPayload x) <!> 
        (
            [
                x.firstName |> isMidStr |> mf "firstName"
                x.lastName |> isMidStr |> mf "lastName"
                x.middleName |> isMidStr |> mf "middleName"
                x.orgName |> isMidStr |> mf "orgName"
                x.email |> isEmail |> mf "email"
                x.phone |> isPhone |> mf "phone"
                x.avatar |> isUrl |> mf "avatar"
                x.passord |> isPassword |> mf "password"
            ] 
            |> sequence
        )
    *)

open ReaderTask

let registerOrg x = 
    x 
    |> fromPayload 
    |> Result.mapError validationException 
    |> ReaderTask.ofResult
    >>= RegisterOrg.registerOrg
