namespace DA.Doer.Users.Fun


module Login =
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
    open DA.Doer.Users.Login
    open DA.Doer.Users.Login.Errors
    open Config
    open Newtonsoft.Json
    open DA.Http.ContentResult


    [<FunctionName("login")>]
    let run(
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")>]
            request: HttpRequest,
            log: ILogger
        ) =

            request.Body
            |> ofStream
            >>= loginFromBody
            |> map result200
            |> bindError (getHttpError >> mapResultStr >> returnM)
            <| (context |> snd)

