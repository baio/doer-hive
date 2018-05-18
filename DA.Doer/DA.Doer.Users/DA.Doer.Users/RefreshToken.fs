module DA.Doer.Users.RefreshToken


open DA.Doer.Users
open DA.Doer.Mongo
open DA.Auth0
open DA.Doer.Users.RefreshTokenDTO
open DA.FSX.ReaderTask

let request = DA.Http.HttpTask.HttpClient.httpClientRequest

// collide the worlds!

let getAuth config = {
    RefreshToken = fun token -> refreshToken token config
}

let mapContext = fun authConfig -> getAuth authConfig

let refreshTokenFromBody payload = mapContext >> refreshTokenFromBody payload

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
        
