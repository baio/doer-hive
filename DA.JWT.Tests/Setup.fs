module Setup

open FSharpx.Task
open DA.FSX.ReaderTask
open DA.JWT
open DA.Auth0
open FsUnit.Xunit

let openid = """{"issuer":"https://baio.auth0.com/","authorization_endpoint":"https://baio.auth0.com/authorize","token_endpoint":"https://baio.auth0.com/oauth/token","userinfo_endpoint":"https://baio.auth0.com/userinfo","mfa_challenge_endpoint":"https://baio.auth0.com/mfa/challenge","jwks_uri":"https://baio.auth0.com/.well-known/jwks.json","registration_endpoint":"https://baio.auth0.com/oidc/register","revocation_endpoint":"https://baio.auth0.com/oauth/revoke","scopes_supported":["openid","profile","offline_access","name","given_name","family_name","nickname","email","email_verified","picture","created_at","identities","phone","address"],"response_types_supported":["code","token","id_token","code token","code id_token","token id_token","code token id_token"],"response_modes_supported":["query","fragment","form_post"],"subject_types_supported":["public"],"id_token_signing_alg_values_supported":["HS256","RS256"],"token_endpoint_auth_methods_supported":["client_secret_basic","client_secret_post"],"claims_supported":["aud","auth_time","created_at","email","email_verified","exp","family_name","given_name","iat","identities","iss","name","nickname","phone_number","picture","sub"]}"""

let jwks = """{"keys":[{"alg":"RS256","kty":"RSA","use":"sig","x5c":["MIIDBTCCAe2gAwIBAgIJALw5AspU77N5MA0GCSqGSIb3DQEBBQUAMBkxFzAVBgNVBAMMDmJhaW8uYXV0aDAuY29tMB4XDTE0MDQyNTEyMzY0N1oXDTI4MDEwMjEyMzY0N1owGTEXMBUGA1UEAwwOYmFpby5hdXRoMC5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCyoTMJQxq083pCo3zhuFxOdSZXr2COQbB4Zfa6XJAM/E7tBCLKZzCSpcmYyryIYD1psbh0D1tgN/lkxBsx+vcg0SCA/HQGn8qHkk4L3HPWs8ht2XIDqZaUnYRw9zIQ7vNT/eYf05l0VXrP1cSTqJqBimiL93hTQOZjUH5EjVWeCM3GyAPLhjAABgwrHQVz5QsZakvkVOkJJSj+XiiG5D3nrG4+QnpEtt0CDARelHDFHkUwKqOYSWsm7S41LNT3gqvwfTQIiw0aY33LSNfiUZ/RZ7sZxLkVDPqXqJ5Sp30Qhu+I+IsuoTza/a0kUS9ZcdCqSIjyUF5jyAF1EgEJBb8zAgMBAAGjUDBOMB0GA1UdDgQWBBQ0sBYLs1ctIVHrWKpXq34zjSKwJzAfBgNVHSMEGDAWgBQ0sBYLs1ctIVHrWKpXq34zjSKwJzAMBgNVHRMEBTADAQH/MA0GCSqGSIb3DQEBBQUAA4IBAQB06fILhAZvZqL+b5Sgvt4bEPmNf4Qa5BFqFK8Hb8Z3uRnYsGxWK3Hg39UUyO1UlzjVk3NSglJKhMHj8zod+jfsAfXc9DYvc7MkYzQ8qQKnH/y0d5IuRhW3Rpew82KvDIHzPWz0yJziy5tHecfHfuquJNdeo3LKPQIOCIT3AnP+zizigrOefnTuUubcvbI9OKpXQp5d7zxV7z4/gIrfCM0vA72Pn9UwSysEa9/EFOXrLnhFXXxJf7W+EQEKShxa4GCKwyxsvqZGgGoltyqacAYMPyPoCAo2vzaWEpWP0ML/9P9l7BfjBqQ3RXrzJVBsteqOnsETYa3fSGbhpp9iDBor"],"n":"sqEzCUMatPN6QqN84bhcTnUmV69gjkGweGX2ulyQDPxO7QQiymcwkqXJmMq8iGA9abG4dA9bYDf5ZMQbMfr3INEggPx0Bp_Kh5JOC9xz1rPIbdlyA6mWlJ2EcPcyEO7zU_3mH9OZdFV6z9XEk6iagYpoi_d4U0DmY1B-RI1VngjNxsgDy4YwAAYMKx0Fc-ULGWpL5FTpCSUo_l4ohuQ956xuPkJ6RLbdAgwEXpRwxR5FMCqjmElrJu0uNSzU94Kr8H00CIsNGmN9y0jX4lGf0We7GcS5FQz6l6ieUqd9EIbviPiLLqE82v2tJFEvWXHQqkiI8lBeY8gBdRIBCQW_Mw","e":"AQAB","kid":"MkNGNjM5REM2QUZCREI4NkVDOEZDNUUzOTE4Qjg0RjA2MTI0RDQzNg","x5t":"MkNGNjM5REM2QUZCREI4NkVDOEZDNUUzOTE4Qjg0RjA2MTI0RDQzNg"}]}"""

let request = DA.Http.HttpTask.HttpClient.httpClientRequest

let getConfig () = 
    [
        "auth0:clientDomain"
        "auth0:clientId"
        "auth0:clientSecret"
        "auth0:audience"
        "auth0:issuer"
        "jwks:configuration"
        "jwks:keys"
    ] 
    |> DA.AzureKeyVault.getConfigSync "azureKeyVault:name"
    |> fun x -> 
        (request, {
            clientDomain = x.[0]
            clientId = x.[1]
            clientSecret = x.[2]
            audience = x.[3]
        }),
        {
            Audience = x.[3]
            Issuer = x.[4]            
            Jwks = ConfigJwksConst (x.[5], x.[6]) // ConfigJwks.ConfigJwksWellKnown // ConfigJwksConst jwks
        }

   
let authConfig, jwtConfig = getConfig()
