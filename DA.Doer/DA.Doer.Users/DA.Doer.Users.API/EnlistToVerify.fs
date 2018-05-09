module DA.Doer.Users.API.EnlistToVerify

open System.IO
open System.Threading.Tasks
open DA.FSX.Task
open System

type UserMustHaveAtLeast5PhotosException (currentLength) =
    inherit Exception()
    member this.currentLength = currentLength
       
///

type Api = {    
    getLatestUserPhotos     : string -> Task<string list>
    getBlob                 : string -> Task<Stream>
    uploadToTraining        : string -> Stream list -> Task<bool>
    markAsReadyForTraining  : string -> Task<bool>
}


let validateUserHasAtLeast5Photos photosList = 
    if (photosList |> List.length) < 5 then 
        photosList.Length |> UserMustHaveAtLeast5PhotosException |> ofException
    else 
        photosList |> returnM

let enlistToVerify userId api = 
    api.getLatestUserPhotos userId
    >>= validateUserHasAtLeast5Photos
    >>= (List.map api.getBlob >> sequence)
    >>= api.uploadToTraining userId
    >>= (fun _ -> api.markAsReadyForTraining userId)
    
    