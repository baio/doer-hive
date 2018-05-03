[<AutoOpen>]
module DA.Doer.Users.UpdateAvatar

open System.Threading.Tasks
open FSharpx.Task
open DA.Auth.Domain
open DA.FSX
open ReaderTask
open Task
open DA.DataAccess.Domain
open DA.Doer.Domain.Auth
open System.IO

type Auth = {
    GetPrincipal: string -> Result<Principal, exn>
    UpdateAvatar: string -> string -> Task<bool>
}

type DataAccess = {
    UploadAvatar: Stream -> string -> Task<string>
    UpdateUserDocAvatar: string -> string -> Task<bool>
}

type ImageResizer = {
    ResizeImage: Stream -> int * int -> Stream
}

type API<'a> = ReaderTask.ReaderTask<DataAccess * Auth * ImageResizer, 'a>

///

let resizes = [
    ("sm", (100, 100))
    ("md", (250, 250))
]

let private updateAvatars userId path  (da, auth, _)= 
        [ 
            fun () -> da.UpdateUserDocAvatar userId path
            fun () -> auth.UpdateAvatar      userId path
        ]  |> FSharpx.Task.Parallel

let private resizeAndUpload stream userId = fun (da, _, resizer) ->
    let getName = sprintf "%s-%s" userId
    resizes
    |> ListPair.map (resizer.ResizeImage stream)
    |> List.append(["orig", stream])
    |> ListPair.bimap (getName) (da.UploadAvatar)
    |> ListPair.crossApply
    |> sequence

let private getPrincipal token = fun (_, auth, _) -> 
    token |> auth.GetPrincipal |> Task.ofResult

let updateAvatar (token: string) (stream: Stream): API<string> = 
    readerTask {
        // get principal from access token
        let! principal = getPrincipal token
        let principalId = principal.Id
        // resize original stream and upload them all
        let! uploadResult = resizeAndUpload stream principalId
        // take smallest image and update user profile's avatar with it
        let avatarUrl = uploadResult.[0]        
        let! _ = updateAvatars principalId avatarUrl
        return avatarUrl
    }
           
