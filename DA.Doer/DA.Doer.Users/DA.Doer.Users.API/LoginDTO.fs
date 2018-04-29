module DA.Doer.Users.LoginDTO

open DA.Doer.Domain.Errors
open DA.Doer.Domain.Validators
open DA.FSX.ValidationResult
open DA.FSX
open FSharpx.Reader
open DA.Auth.Domain

type LoginInfoDTO =
    { 
        email: obj
        password: obj
    }

// TODO : Domain object string opts !
let private norm (x: string) = if isNull x then null else x.ToLower();

let private mapPayload (info: LoginInfoDTO): LoginInfo  = 
    {
        Email = info.email :?> string |> norm
        Password = info.password :?> string
    }

let fromPayload = 
    [
        "email", fun x -> x.email |> isEmail
        "password", fun x ->  x.password |> isPassword
    ]
    |> validatePayload mapPayload

open ReaderTask

let loginFromBody :(string -> API<_>) = 
    fromPayload 
    >> Result.mapError validationException 
    >> ReaderTask.ofResult 
    >=> Login.login


