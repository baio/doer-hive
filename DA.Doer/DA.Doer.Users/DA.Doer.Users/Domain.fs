namespace DA.Doer.Users

module AuthId = 
    
    let auth2domain (x: string) = 
        '|' |> x.LastIndexOf  |> x.Substring

    let domain2auth = 
        sprintf "doer|%s"


[<AutoOpen>]
module Utils = 
    open DA.Doer.Domain.Auth
    open DA.FSX.ReaderTask
    
    let private trimBearer (x: string) = x.Split([|' '|]).[1]

    let getPrincipal = 
        trimBearer >> DA.JWT.getClaims >> map(
            principalFromClaims >> (fun x -> {x with Id = AuthId.auth2domain x.Id } )
        )