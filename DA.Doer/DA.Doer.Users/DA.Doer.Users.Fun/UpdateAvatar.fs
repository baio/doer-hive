namespace DA.Doer.Users.Fun

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


    [<FunctionName("updateAvatar")>]
    let run(
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "update-avatar")>]
            request: HttpRequest,
            log: ILogger
        ) =            
            updateAvatar (request.Headers.Item("Authorization").[0]) request.Body
            |> map result200
            |> bindError (getHttpError >> mapResultStr >> returnM)
            <| context2

