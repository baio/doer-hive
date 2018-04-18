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

type Response = {
    url: string
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
    
