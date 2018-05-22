module DA.Doer.Users.Fun.IdentifyPhoto

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
   
open DA.Doer.Users.IdentifyPhoto
open DA.Doer.Users.IdentifyPhoto.Errors
open Newtonsoft.Json
open DA.Http.ContentResult
open DA.HTTP.Request
open Config 

(*
    Identify photo using face++ service.
*)
[<FunctionName("identify-photo")>]
let run(
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/identify-photo")>]
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
            let! principal = getPrincipal request jwtConfig
            #if DEBUG_POSTMAN
            let! fileStream = readFirstFileContentDebug  request                
            #else
            let! fileStream = readFirstFileContent  request
            #endif                
            let! result   = identifyPhoto principal fileStream context
            return result200 result
        }
        |> bindHttpError getHttpError

            

