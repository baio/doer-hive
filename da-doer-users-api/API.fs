module DA.Doer.Users.API

open DA.FSX.ReaderTask
open System.Threading.Tasks

type OrgDoc = {    
    Name: string
    OwnerEmail: string
}

type UserDoc = {
    OrgId: string
    Role: string
    FirstName: string
    MidName: string 
    LastName: string
    Email: string
    Phone: string
    Ancestors: string seq
    Avatar: string
}

type DataAccessTable = 
    | User of UserDoc
    | Org of OrgDoc

type DataAccess = {
    insertDoc: DataAccessTable -> Task<string>
}

// 

type RegisterUserInfo = {
    UserId: string
    OrgId: string
    Name: string
    Email: string
    Password: string
    Avatar: string
    Role: string
}

type TokensResult = {
    IdToken: string
    AccessToken: string
    RefreshToken: string
}

type RegisterUserResult = {
    UserId: string
    Tokens: TokensResult
}

type Auth = {
    registerUser: RegisterUserInfo -> Task<RegisterUserResult>
}


type API<'a> = ReaderTask<DataAccess * Auth, 'a>