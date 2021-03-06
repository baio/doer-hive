﻿namespace DA.Auth0

type Auth0Config = {
    ClientDomain: string
    ClientId: string
    ClientSecret: string
    Audience: string
}


module Errors = 

    open System
    open DA.Auth.Domain.Errors    

    let matchUserAlreadyExistsError (x: exn) =
        match x with
        | :? DA.FSX.HttpTask.HttpException as ex when 
            ex.ExceptionData.StatusCode = Net.HttpStatusCode.Conflict && ex.ExceptionData.Uri.Host.IndexOf(".auth0.") <> -1 ->
                Some UserAlreadyExists
        | _ ->
            None


    let matchNetworkException (x: exn) =
        match x with
        | :? System.Net.Http.HttpRequestException ->
            Some { Message = x.Message; Response = None; Code = System.Net.HttpStatusCode.InternalServerError }
        | _ ->
            None

    let matchRequestException (x: exn) =
        match x with
        | :? DA.FSX.HttpTask.HttpException as ex ->
            Some { Message = ex.Message; Code = ex.ExceptionData.StatusCode; Response = Some { Uri = ex.ExceptionData.Uri } }
        | _ ->
            None
