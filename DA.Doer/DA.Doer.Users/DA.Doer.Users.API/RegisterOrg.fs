[<AutoOpen>]
module DA.Doer.Users.RegisterOrg

open System.Threading.Tasks
open DA.Auth.Domain
open DA.FSX
open DA.DataAccess.Domain

open ReaderTask

// API

type DataAccessTable = 
    | User of UserDoc
    | Org of OrgDoc

type DataAccessApi = {
    InsertDoc: DataAccessTable -> Task<string>
}

type AuthApi = {
    RegisterUser: CreateUserInfo -> Task<RegisterUserResult>
}

type Api = {
    DataAccess: DataAccessApi
    Auth      : AuthApi
}

type API<'a> = ReaderTask<Api, 'a>

///

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

type RegOrgApi<'a> = ReaderTask<RegisterOrgInfo * Api, 'a>

let insertOrg'                 info = fun api -> info |> info2org |> Org |> api.DataAccess.InsertDoc

let insertUser'   orgId        info = fun api -> info |> info2user orgId |> User |> api.DataAccess.InsertDoc

let registerUser' orgId userId info = fun api -> info |> info2register orgId userId |> api.Auth.RegisterUser

//

let insertOrg                : RegOrgApi<string> =             insertOrg' |> Reader.flat

let insertUser   orgId       : RegOrgApi<string> =             insertUser' orgId |> Reader.flat

let registerUser orgId userId: RegOrgApi<RegisterUserResult> = registerUser' orgId userId |> Reader.flat

type RegisterOrgResult = {
    userId: string
    orgId: string
    authUserId: string
    tokens: TokensResult
}
        
let registerOrg (info: RegisterOrgInfo): API<RegisterOrgResult> = fun x ->
    (readerTask {
        let! orgId  = insertOrg
        let! userId = insertUser   orgId
        let! result = registerUser orgId userId
        return { orgId = orgId; userId = userId; authUserId = result.UserId; tokens = result.Tokens }
    }) (info, x)
    
