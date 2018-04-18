module HttpTaskTests

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FSharpx.Task

open System.Net
open DA.FSX.HttpTask

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

let webClient = new WebClient()
let ofRequest' = WebClient.ofRequest webClient
let ofRequest r = str2json<Response> <!> ofRequest' r
let ofGet = fromGetLike >> ofRequest
let ofPost = fromPostLike >> ofRequest

[<Fact>]
let ``Get must work`` () =
    
    let request = {
        url = "https://httpbin.org/get"
        httpMethod = GET
        headers = []
        queryString = []
        payload = None
    } 

    ofRequest' request

[<Fact>]
let ``Get with parse response must work`` () =
    
    let request = {
        url = "https://httpbin.org/get"
        httpMethod = GET
        headers = []
        queryString = []
        payload = None
    } 

    (fun resp -> resp.url |> should equal "https://httpbin.org/get") <!> ofRequest request     
    

[<Fact>]
let ``Get with headers must work`` () =
    
    let request = {
        url = "https://httpbin.org/get"
        httpMethod = GET
        queryString = []
        headers = [ "test", "123" ]
        payload = None
    } 

    (fun resp -> resp.headers.test |> should equal "123") <!> ofRequest request     
    
[<Fact>]
let ``Get with query string must work`` () =
    
    let request = {
        url = "https://httpbin.org/get"
        httpMethod = HttpGetLikeMethod.GET
        queryString = [ "q", "1" ]
        headers = [ ]
    } 

    (fun resp -> resp.args.q |> should equal "1") <!> ofGet request     
    

    
[<Fact>]
let ``Post with form-value payload must work`` () =
    
    let request = {
        url = "https://httpbin.org/post"
        httpMethod = HttpPostLikeMethod.POST
        headers = []
        payload = FormPayload [ "x", "100" ] 
    } 

    (fun resp -> resp.form.x |> should equal "100") <!> ofPost request     

[<Fact>]
let ``Post with json payload must work`` () =
    
    let request = {
        url = "https://httpbin.org/post"
        httpMethod = HttpPostLikeMethod.POST
        headers = []
        payload = JsonPayload { lol = true }
    } 

    (fun resp -> resp.json.lol |> should equal true) <!> ofPost request     
