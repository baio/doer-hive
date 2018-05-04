module JWTTests

open System
open Xunit
open Setup
open DA.JWT
open FSharpx.Task
open DA.FSX.Task
open FsUnit.Xunit
open DA.Auth0
open DA.Auth.Domain

[<Fact>]
let ``Validate token by well known jwks`` () =
   
    let userInfo = {
        UserId = "bd296071-f13f-4023-bf91-885ee0729131"
        OrgId = "5538ee3c-d332-44a5-ae3b-610e8b015321"
        Name = "validate token user"
        Email = "validate_token_user@gmail.com"
        Password = "PasLslol123"
        Avatar = "http://avatar.com/1"
        Role = "Owner"
    }
    
    let assert' = fun x ->
        x |> should not' Empty

    assert' <!> 
        task {
            let! regUserRes = registerUser userInfo authConfig
            let! principalRes = getPrincipal regUserRes.tokens.accessToken jwtConfig
            let! _ = removeUser regUserRes.userId authConfig
            return principalRes
        }
    
    

    
