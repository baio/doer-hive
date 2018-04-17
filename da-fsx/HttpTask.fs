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

type HttpMethod = POST | PUT | PATCH | DELETE | GET 

type Request = {
    url: Url
    httpMethod: HttpMethod
    payload: Payload 
    headers: Headers
}

let inline str2json'<'a> str = JsonConvert.DeserializeObject<'a> str

let inline str2json (t: Task<_>) = str2json' <!> t



(*

type RequestApiRequest = Request -> Task<string>

type RequestApi = {
    request : RequestApiRequest
}

module HttpTask = 
    
    open ReaderTask

    open Newtonsoft.Json

    type Http<'T> = ReaderTask<RequestApi, 'T>
    
    let request<'T> (request: Request) : Http<'T> = 
        JsonConvert.DeserializeObject<'T> <!> (fun ({ request = req }) -> req request)
        
    let post2<'T> (url: Url) (headers: Headers) (payload: Payload) : Http<'T> = 
        request<'T>({url = url; headers = headers; httpMethod = POST; payload = payload})
   
    let post<'T> (url: Url) (payload: Payload) : Http<'T> = 
        post2<'T> url [] payload

    let delete<'T> (url: Url) (headers: Headers) (payload: Payload) : Http<'T> = 
        request<'T>({url = url; headers = headers; httpMethod = DELETE; payload = payload})

    let get<'T> (url: Url) (headers: Headers) : Http<'T> = 
        request<'T>({url = url; headers = headers; httpMethod = GET; payload = None})
*)

module WebClient = 

    open System.Net
    
    let chainWebClient (webClient: WebClient) (request: Request) : Task<string> =         
        webClient.DownloadStringTaskAsync request.url
                
    let inline runRequest (webClient: WebClient) = returnM >=> chainWebClient webClient       
        