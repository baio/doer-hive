module DA.Doer.Users.Fun.EnlistToVerify

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
   
open DA.Doer.Users.API.EnlistToVerify
open DA.Doer.Users.EnlistToVerify
open DA.Doer.Users.EnlistToVerify.Errors
open Config
open Newtonsoft.Json
open DA.Http.ContentResult
open DA.HTTP.Request

[<FunctionName("enlist-to-verify")>]
let run(
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/{userId:guid}/enlist-to-verify")>]
        request: HttpRequest,
        userId : string,
        log: ILogger
    ) =            
        let context = {
            Mongo = mongoApi
            Blob = blobApi
            FacePP = faceppApi
        }
        task {                
            let authToken = readAuthHeader request
            let! prinipal = getPrincipal authToken jwtConfig
            let! result   = enlistToVerify prinipal userId context
            return result200 result
        }
        |> bindError (getHttpError >> mapResultStr >> returnM)

            

