[<AutoOpen>]
module DA.Auth0.API

open DA.Auth0
open DA.FSX
open System.Threading.Tasks

open DA.FSX.ReaderTask
open DA.FSX.Reader
open FSharpx.Reader
open DA.FSX.HttpTask
open DA.Auth0.RequestAPI
open DA.Auth.Domain

type HttpRequest = Request -> Task<string>

type Auth0Api = {
    Request: HttpRequest
    Config : Auth0Config
}

type API<'a> = ReaderTask<Auth0Api, 'a>

// Response DTOs

type UserIdResponse = { user_id : string }

type LoginResponse = {
    id_token: string
    access_token: string
    refresh_token: string
}

type TokensResponse = {
    id_token: string
    access_token: string
    refresh_token: string
}

type ManagementTokenResponse = {
    token_type: string
    access_token: string
}

// Domain 

type FoldRequest<'a> = (HttpRequest -> ReaderTask<Auth0Config, 'a>) -> ReaderTask<Auth0Api, 'a>
let fold: FoldRequest<_> = fun f api -> f api.Request api.Config

let mapResponse f x = ReaderTask.map(str2json >> f) x

// get management token

let mapManagementTokenResponse = mapResponse(fun x -> 
        x.token_type + " " + x.access_token
    )

let managementToken' (f: HttpRequest) = (f <!> getManagementToken) |> mapManagementTokenResponse

let managementToken = managementToken' |> fold
        
let managementTokenMem: API<string> = Task.memoize(managementToken)

//

let mapUserIdResponse = mapResponse(fun x -> x.user_id)

let inline withToken f = managementTokenMem |> ReaderTask.bind f

// create user

let createUser'' userInfo token (f: HttpRequest) = (f <!> createUser userInfo token) |> mapUserIdResponse

let createUser' userInfo token = createUser'' userInfo token |> fold

let createUser = createUser' >> withToken 

// update user avatar

let updateUserAvatar'' userInfo token (f: HttpRequest) = (f <!> updateUserAvatar userInfo token) |> mapUserIdResponse

let updateUserAvatar' userInfo token = updateUserAvatar'' userInfo token |> fold

let updateUserAvatar = updateUserAvatar' >> withToken 

// let createUser userInfo = getManagementToken |> ReaderTask.bind (createUserWithToken userInfo)
    
// get user (by user token ?)

let getUser' token (f: HttpRequest) = (f <!> getUser token) |> mapUserIdResponse

let getUser = getUser' >> fold
// login

let mapLoginResponse x = x |> mapResponse(fun x -> 
        {
            IdToken = x.id_token
            AccessToken = x.access_token
            RefreshToken = x.refresh_token
        }
        )

let login' loginInfo (f: HttpRequest) = (f <!> login loginInfo) |> mapLoginResponse

let login = login' >> fold

// remove user

let removeUser'' userId token (f: HttpRequest) = (f <!> removeUser token userId) |> mapResponse(fun _ -> true)

let removeUser' userId token = removeUser'' userId token |> fold

let removeUser = removeUser' >> withToken

// register user

let registerUser userInfo =    
    readerTask {
        let! userId = createUser userInfo
        let! tokens = login { Email = userInfo.Email; Password = userInfo.Password }
        return { UserId = userId; Tokens = tokens }
    }    

// refresh token

let refreshToken' token (f: HttpRequest) = (f <!> refreshToken token) |> mapLoginResponse

let refreshToken = refreshToken' >> fold