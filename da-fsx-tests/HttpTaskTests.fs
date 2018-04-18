module HttpTaskTests

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FSharpx.Task

open System.Net
open DA.FSX.HttpTask
open WebClient

let webClient = new WebClient()

type ResponseHeaders = {
    test: string
}

type Response = {
    url: string
    headers: ResponseHeaders
}

let ofRequest' = ofRequest webClient
let ofRequest<'a> r = str2json<Response> <!> ofRequest' r

[<Fact>]
let ``Get must work`` () =
    
    let request = {
        url = "https://httpbin.org/get"
        httpMethod = GET
        headers = []
        payload = None
    } 

    ofRequest' request

[<Fact>]
let ``Get with parse response must work`` () =
    
    let request = {
        url = "https://httpbin.org/get"
        httpMethod = GET
        headers = []
        payload = None
    } 

    (fun resp -> resp.url |> should equal "https://httpbin.org/get") <!> ofRequest request     
    

[<Fact>]
let ``Get with headers must work`` () =
    
    let request = {
        url = "https://httpbin.org/get"
        httpMethod = GET
        headers = [ "test", "123" ]
        payload = None
    } 

    (fun resp -> resp.headers.test |> should equal "123") <!> ofRequest request     
    
