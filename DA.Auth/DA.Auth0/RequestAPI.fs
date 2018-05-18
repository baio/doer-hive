module DA.Auth0.RequestAPI

open FSharpx.Task
open System.Threading.Tasks
open DA.FSX.HttpTask
open FSharpx.Reader
open DA.Auth.Domain

type Auth0Reader<'a> = Reader<Auth0Config, 'a>
type RequestAPI = Auth0Reader<Request>

// create user

type UserMetadata = {
    name: string
    avatar: string
}

type AppMetadata = {
    role: string
    orgId: string
}

type CreateUserPayload = {
    user_id: string
    connection: string
    email: string
    password: string
    user_metadata: UserMetadata
    app_metadata: AppMetadata
}


type UpdateUserAvatarMetadata = { avatar: string }

type UpdateUserAvatarPayload = { user_metadata: UpdateUserAvatarMetadata }


type CreateUser = CreateUserInfo -> string -> RequestAPI
let createUser: CreateUser = fun userInfo token env -> 
    {
        HttpMethod = POST
        Url = sprintf "https://%s.auth0.com/api/v2/users" (env.ClientDomain)
        Payload = JsonPayload 
            {
                // Name and Picture parameters couldn't be set for profile directly
                // https://auth0.com/docs/user-profile/normalized/auth0 (Fields that are always generated)
                user_id = sprintf "doer|%s" userInfo.UserId
                connection = "Username-Password-Authentication"
                email = userInfo.Email
                password = userInfo.Password
                user_metadata =  
                    {
                        name = userInfo.Name
                        avatar = userInfo.Avatar
                    }
                app_metadata = 
                    {
                        role = sprintf "%O" userInfo.Role
                        orgId = userInfo.OrgId                    
                    }
            }
        Headers = ["authorization", token]
        QueryString = []
    }

type UpdateUserAvatar = (string * string) -> string -> RequestAPI
let updateUserAvatar: UpdateUserAvatar = fun (userId, avatarUrl) token env -> 
    {
        HttpMethod = PATCH
        Url = sprintf "https://%s.auth0.com/api/v2/users/%s" (env.ClientDomain) userId
        Payload = JsonPayload 
            {
                user_metadata =  
                    {
                        avatar = avatarUrl
                    }
            }
        Headers = ["authorization", token]
        QueryString = []
    }

// get user

type GetUser = string -> RequestAPI
let getUser: GetUser = fun token env -> 
    {
        HttpMethod = GET
        Url = sprintf "https://%s.auth0.com/api/v2/users?fields=user_id" (env.ClientDomain)
        Payload = None
        Headers = ["authorization", token]
        QueryString = []
    }

// get management token

let getManagementToken: RequestAPI = fun env -> 
    {
        HttpMethod = POST
        Url = sprintf "https://%s.auth0.com/oauth/token" env.ClientDomain
        Payload = FormPayload  [
                    "audience", sprintf "https://%s.auth0.com/api/v2/" env.ClientDomain
                    "grant_type", "client_credentials"
                    "client_id", env.ClientId
                    "client_secret", env.ClientSecret
                ]
        Headers = []
        QueryString = []
    }

// remove user

type RemoveUser = string -> string -> RequestAPI
let removeUser: RemoveUser = fun token userId env -> 
    {
        HttpMethod = DELETE
        Url = sprintf "https://%s.auth0.com/api/v2/users/%s" env.ClientDomain userId
        Payload = None
        Headers = ["authorization", token]
        QueryString = []
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

type Login = LoginInfo -> RequestAPI
let login: Login = fun loginInfo env -> 
    {
        HttpMethod = POST
        Url = sprintf "https://%s.auth0.com/oauth/token" env.ClientDomain
        Payload = FormPayload 
            [
                "username", loginInfo.Email
                "password", loginInfo.Password
                "grant_type", "password"
                "client_id", env.ClientId
                "client_secret", env.ClientSecret
                "audience", env.Audience
                "scope", "openid profile offline_access"
            ]
        Headers = []
        QueryString = []
    }

type RefreshToken = string -> RequestAPI
let refreshToken: RefreshToken = fun token env -> 
    {
        HttpMethod = POST
        Url = sprintf "https://%s.auth0.com/oauth/token" env.ClientDomain
        Payload = FormPayload 
            [
                "grant_type", "refresh_token"
                "client_id", env.ClientId
                "client_secret", env.ClientSecret
                "refresh_token", token
            ]
        Headers = []
        QueryString = []
    }


