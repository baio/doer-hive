module DA.Http.HttpTask.HttpClient

open DA.FSX.Task
open DA.FSX.HttpTask
open System.Net.Http
open System.Threading.Tasks
open System.Text
open Newtonsoft.Json

let private getHttpMethod (method: DA.FSX.HttpTask.HttpMethod) = 
    match method with
    | GET -> HttpMethod.Get
    | POST -> HttpMethod.Post
    | PUT -> HttpMethod.Put    
    | DELETE -> HttpMethod.Delete
    | PATCH -> new HttpMethod("PATCH")

let private  getUrl (request: Request) =
    if (Seq.length request.QueryString) = 0 then
        request.Url
    else 
        request.Url + "?" + 
            (request.QueryString |> Seq.map(fun (k, v) -> k + "=" + v) |> String.concat "&")

let private getFormBody (x: (string * string) list) =  
    new FormUrlEncodedContent(x 
        |> List.map(fun (k, v) -> System.Collections.Generic.KeyValuePair(k, v))
    )
    
let private getJsonBody x =  
    new StringContent(JsonConvert.SerializeObject(x), Encoding.UTF8, "application/json")

let private getFormMultipartBody (streams, data) =  
    let requestContent = new MultipartFormDataContent() 
    streams |> List.iter(fun (name, stream) ->
        let imageContent = new StreamContent(stream)
        requestContent.Add(imageContent, name, name) 
    )
    data |> List.iter(fun (name, x) ->
        let strContent = new StringContent(x)
        requestContent.Add(strContent, name) 
    )
    //let formData = getFormBody data
    //requestContent.Add(formData)
    requestContent
            
let private getRequestMessage (request: Request): HttpRequestMessage = 
    let url = getUrl request
    let method = getHttpMethod request.HttpMethod
    let m = new HttpRequestMessage(method, url)
    request.Headers |> Seq.iter(fun (k, v) -> m.Headers.Add(k, v))
    match request.HttpMethod with
    | PUT | POST | PATCH -> 
        match request.Payload with
        | FormPayload x -> 
            m.Content <- getFormBody(x)
        | JsonPayload x -> 
            m.Content <- getJsonBody(x)
        | FormMultipartPayload (x, y) -> 
            m.Content <- getFormMultipartBody(x, y)
        | None ->             
            ()
    | _ -> ()
    m

let private convertToHttpException (x: HttpResponseMessage) =
    x.Content.ReadAsStringAsync()
    |> map (fun content ->
        {
            StatusCode =    x.StatusCode
            Reason     =    x.ReasonPhrase
            Content    =    content
            Uri        =    x.RequestMessage.RequestUri
            Method     =    x.RequestMessage.Method
        }
    )
    |> map HttpException

                
let chainHttpRequest (httpClient: HttpClient) (request: Request) : Task<string> =         
    request 
    |> getRequestMessage 
    |> fun x -> httpClient.SendAsync(x) 
    >>= (fun x -> 
        match x.IsSuccessStatusCode with
        | true  ->
            x.Content.ReadAsStringAsync()
        | false ->
            x |> convertToHttpException >>= ofException
    )

let httpClientRequest = chainHttpRequest (new HttpClient())
