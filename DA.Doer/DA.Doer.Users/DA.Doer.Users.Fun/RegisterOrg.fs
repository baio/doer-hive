namespace DA.Doer.Users.Fun

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.Extensions.Logging
open DA.Doer.Users

open DA.FSX
open DA.FSX.ReaderTask
open DA.Doer.Users.RegisterOrgDTO
open DA.Doer.Users.RegisterOrg
open Config

module Main =

    [<FunctionName("register-org")>]
    let run(
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register-org")>]
        request: HttpRequest,
        log: ILogger
        ) =
            let task = 
                request.Body 
                |> IO.readString 
                |> ofTask 
                >>= registerOrgDTO 
                |> map (fun x ->
                    ContentResult(Content = x.userId, ContentType = "text/html")
                )
            task context

