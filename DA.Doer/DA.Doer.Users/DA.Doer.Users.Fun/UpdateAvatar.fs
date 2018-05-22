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
    open DA.FSX.Task
   
    open DA.Doer.Users.UpdateAvatar
    open DA.Doer.Users.UpdateAvatar.Errors
    open Config
    open Newtonsoft.Json
    open DA.Http.ContentResult
    open DA.HTTP.Request

    [<FunctionName("update-avatar")>]
    let run(
            [<HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "/users/avatar")>]
            request: HttpRequest,
            userId: string,
            log: ILogger
        ) =            
            let context = {
                Mongo = mongoApi
                Auth0 = auth0Api
                Blob = blobApi
            }
            task {                
                #if DEBUG_POSTMAN
                let! fileStream = readFirstFileContentDebug  request                
                #else
                let! fileStream = readFirstFileContent  request
                #endif                
                let! prinipal = getPrincipal request jwtConfig
                let! result   = updateAvatar prinipal fileStream context
                return result200 result
            }
            |> bindHttpError getHttpError

            

