module DA.Doer.Domain.Auth

type Principal = {
    Id: string
    OrgId: string
    Role: string
}


[<AutoOpen>]
module Profile = 
    open DA.Doer.Domain.Users
    
    let principalFromClaims (claims: Map<string, string>) = 
        {
            Id = claims.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]
            OrgId = claims.["https://doer.auth.com/orgId"]
            Role = claims.["https://doer.auth.com/role"]
        }

    let principalCanCreateWorkers (principal: Principal) =     
        let role = roleFromString principal.Role
        if role = Owner || role = Master then
            true
        else 
            false
        