[<AutoOpen>]
module DA.Doer.Users.Login

open System.Threading.Tasks
open DA.Auth.Domain
open DA.FSX
open DA.DataAccess.Domain

open ReaderTask

type Auth = {
    Login: LoginInfo -> Task<TokensResult>
}


type API<'a> = ReaderTask<Auth, 'a>

///

let login (info: LoginInfo): API<TokensResult> = fun x -> x.Login info   