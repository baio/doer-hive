module DA.Http.ContentResult

open Microsoft.AspNetCore.Mvc
open System
open FSharp.Data
open Microsoft.AspNetCore.Http
open Newtonsoft.Json

let contentResult (code, content) =
    ContentResult(
        Content = content,
        StatusCode = Nullable<int>(code),
        ContentType = HttpContentTypes.Json
    )

let mapResultJson (code, a) = 
    a |> JsonConvert.SerializeObject |> fun x -> contentResult (code, x)

let mapResultStr (code, a) = contentResult (code, a)

