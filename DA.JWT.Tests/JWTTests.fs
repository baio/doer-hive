module JWTTests

open System
open Xunit
open Setup
open DA.JWT
open FSharpx.Task
open DA.FSX.Task
open FsUnit.Xunit
open DA.Auth0
open DA.Auth0.API
open DA.Auth.Domain

let token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Ik1rTkdOak01UkVNMlFVWkNSRUk0TmtWRE9FWkROVVV6T1RFNFFqZzBSakEyTVRJMFJEUXpOZyJ9.eyJodHRwczovL2RvZXIuYXV0aC5jb20vb3JnSWQiOiI1YWU0ZDlhM2Q5ZjE1ZTBkMTQ1ZTM0MWUiLCJodHRwczovL2RvZXIuYXV0aC5jb20vcm9sZSI6Ik93bmVyIiwiaXNzIjoiaHR0cHM6Ly9iYWlvLmF1dGgwLmNvbS8iLCJzdWIiOiJhdXRoMHxkb2VyfDVhZTRkOWEyZDlmMTVlMGQxNDVlMzQxZCIsImF1ZCI6WyJodHRwczovL2RvZXItdGVzdC5jb20iLCJodHRwczovL2JhaW8uYXV0aDAuY29tL3VzZXJpbmZvIl0sImlhdCI6MTUyNTQzNDEzOSwiZXhwIjoxNTI1NTIwNTM5LCJhenAiOiI1U3ZrN1dQZWNWYlJFMTY1dENFWU5XSVRSYlZzSVhFNCIsInNjb3BlIjoib3BlbmlkIHByb2ZpbGUgb2ZmbGluZV9hY2Nlc3MiLCJndHkiOiJwYXNzd29yZCJ9.TEzE7mm-DyfO0EEMPMjrBsX1cbD6r0xEuFkssGJcEv4_qOzEtNLYHT1ZC5muldNX-X4OSTxFJP0gN1tvqNyG6zOFrAtGj-CAE-p9eYlWi6J-2vhVeF5a4H3ccKFl22spfvc0mBk6exEE5U733lF6KHVbGS9cPyKBoXR5VM7JXEwqzXEKTMDEbKCYOibqZ60KYUobyYqEa4zpV6u1KZwiMp3b4gmTjN1FuC8VzOWo30M24Z-IXbF7r4DhIA7_0h0XVA-8PazWOGaVpUaggIe4nv95hbXiYy6c2Y-Jpo0d6rWZse3u4uu8feTfDlz5b7w0UdwyiI9zg0VL-MwI2r-CDw"

[<Fact>]
let ``Validate token by well known jwks`` () =
   
    let userInfo = {
        UserId = "bd296071-f13f-4023-bf91-885ee0729137"
        OrgId = "5538ee3c-d332-44a5-ae3b-610e8b015327"
        Name = "validate token user"
        Email = "validate_token_user@gmail.com"
        Password = "PasLslol123"
        Avatar = null
        Role = "Owner"
    }
    
    let assert' = fun x ->
        x |> should not' Empty

    assert' <!> (task {
        //let! regUserRes = registerUser userInfo authConfig
        let! principalRes = getPrincipal token jwtConfig
        // let! _ = removeUser regUserRes.userId authConfig
        return principalRes
    }) 

    
