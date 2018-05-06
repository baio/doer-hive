namespace DA.Auth.Domain.Errors

open System.Net

type NetworkErrorResponse = {
    Uri   : System.Uri
}

type NetworkError = {
    Code       : HttpStatusCode
    Message    : string
    Response   : NetworkErrorResponse option
}

type UserAlreadyExists = UserAlreadyExists


