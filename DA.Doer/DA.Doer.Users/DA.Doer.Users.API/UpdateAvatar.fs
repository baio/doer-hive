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

type AuthApi = {
    GetPrincipalId: string -> Task<string>
    UpdateAvatar: string -> string -> Task<bool>
}

type DataAccessApi = {
    UploadBlob: Stream -> string -> Task<string>
    UpdateUserDocAvatar: string -> string -> Task<bool>
}

type ImageResizerApi = {
    ResizeImage: Stream -> int * int -> Stream
}

type Api = {
    Auth        : AuthApi
    DataAccess  : DataAccessApi
    ImageResizer: ImageResizerApi
}

type API<'a> = ReaderTask.ReaderTask<Api, 'a>

///

let resizes = [
    ("sm", (100, 100))
    ("md", (250, 250))
]

let private updateAvatars userId path api = 
    [ 
        fun () -> api.DataAccess.UpdateUserDocAvatar userId path
        fun () -> api.Auth.UpdateAvatar      userId path
    ]  |> FSharpx.Task.Parallel

let private normalize (x: string) = x.Replace('|', '_')

let private resizeAndUpload stream userId = fun api ->
    let getName = sprintf "%s-%s" userId >> normalize
    resizes
    |> ListPair.map (api.ImageResizer.ResizeImage stream)
    |> fun x -> x@["orig", stream]
    |> ListPair.bimap (getName) (api.DataAccess.UploadBlob)
    |> ListPair.crossApply
    |> sequence

let private getPrincipal token = fun api -> 
    token |> api.Auth.GetPrincipalId

let updateAvatar (token: string) (stream: Stream): API<string> = 
    readerTask {
        // get principal from access token
        let! principalId = getPrincipal token
        // resize original stream and upload them all
        let! uploadResult = resizeAndUpload stream principalId
        // take smallest image and update user profile's avatar with it
        let avatarUrl = uploadResult.[0]        
        let! _ = updateAvatars principalId avatarUrl
        return avatarUrl
    }
          
    