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
    queryString: QueryString
}

let inline str2json<'a> str = JsonConvert.DeserializeObject<'a> str


module WebClient = 

    open System.Net
    
    let private webHeaders (headers: Headers) = 
        let webHeaders = new WebHeaderCollection()        
        for (x, y) in headers do webHeaders.Set(x, y)        
        webHeaders

    let private webQueryString (qs: QueryString) = 
        let webQs = new System.Collections.Specialized.NameValueCollection()
        for (x, y) in qs do webQs.Set(x, y)        
        webQs 
    
    let chainWebClient (webClient: WebClient) (request: Request) : Task<string> =         
        webClient.Headers <- webHeaders request.headers
        webClient.QueryString <- webQueryString request.queryString
        webClient.DownloadStringTaskAsync request.url
                
    let inline ofRequest (webClient: WebClient) = returnM >=> chainWebClient webClient       
    