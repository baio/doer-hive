namespace DA.Doer.Users.Fun


module Main =
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
    open DA.Doer.Users.RegisterOrg
    open DA.Doer.Users.RegisterOrg.Errors
    open Config
    open Newtonsoft.Json
    open DA.Http.ContentResult


    [<FunctionName("register-org")>]
    let run(
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register-org")>]
            request: HttpRequest,
            log: ILogger
        ) =

            request.Body
            |> ofStream
            >>= registerOrgFromBody
            |> map result201
            |> bindError (getHttpError >> mapResultStr >> returnM)
            <| context

