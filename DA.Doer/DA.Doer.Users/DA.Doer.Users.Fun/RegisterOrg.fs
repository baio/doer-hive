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

    let contentResult (code, content) =
        ContentResult(
            Content = content,
            StatusCode = Nullable<int>(code),
            ContentType = HttpContentTypes.Json
       )

    let mapSuccess x = x |> JsonConvert.SerializeObject |> fun x -> contentResult (StatusCodes.Status201Created, x)

    let mapError x = x |> getHttpError |> contentResult

    [<FunctionName("register-org")>]
    let run(
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register-org")>]
            request: HttpRequest,
            log: ILogger
        ) =

            request.Body
            |> ofStream
            >>= registerOrgFromBody
            |> map mapSuccess
            |> bindError (mapError >> returnM)
            <| context

