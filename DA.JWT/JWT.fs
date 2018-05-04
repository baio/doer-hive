[<AutoOpen>]
module DA.JWT

// creds : https://liftcodeplay.com/2017/11/25/validating-auth0-jwt-tokens-in-azure-functions-aka-how-to-use-auth0-with-azure-functions/

open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.IdentityModel.Tokens
open System.Threading
open System.Threading.Tasks
open DA.FSX.Task
open System.Net.Http
open System


type ConfigJwks = 
    | ConfigJwksConst of string
    | ConfigJwksWellKnown

type Config = {
    Issuer: string
    Audience: string
    Jwks: ConfigJwks
}

type JwkDocRetriever(jwks) =    
    interface IDocumentRetriever with
        member this.GetDocumentAsync (address, cancel) =
            Tasks.Task.FromResult(jwks)

let getSigningKey (configurationManager: ConfigurationManager<OpenIdConnectConfiguration>) =
    configurationManager.GetConfigurationAsync(CancellationToken.None)     
    |> map(fun res -> res.SigningKeys |> Seq.cast<SecurityKey>)
    
let getIssuerSigningKeysWellKnown (issuer: string) = 

    // TODO use 'use' !
    let httpClient = new HttpClient()
    let requireHttps = issuer.StartsWith("https://")
    let documentRetriever = HttpDocumentRetriever(httpClient, RequireHttps = requireHttps)

    new ConfigurationManager<OpenIdConnectConfiguration>(
        sprintf "%s.well-known/openid-configuration" issuer,
        OpenIdConnectConfigurationRetriever(),
        documentRetriever
    ) |> getSigningKey

let getIssuerSigningKeysConst (jwks: string) = 
    new ConfigurationManager<OpenIdConnectConfiguration>(
        "",
        OpenIdConnectConfigurationRetriever(),
        JwkDocRetriever(jwks)
    ) |> getSigningKey

let getIssuerSigningKeys config = 
    match config.Jwks with
    | ConfigJwksWellKnown ->
        getIssuerSigningKeysWellKnown config.Issuer
    | ConfigJwksConst str ->
        getIssuerSigningKeysConst str
        
let getPrincipal' token config (issuerSigningKeys: SecurityKey seq) =
        
    let validationParameter = 
        new TokenValidationParameters(
            RequireSignedTokens = true,                
            ValidAudience = config.Audience,
            ValidateAudience = true,
            ValidIssuer = config.Issuer,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKeys = issuerSigningKeys,
            // TODO : bug with european tenant ?
            ClockSkew = TimeSpan(hours = 2, minutes = 0, seconds = 0)
        )

    let handler = new JwtSecurityTokenHandler()            
    handler.ValidateToken(token, validationParameter, ref null)


let getPrincipal token config =     
    getIssuerSigningKeys config
    |> map(getPrincipal' token config)

let getClaims' (principal: ClaimsPrincipal) =     
    principal.Claims
    |> Seq.map(fun claim -> claim.Type, claim.Value)
    |> Map.ofSeq

let getClaims token config =     
    getPrincipal token config
    |> map getClaims'
    