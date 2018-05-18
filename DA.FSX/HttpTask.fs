namespace DA.FSX.HttpTask

open System.Threading.Tasks
open FSharpx.Task
open Newtonsoft.Json
open System.IO

type Url = string
type Headers = seq<string * string>
type QueryString = seq<string * string>

type Payload = 
    FormPayload of list<string * string> 
    | FormMultipartPayload of (list<string * Stream>) * (list<string * string>)
    | JsonPayload of obj
    | None

type HttpGetLikeMethod = DELETE | GET 

type HttpPostLikeMethod = POST | PUT | PATCH

type HttpMethod = POST | PUT | PATCH | DELETE | GET 

type GetLikeRequest = {
    Url: Url
    HttpMethod: HttpGetLikeMethod 
    Headers: Headers
    QueryString: QueryString
}

type PostLikeRequest = {
    Url: Url
    HttpMethod: HttpPostLikeMethod 
    Headers: Headers
    Payload: Payload 
}

type Request = {
    Url: Url
    HttpMethod: HttpMethod
    Payload: Payload 
    Headers: Headers
    QueryString: QueryString
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
        Url = r.Url
        HttpMethod = match r.HttpMethod with HttpGetLikeMethod.GET -> GET | HttpGetLikeMethod.DELETE -> DELETE
        Payload = None
        Headers = r.Headers
        QueryString = r.QueryString
    }

    let inline fromPostLike (r: PostLikeRequest) = {
        Url = r.Url
        HttpMethod = match r.HttpMethod with HttpPostLikeMethod.POST -> POST | HttpPostLikeMethod.PUT -> PUT | HttpPostLikeMethod.PATCH -> PATCH
        Payload = r.Payload
        Headers = r.Headers
        QueryString = []
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
        bytes2str <!> webClient.UploadValuesTaskAsync(request.Url, getMethod(request.HttpMethod), body) 

    let private uploadString (webClient: WebClient) request body =
        webClient.UploadStringTaskAsync(request.Url, getMethod(request.HttpMethod), body)

    let private upload (webClient: WebClient) request =
        let uploadValues = uploadValues webClient request
        let uploadString = uploadString webClient request
        match request.Payload with
        | JsonPayload x -> 
            webClient.Headers.Add("content-type", "application/json")
            x |> JsonConvert.SerializeObject |> uploadString
        | FormPayload x ->
            x |> seq2coll |> uploadValues
        | FormMultipartPayload _ ->
            failwith "Not implemented"                
        | None ->
            uploadValues (NameValueCollection())
    
    let chainWebClient (webClient: WebClient) (request: Request) : Task<string> =         
                
        webClient.Headers <- seq2coll' (WebHeaderCollection()) request.Headers
        webClient.QueryString <- seq2coll request.QueryString
        
        match request.HttpMethod with
        | GET ->
            webClient.DownloadStringTaskAsync request.Url
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
    