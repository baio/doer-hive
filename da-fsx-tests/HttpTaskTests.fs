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

let runRequest = runRequest webClient
let chainWebClient = chainWebClient webClient

[<Fact>]
let ``Get must work`` () =
    
    let request = {
        url = "https://httpbin.org/get"
        httpMethod = GET
        headers = []
        payload = None
    } 

    runRequest request
