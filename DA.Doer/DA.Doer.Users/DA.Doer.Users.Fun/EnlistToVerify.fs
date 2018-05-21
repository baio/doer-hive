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

(*
This function use batch flow to add photos to training service.
It expects some user photos already uploaded in storage and then use them to send to training service 
and potentiallly train. Face plus plus works differently, it provides api to add photos sequentially
which is better since we could validate photos before add them to user profile photos.
See function `enlist-photo`
*)
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
            let! principal = getPrincipal request jwtConfig
            let! result   = enlistToVerify principal userId context
            return result200 result
        }
        |> bindHttpError getHttpError

            

