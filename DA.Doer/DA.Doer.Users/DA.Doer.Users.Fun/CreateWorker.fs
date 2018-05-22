module DA.Doer.Users.Fun.CreateWorker

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
   
open DA.Doer.Users.API.CreateWorker.Payload
open DA.Doer.Users.CreateWorker
open DA.Doer.Users.CreateWorker.Errors
open Newtonsoft.Json
open DA.Http.ContentResult
open DA.HTTP.Request
open Config 

(*
    Identify photo using face++ service.
*)
[<FunctionName("create-worker")>]
let run(
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "wrokers")>]
        request: HttpRequest,        
        log: ILogger
    ) =            
        let context = {
            Mongo = mongoApi
            Blob = blobApi
            FacePP = faceppApi
        }
        task {                
            let! principal  = getPrincipal request jwtConfig
            let! workerInfo = fromPayload request.Body
            let! result     = createWorker principal workerInfo context
            return result200 result
        }
        |> bindHttpError getHttpError

            

