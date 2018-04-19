module DA.Auth0.API

open DA.FSX
open System.Threading.Tasks

open DA.FSX.ReaderTask
open DA.FSX.Reader
open FSharpx.Task
open FSharpx.Reader
open DA.FSX.HttpTask
open DA.Auth0.RequestAPI

type HttpRequest = Request -> Task<string>

// Response DTOs

type UserIdResponse = { user_id : string }

type TokensResponse = {
    id_token: string
    access_token: string
    refresh_token: string
}

// Domain 

type TokensResult = {
    idToken: string
    accessToken: string
    refreshToken: string
}

let mapResponse f x = ReaderTask.map(str2json >> f) x

//

let mapUserIdResponse = mapResponse(fun x -> x.user_id)

// create user

let createUser' userInfo (f: HttpRequest) = (f <!> createUser userInfo) |> mapUserIdResponse

let createUser = createUser' >> flat
   
// get user

let getUser token (f: HttpRequest) = (f <!> getUser token) |> mapUserIdResponse

// user tokens 

let mapTokensResponse x = x |> mapResponse(fun x -> 
    {
        idToken = x.id_token
        accessToken = x.access_token
        refreshToken = x.refresh_token
    })

let getUserTokens' token (f: HttpRequest) = (f <!> getUserTokens token) |> mapTokensResponse

let getUserTokens = getUserTokens' >> flat

// register user

//open ReaderTask

type RegisterUserResult = {
    userId: string
    tokens: TokensResult
}

let registerUser userInfo =    
    readerTask {
        let! userId = createUser userInfo
        let! tokens = getUserTokens userInfo
        return { userId = userId; tokens = tokens }
    }
    

