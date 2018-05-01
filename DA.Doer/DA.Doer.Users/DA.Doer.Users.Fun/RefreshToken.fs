namespace DA.Doer.Users.Fun


module RefreshToken =
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
    open DA.Doer.Users.RefreshToken
    open DA.Doer.Users.RefreshToken.Errors
    open Config
    open Newtonsoft.Json
    open DA.Http.ContentResult


    [<FunctionName("refresh-token")>]
    let run(
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "refresh-token")>]
            request: HttpRequest,
            log: ILogger
        ) =

            request.Body
            |> ofStream
            >>= refreshTokenFromBody
            |> map result200
            |> bindError (getHttpError >> mapResultStr >> returnM)
            <| (context |> snd)

