namespace DA.Auth0

type Auth0Config = {
    clientDomain: string
    clientId: string
    clientSecret: string
    audience: string
}


module Errors = 
    open System
    open DA.Auth.Domain.Errors

    let matchUserAlreadyExistsError (x: exn) =
        match x with
        | :? System.Net.WebException as ex when (ex.Response :?> System.Net.HttpWebResponse).StatusCode = Net.HttpStatusCode.Conflict && ex.Response.ResponseUri.Host.IndexOf(".auth0.") <> -1 ->
            Some UserAlreadyExists
        | _ ->
            None
