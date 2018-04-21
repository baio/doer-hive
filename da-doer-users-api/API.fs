module DA.Doer.Users.API

open DA.FSX.ReaderTask
open System.Threading.Tasks
open DA.Auth.Domain
open DA.DataAccess.Domain

type DataAccessTable = 
    | User of UserDoc
    | Org of OrgDoc

type DataAccess = {
    insertDoc: DataAccessTable -> Task<string>
}

// 

type Auth = {
    registerUser: CreateUserInfo -> Task<RegisterUserResult>
}


type API<'a> = ReaderTask<DataAccess * Auth, 'a>