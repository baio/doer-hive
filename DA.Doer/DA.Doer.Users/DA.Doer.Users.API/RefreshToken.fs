[<AutoOpen>]
module DA.Doer.Users.RefreshToken

open System.Threading.Tasks
open DA.Auth.Domain
open DA.FSX
open DA.DataAccess.Domain

open ReaderTask

// API


//

type Auth = {
    refreshToken: string -> Task<TokensResult>
}


type API<'a> = ReaderTask<Auth, 'a>

///

let refreshToken (token: string): API<TokensResult> = fun x -> x.refreshToken token
    
    
