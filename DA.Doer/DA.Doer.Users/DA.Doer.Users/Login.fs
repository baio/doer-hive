module DA.Doer.Users.Login


open DA.Doer.Users
open DA.Doer.Mongo
open DA.Auth0
open DA.Doer.Users.LoginDTO
open DA.FSX.ReaderTask

let request = DA.Http.HttpTask.HttpClient.httpClientRequest

// collide the worlds!

let getAuth config = {
    login = fun loginInfo -> login loginInfo config
}

let mapContext = fun authConfig -> getAuth authConfig

let loginFromBody payload = mapContext >> loginFromBody payload

module Errors =
    
    open DA.Doer.Users.Errors
    open DA.Doer.Domain.Errors    
    
    let getHttpError (ex: exn) =  
        [
            matchValidationError >> (Option.map validation)
            unexepcted >> Some
        ] 
        |> List.choose(fun x -> x ex)
        |> List.head
        
