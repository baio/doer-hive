module DA.FSX.HttpTask

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

let inline str2json<'a> str = JsonConvert.DeserializeObject<'a> str

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
    
    let private bytes2str (xs: byte[]) = System.Text.Encoding.UTF8.GetString(xs)

    let private seq2coll'<'a when 'a :> NameValueCollection> (xs: 'a) (s: seq<string * string>) = 
        for (x, y) in s do xs.Set(x, y)        
        xs 

    let private seq2coll (s: seq<string * string>) = 
        seq2coll' (new System.Collections.Specialized.NameValueCollection()) s
    
    let chainWebClient (webClient: WebClient) (request: Request) : Task<string> =         
        webClient.Headers <- seq2coll' (new WebHeaderCollection()) request.headers
        webClient.QueryString <- seq2coll request.queryString
        match request.httpMethod with
        | GET | DELETE ->
            webClient.DownloadStringTaskAsync request.url
        | PUT | POST | PATCH ->
            match request.payload with
            | FormPayload payload ->
                payload 
                |> seq2coll 
                |> fun x -> webClient.UploadValuesTaskAsync(request.url, x)
                |> map(bytes2str)
            | _ -> failwith "Not impl"
                
    let inline ofRequest (webClient: WebClient) = returnM >=> chainWebClient webClient       
    