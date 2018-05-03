[<AutoOpen>]
module DA.Doer.Users.UpdateAvatar

open System.Threading.Tasks
open DA.Auth.Domain
open DA.FSX
open DA.DataAccess.Domain
open DA.Doer.Domain.Auth
open System.IO

type Auth = {
    principal: Principal
    updateAvatar: string -> Task<bool>
}

type DataAccess = {
    uploadBlob: Stream -> Task<string>
    getUserDoc: string -> Task<UserDoc>
    updateDoc: UserDoc -> Task<bool>
}

type API<'a> = ReaderTask.ReaderTask<DataAccess * Auth, 'a>

///
open Task
(*
let checkAuth: API<bool> = fun (da, auth) ->
    (fun (doc: UserDoc) -> doc. = auth.principal.id) <!> da.getDoc 
    


let updateAvatar (stream: Stream): API<string> = 
    ReaderTask.readerTask {
        let! orgId  = insertOrg
        let! userId = insertUser   orgId
        let! result = registerUser orgId userId
        return { orgId = orgId; userId = userId; authUserId = result.userId; tokens = result.tokens }
    }
*)
    
    
