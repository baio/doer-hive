module DA.Doer.Domain.Auth

type Principal = {
    Id: string
    OrgId: string
}

type UserProfile = {
    Id: string
    OrgId: string
    Role: string
}


[<AutoOpen>]
module Profile = 
    
    let profileFromClaims (claims: Map<string, string>) = 
        {
            Id = claims.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]
            OrgId = claims.["https://doer.auth.com/orgId"]
            Role = claims.["https://doer.auth.com/role"]
        }
