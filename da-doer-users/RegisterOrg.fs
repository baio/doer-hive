module DA.Doer.Users.RegisterUser

open DA.Doer.Users.API
open DA.FSX

open ReaderTask

type PersonName = {
    FirstName: string
    LastName: string
    MiddleName: string
}

type User = {
    Name: PersonName
    Phone: string
    Email: string
    Avatar: string
}

type Org = {
    Name: string
}

type RegisterOrgInfo = {
    User: User
    Org: Org
    Password: string
}

let OWNER_ROLE = "Owner"

let info2org (info: RegisterOrgInfo) = 
    {
        Name = info.Org.Name
        OwnerEmail = info.User.Email
    }

let info2user orgId (info: RegisterOrgInfo) = 
    {
        OrgId = orgId
        Role = OWNER_ROLE
        FirstName = info.User.Name.FirstName
        MidName = info.User.Name.MiddleName
        LastName = info.User.Name.LastName
        Email = info.User.Email
        Phone = info.User.Phone
        Ancestors = []
        Avatar = info.User.Avatar
    }

let info2register orgId userId (info: RegisterOrgInfo) = 
    {
        UserId = userId
        OrgId = orgId
        Name = sprintf "%s %s %s" info.User.Name.LastName info.User.Name.FirstName info.User.Name.MiddleName
        Email = info.User.Email
        Password = info.Password
        Avatar = info.User.Avatar
        Role = OWNER_ROLE
    }

type RegOrgApi<'a> = ReaderTask<RegisterOrgInfo * (DataAccess * Auth), 'a>

let insertOrg'                 info = fun (dataAccess, _) -> info |> info2org |> Org |> dataAccess.insertDoc

let insertUser'   orgId        info = fun (dataAccess, _) -> info |> info2user orgId |> User |> dataAccess.insertDoc

let registerUser' orgId userId info = fun (_,       auth) -> info |> info2register orgId userId |> auth.registerUser

//

let insertOrg                : RegOrgApi<string> =             insertOrg' |> Reader.flat

let insertUser   orgId       : RegOrgApi<string> =             insertUser' orgId |> Reader.flat

let registerUser orgId userId: RegOrgApi<RegisterUserResult> = registerUser' userId orgId |> Reader.flat
        
let registerOrg (info: RegisterOrgInfo): Api<RegisterUserResult> = fun x ->
    (readerTask {
        let! orgId  = insertOrg
        let! userId = insertUser   orgId
        let! result = registerUser orgId userId
        return result
    }) (info, x)
    
