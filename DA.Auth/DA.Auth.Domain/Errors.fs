namespace DA.Auth.Domain.Errors

open System.Net

type NetworkError = {
    message: string
    response: WebResponse option
}

type UserAlreadyExists = UserAlreadyExists


