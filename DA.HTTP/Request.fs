module DA.HTTP.Request

open DA.FSX.Task
open Microsoft.AspNetCore.Http
open DA.FSX.HttpTask
open System.Net.Http
open System
open DA.FSX


let private getMethod = function 
    | "GET" -> HttpMethod.Get 
    | "POST" -> HttpMethod.Post 
    | "PUT" -> HttpMethod.Put
    | "PATCH" -> HttpMethod("Patch")
    | "DELETE" -> HttpMethod.Delete
    | _ as x -> HttpMethod(x)

let wrapError f (request: HttpRequest) = 
    f (Uri (request.Host.Value + request.Path.Value)) (getMethod request.Method)
    |> HttpException 


let fileContentException uri method =
    {
        StatusCode =    System.Net.HttpStatusCode.BadRequest
        Reason     =    "File content not present as multipart form"
        Content    =    "File content not present as multipart form, provide one as multipart form"
        Uri        =    uri
        Method     =    method
    
    }

let readFirstFileContent (request: HttpRequest) = 
    request.ReadFormAsync()
    |> bindError(fun e -> wrapError fileContentException request |> ofException)
    |> map (fun parts -> 
        if parts.Count = 0 then
            wrapError fileContentException request |> raise            
        else 
            parts.Files.[0].OpenReadStream()
    )


let authorizationHeaderNotFoundException uri method =
    {
        StatusCode =    System.Net.HttpStatusCode.Forbidden
        Reason     =    "Authorization header is not provided"
        Content    =    "Authorization header is not provided, provide one as JWT Bearer token"
        Uri        =    uri
        Method     =    method
    
    }

let readAuthHeader (request: HttpRequest) = 
    if request.Headers.Item("Authorization").Count = 1 then
        request.Headers.Item("Authorization").Item(0)
    else 
        wrapError authorizationHeaderNotFoundException request |> raise


// Utility, since all of the time these functions will be part of the TaskReader flow

let readAuthHeader'       x = x |> readAuthHeader |> ReaderTask.ofTask

let readFirstFileContent' x = x |> readFirstFileContent |> ReaderTask.ofTask

// Quick fallback to override POSTMAN bug: https://github.com/postmanlabs/postman-app-support/issues/576            
let readFirstFileContentDebug (x: HttpRequest) = x.Body