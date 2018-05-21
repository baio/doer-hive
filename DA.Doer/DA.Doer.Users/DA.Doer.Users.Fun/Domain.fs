[<AutoOpen>]
module DA.Doer.Users.Fun.Domain

open Microsoft.AspNetCore.Http
open DA.HTTP.Request
open DA.Doer.Users.Utils
open DA.FSX.Task
open DA.Http.ContentResult

let getPrincipal (request: HttpRequest) jwtConfig = 
    let authToken = readAuthHeader request
    getPrincipal authToken jwtConfig

let bindHttpError f = bindError (f >> mapResultStr >> returnM)