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
    f (Uri request.Path.Value) (getMethod request.Method)
    |> HttpException 


let fileContentException uri method =
    {
        StatusCode =    System.Net.HttpStatusCode.BadRequest
        Reason     =    "File content not present"
        Content    =    "File content not present, provide one as multipart form"
        Uri        =    uri
        Method     =    method
    
    }

let readFirstFileContent (request: HttpRequest) = 
    request.ReadFormAsync()
    |> map (fun parts -> 
        if parts.Count = 1 then
            parts.Files.[0].OpenReadStream()
        else 
            wrapError fileContentException request |> raise
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
        request.Headers.Item("Authorization").Item(0) |> Ok
    else 
        wrapError authorizationHeaderNotFoundException request |> Error


// Utility, since all of the time these functions will be part of the TaskReader flow

let readAuthHeader'       x = x |> readAuthHeader |> ReaderTask.ofResult

let readFirstFileContent' x = x |> readFirstFileContent |> ReaderTask.ofTask