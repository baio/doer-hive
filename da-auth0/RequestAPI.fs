module DA.Auth0.RequestAPI

open FSharpx.Task
open System.Threading.Tasks
open DA.FSX.HttpTask
open FSharpx.Reader


type Auth0Reader<'a> = Reader<Auth0Config, 'a>
type RequestAPI = Auth0Reader<Request>

// create user

type CreateUserInfo = {
    userId: string
    orgId: string
    name: string
    email: string
    password: string
    avatar: string
    role: string
}

type CreateUser = CreateUserInfo -> RequestAPI
let createUser: CreateUser = fun userInfo env -> 
    {
        httpMethod = POST
        url = sprintf "https://%s.auth0.com/api/v2/users" (env.clientDomain)
        payload = JsonPayload userInfo
        headers = []
        queryString = []
    }

// get user

type GetUser = string -> RequestAPI
let getUser: GetUser = fun token env -> 
    {
        httpMethod = GET
        url = sprintf "https://%s.auth0.com/api/v2/users?fields=user_id" (env.clientDomain)
        payload = None
        headers = ["authorization", token]
        queryString = []
    }

// get management token

let getManagementToken: RequestAPI = fun env -> 
    {
        httpMethod = POST
        url = sprintf "https://%s.auth0.com/oauth/token" env.clientDomain
        payload = FormPayload  [
                    "audience", sprintf "https://%s.auth0.com/api/v2/" env.clientDomain
                    "grant_type", "client_credentials"
                    "client_id", env.clientId
                    "client_secret", env.clientSecret
                ]
        headers = []
        queryString = []
    }

// remove user

type RemoveUser = string -> string -> RequestAPI
let removeUser: RemoveUser = fun token userId env -> 
    {
        httpMethod = DELETE
        url = sprintf "https://%s.auth0.com/api/v2/users/%s" env.clientDomain userId
        payload = None
        headers = ["authorization", token]
        queryString = []
    }

// user token

// https://auth0.com/docs/api/authentication#authorization-code
// https://auth0.com/docs/api-auth/tutorials/adoption/api-tokens
// https://auth0.com/docs/tokens/access-token
// If the audience is set to the unique identifier of a custom API, then the Access Token will be a JSON Web Token (JWT).
// https://auth0.com/docs/api-auth/tutorials/verify-access-token

// https://community.auth0.com/questions/4367/how-to-obtain-user-jwt-token-from-usernamepassword
// Cleint grant-type password must be on for the client
// https://auth0.com/docs/clients/client-grant-types

// In order ro retrieve user_metadata, use custom claims + rules
// https://auth0.com/docs/api-auth/tutorials/adoption/scope-custom-claims
// https://auth0.com/docs/rules/current
(*
    //Add attributes to the idToken	
    function (user, context, callback) {
        var namespace = 'https://doer.auth.com/';
        user.user_metadata = user.user_metadata || {};
        context.idToken[namespace + 'name'] = user.user_metadata.name;
        context.idToken[namespace + 'avatar'] = user.user_metadata.avatar;
        callback(null, user, context);
    }
*)
(*
    //Add attributes to the accessToken
    function (user, context, callback) {
        var namespace = 'https://doer.auth.com/';
        user.app_metadata = user.app_metadata || {};
        context.accessToken[namespace + 'orgId'] = user.app_metadata.orgId;
        context.accessToken[namespace + 'role'] = user.app_metadata.role;
        callback(null, user, context);
    }
*)

type GetUserTokens = CreateUserInfo -> RequestAPI
let getUserTokens: GetUserTokens = fun userInfo env -> 
    {
        httpMethod = POST
        url = sprintf "https://%s.auth0.com/oauth/token" env.clientDomain
        payload = FormPayload 
            [
                "username", userInfo.email
                "password", userInfo.password
                "grant_type", "password"
                "client_id", env.clientId
                "client_secret", env.clientSecret
                "audience", env.audience
                "scope", "openid profile"
            ]
        headers = []
        queryString = []
    }

// register user

(*
type Funx<'a> = Request -> System.Threading.Tasks.Task<'a>

// let getUser' (f: Funx<'a>) token = getUser token >>= (fun x _ -> f x)

type Extend<'a> = (Reader<Auth0Config, Request> -> Task<'a>) ->  Reader<Auth0Config, Request> -> Reader<Auth0Config, Task<'a>>
type Contramap<'a> = (Request -> Task<'a>) -> Auth0Reader<Request> -> Auth0Reader<Task<'a>>
//let contramap: Contramap<_> = fun f t -> t
    
//let xxx y token = y |> contramap (fun x -> getUser token)

type APIX<'a> = (Request -> Task<'a>) -> (Auth0Config -> Request) -> (Auth0Config -> Task<'a>)

let apix: APIX<_> = fun f api -> 
    fun env -> (api env) |> f 
*)