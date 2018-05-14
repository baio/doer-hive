module DA.Doer.Users.API.EnlistToVerify

open System.IO
open System.Threading.Tasks
open FSharpx.Task
open DA.FSX.Task
open System
open DA.Doer.Domain.Auth

///
type UserId            = string
type PhotoPath         = string
type TagId             = string
type UploadStatus      = UploadStatusOk | UploadStatusFail

type TrainingResultError = {
    PhotoPath: PhotoPath
    Status   : string
}

type TrainingBatchUploadResult = UploadStatus list

type TrainingUploadResult = {
    TagId           : TagId 
    TotalPhotosCount: int
    Results         : UploadStatus list
}

type UploadedPhotoParams = {
    UserId: UserId
    TagId : TagId
    Count : int
}

///

type UserMustHaveAtLeast5PhotosException (currentLength) =
    inherit Exception()
    member this.currentLength = currentLength

type TrainingUploadFailedException (trainingTotalPhotosCount: int, trainingUploadErrorsCount: int) =
    inherit Exception()
    member this.trainingTotalPhotosCount  = trainingTotalPhotosCount
    member this.trainingUploadErrorsCount = trainingUploadErrorsCount
       
///

type Api = {    
    getLatestUserPhotos     : UserId -> Task<PhotoPath list>
    getBlob                 : PhotoPath -> Task<Stream>
    getUserPhotoTrainingTag : UserId -> Task<TagId option>
    uploadToTraining        : UserId -> TagId option -> Stream list -> Task<TrainingUploadResult>
    markAsReadyForTraining  : UploadedPhotoParams -> Task<bool>
}

let validateUserHasAtLeast5Photos photosList = 
    if (photosList |> List.length) < 5 then 
        photosList.Length |> UserMustHaveAtLeast5PhotosException |> ofException
    else 
        photosList |> returnM

(*
Hadle result of upload to training
If not all items were uploaded with success, function will remove failed uploads from images
and then return error
*)
let handleUploadResult api userId (uploadResult: TrainingUploadResult) = 

    let successResultsLength = 
        uploadResult.Results
        |> List.choose (function | UploadStatusOk -> Some true | _ -> None)
        |> List.length

    let uploadResultsLength = uploadResult.Results.Length

    {
        UserId = userId
        TagId = uploadResult.TagId
        Count = uploadResult.TotalPhotosCount
    } 
    |> api.markAsReadyForTraining
    >>= (fun x -> 
        if successResultsLength <> uploadResultsLength then
            TrainingUploadFailedException(uploadResultsLength - successResultsLength, uploadResult.TotalPhotosCount) |> ofException
        else 
            returnM x
    )

let uploadToTraining api userId streams tagId =
    api.uploadToTraining userId tagId streams
    >>= handleUploadResult api userId

let enlistToVerify userId api = 
    api.getLatestUserPhotos userId
    >>= fun photoNames ->
        photoNames 
        |> validateUserHasAtLeast5Photos
        >>= (fun paths ->
            let streams = paths |> List.map api.getBlob |> sequence
            let tag = api.getUserPhotoTrainingTag userId
            FSharpx.Task.lift2(uploadToTraining api userId) streams tag
                >>= (fun x -> x)
        )
    
    