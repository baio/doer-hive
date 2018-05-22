module DA.Doer.Users.Fun.EnlistPhoto

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
   
open DA.Doer.Users.EnlistPhoto
open DA.Doer.Users.EnlistPhoto.Errors
open Config
open Newtonsoft.Json
open DA.Http.ContentResult
open DA.HTTP.Request

(*
    Validate photo using face++ service.
    If foto validated (single person found on image) add foto to storage and enlist it for verification
*)
[<FunctionName("enlist-photo")>]
let run(
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/{userId:guid}/enlist-photo")>]
        request: HttpRequest,
        userId : Guid,
        log: ILogger
    ) =            
        let context = {
            Mongo = mongoApi
            Blob = blobApi
            FacePP = faceppApi
        }
        task {                
            let! principal = getPrincipal request jwtConfig
            #if DEBUG_POSTMAN
            let! fileStream = readFirstFileContentDebug  request                
            #else
            let! fileStream = readFirstFileContent  request
            #endif                
            let! result   = enlistPhoto principal (userId.ToString()) fileStream context
            return result200 result
        }
        |> bindHttpError getHttpError

            

