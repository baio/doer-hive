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
    open Config
    open Newtonsoft.Json
    open DA.Doer.Domain.Errors

    let mapValidationErrors errs =
        errs
        |> List.map(FSharpx.Reader.lift2(sprintf """ "%s" : "%s" """) fst snd)
        |> String.concat ","
        |> sprintf
                """
                    { "validationErrors" : { %s } }
                """

    let contentResult (code, content) =
        ContentResult(
            Content = content,
            StatusCode = Nullable<int>(code),
            ContentType = HttpContentTypes.Json
       )

    let mapSuccess x = x |> JsonConvert.SerializeObject |> fun x -> contentResult (StatusCodes.Status201Created, x)

    let mapError' (x: exn) =
        match x with
        | :? ValidationException as ex ->
            StatusCodes.Status400BadRequest, mapValidationErrors ex.Errors
        | :? System.Net.WebException as ex when (ex.Response :?> System.Net.HttpWebResponse).StatusCode = Net.HttpStatusCode.Conflict && ex.Response.ResponseUri.Host.IndexOf(".auth0.") <> -1 ->
            StatusCodes.Status409Conflict, """{ "userAlreadyExists" : true }"""
        | _ ->
            StatusCodes.Status500InternalServerError, ""

    let mapError x = x |> mapError' |> contentResult

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

