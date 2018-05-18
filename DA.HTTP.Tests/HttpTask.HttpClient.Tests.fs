module HttpTask.HttpClient.Tests

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FSharpx.Task

open System.Net
open DA.FSX.HttpTask
open DA.Http.HttpTask.HttpClient

type RequestPayload = {
    lol: Boolean
}

type ResponseHeaders = {
    test: string
}

type ResponseQs = {
    q: string
}

type ResponseForm = {
    x: string
}

type ResponseData = {
    x: string
}

type Response = {
    url: string
    headers: ResponseHeaders
    args: ResponseQs
    form: ResponseForm
    json: RequestPayload
}

let client' = httpClientRequest
let client r = str2json<Response> <!> client' r
let ofGet = fromGetLike >> client
let ofPost = fromPostLike >> client

[<Fact>]
let ``Get must work`` () =
    
    let request = {
        Url = "https://httpbin.org/get"
        HttpMethod = GET
        Headers = []
        QueryString = []
        Payload = None
    } 

    client' request

[<Fact>]
let ``Get with parse response must work`` () =
    
    let request = {
        Url = "https://httpbin.org/get"
        HttpMethod = GET
        Headers = []
        QueryString = []
        Payload = None
    } 

    (fun resp -> resp.url |> should equal "https://httpbin.org/get") <!> (client request)
    

[<Fact>]
let ``Get with headers must work`` () =
    
    let request = {
        Url = "https://httpbin.org/get"
        HttpMethod = GET
        QueryString = []
        Headers = [ "test", "123" ]
        Payload = None
    } 

    (fun resp -> resp.headers.test |> should equal "123") <!> (client request)     
    
[<Fact>]
let ``Get with query string must work`` () =
    
    let request = {
        Url = "https://httpbin.org/get"
        HttpMethod = HttpGetLikeMethod.GET
        QueryString = [ "q", "1" ]
        Headers = [ ]
    } 

    (fun resp -> resp.args.q |> should equal "1") <!> (ofGet request)
    

    
[<Fact>]
let ``Post with form-value payload must work`` () =
    
    let request = {
        Url = "https://httpbin.org/post"
        HttpMethod = HttpPostLikeMethod.POST
        Headers = []
        Payload = FormPayload [ "x", "100" ] 
    } 

    (fun resp -> resp.form.x |> should equal "100") <!> (ofPost request)

[<Fact>]
let ``Post with json payload must work`` () =
    
    let request = {
        Url = "https://httpbin.org/post"
        HttpMethod = HttpPostLikeMethod.POST
        Headers = []
        Payload = JsonPayload { lol = true }
    } 

    (fun resp -> resp.json.lol |> should equal true) <!> (ofPost request)     
