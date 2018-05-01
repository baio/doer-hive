module DA.Doer.Users.RefreshTokenDTO

open DA.Doer.Domain.Errors
open DA.Doer.Domain.Validators
open DA.FSX.ValidationResult
open DA.FSX
open FSharpx.Reader
open DA.Auth.Domain

type RefreshTokenDTO =
    { 
        token: obj
    }



let fromPayload = 
    [
        "token", fun x -> x.token |> notEmptyString
    ]
    |> validatePayload (fun x -> x.token :?> string)

open ReaderTask

let refreshTokenFromBody :(string -> API<_>) = 
    fromPayload 
    >> Result.mapError validationException 
    >> ReaderTask.ofResult 
    >=> RefreshToken.refreshToken


