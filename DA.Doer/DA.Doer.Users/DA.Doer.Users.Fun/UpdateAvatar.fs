﻿namespace DA.Doer.Users.Fun

module UpdateAvatar =
    open FSharp.Data
    open System
    open Microsoft.AspNetCore.Mvc
    open Microsoft.AspNetCore.Http
    open Microsoft.Azure.WebJobs
    open Microsoft.Azure.WebJobs.Host
    open Microsoft.Azure.WebJobs.Extensions.Http
    open Microsoft.Extensions.Logging
    open DA.Doer.Users
    open DA.FSX.ReaderTask
    open DA.Doer.Users.UpdateAvatar
    open DA.Doer.Users.UpdateAvatar.Errors
    open Config
    open Newtonsoft.Json
    open DA.Http.ContentResult
    open DA.HTTP.Request


    [<FunctionName("update-avatar")>]
    let run(
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "update-avatar")>]
            request: HttpRequest,
            log: ILogger
        ) =            
            let x = readAuthHeader' request
            let y = readFirstFileContent' request
            (updateAvatar <!> x <*> y) |> bind(fun x -> x)
            |> map result200
            |> bindError (getHttpError >> mapResultStr >> returnM)
            <| context2

            

