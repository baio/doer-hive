﻿namespace DA.FSX.HttpTask

open System.Threading.Tasks
open FSharpx.Task
open Newtonsoft.Json

type Url = string
type Headers = seq<string * string>
type QueryString = seq<string * string>

type Payload = 
    FormPayload of list<string * string> 
    | JsonPayload of obj
    | None

type HttpGetLikeMethod = DELETE | GET 

type HttpPostLikeMethod = POST | PUT | PATCH

type HttpMethod = POST | PUT | PATCH | DELETE | GET 

type GetLikeRequest = {
    url: Url
    httpMethod: HttpGetLikeMethod 
    headers: Headers
    queryString: QueryString
}

type PostLikeRequest = {
    url: Url
    httpMethod: HttpPostLikeMethod 
    headers: Headers
    payload: Payload 
}

type Request = {
    url: Url
    httpMethod: HttpMethod
    payload: Payload 
    headers: Headers
    queryString: QueryString
}

type HttpExceptionData = {
    StatusCode :    System.Net.HttpStatusCode
    Reason     :    string
    Content    :    string    
    Uri        :    System.Uri
    Method     :    System.Net.Http.HttpMethod
}

type HttpException(ExceptionData: HttpExceptionData) = 
    
    inherit System.Net.WebException(ExceptionData.Content)
    
    member this.ExceptionData = ExceptionData



[<AutoOpen>]
module Utils = 

    let inline str2json<'a> str = 
        JsonConvert.DeserializeObject<'a> str


    let inline fromGetLike (r: GetLikeRequest) = {
        url = r.url
        httpMethod = match r.httpMethod with HttpGetLikeMethod.GET -> GET | HttpGetLikeMethod.DELETE -> DELETE
        payload = None
        headers = r.headers
        queryString = r.queryString
    }

    let inline fromPostLike (r: PostLikeRequest) = {
        url = r.url
        httpMethod = match r.httpMethod with HttpPostLikeMethod.POST -> POST | HttpPostLikeMethod.PUT -> PUT | HttpPostLikeMethod.PATCH -> PATCH
        payload = r.payload
        headers = r.headers
        queryString = []
    }

module WebClient = 

    open System.Net
    open System.Collections.Specialized
    open DA.FSX
    open DA.FSX

    let private convertToHttpException (x: WebException) =
        x.Response.GetResponseStream()
        |> DA.FSX.IO.readString
        |> map (fun content ->
            {   
                // TODO !
                StatusCode =    System.Net.HttpStatusCode.InternalServerError
                Reason     =    x.Message
                Content    =    content
                Uri        =    x.Response.ResponseUri
                // TODO !
                Method     =    System.Net.Http.HttpMethod.Get
            }
        )
        |> map HttpException

    
    let private bytes2str (xs: byte[]) = System.Text.Encoding.UTF8.GetString(xs)

    let private seq2coll'<'a when 'a :> NameValueCollection> (xs: 'a) (s: seq<string * string>) = 
        for (x, y) in s do xs.Set(x, y)        
        xs 

    let private seq2coll (s: seq<string * string>) = 
        seq2coll' (NameValueCollection()) s
    
    let private getMethod = function GET -> "GET" | POST -> "POST" | PUT -> "PUT" | PATCH -> "PATCH" | DELETE -> "DELETE"

    let private uploadValues (webClient: WebClient) request body =
        bytes2str <!> webClient.UploadValuesTaskAsync(request.url, getMethod(request.httpMethod), body) 

    let private uploadString (webClient: WebClient) request body =
        webClient.UploadStringTaskAsync(request.url, getMethod(request.httpMethod), body)

    let private upload (webClient: WebClient) request =
        let uploadValues = uploadValues webClient request
        let uploadString = uploadString webClient request
        match request.payload with
        | JsonPayload x -> 
            webClient.Headers.Add("content-type", "application/json")
            x |> JsonConvert.SerializeObject |> uploadString
        | FormPayload x ->
            x |> seq2coll |> uploadValues
        | None ->
            uploadValues (NameValueCollection())
    
    let chainWebClient (webClient: WebClient) (request: Request) : Task<string> =         
                
        webClient.Headers <- seq2coll' (WebHeaderCollection()) request.headers
        webClient.QueryString <- seq2coll request.queryString
        
        match request.httpMethod with
        | GET ->
            webClient.DownloadStringTaskAsync request.url
        | DELETE ->
            uploadValues webClient request (NameValueCollection())
        | PUT | POST | PATCH ->
            upload webClient request
        |> Task.bindError(
            function 
            :? WebException as ex -> ex |> convertToHttpException >>= Task.ofException
            | _ as ex -> ex |> Task.ofException
         )
                
    let inline ofRequest (webClient: WebClient) = returnM >=> chainWebClient webClient       

    let webClientRequest = chainWebClient (new WebClient())
    