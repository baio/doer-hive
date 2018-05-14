module DA.Doer.Users.EnlistToVerify

open DA.Doer.Users.API.EnlistToVerify
open DA.Doer.Mongo
open DA.FSX.Task
open Microsoft.WindowsAzure.Storage.Blob
open DA.HTTP
open Microsoft.Cognitive.CustomVision
open System
open DA.VisionAPI

type Config = {
    mongo        : MongoConfig
    blobContainer: CloudBlobContainer
    trainingApi  : TrainingApi
}

let VISION_API_PROJECT_ID = Guid.Parse("test")

let mapUploadToTrainingResult (tagId: Guid, summary: Models.CreateImageSummaryModel, photosCount: int) = 
    {
        TagId            = tagId.ToString()
        TotalPhotosCount = photosCount
        Results          =     summary.Images 
            |> Seq.map(fun x -> 
                if x.Status = "OK" then UploadStatusOk else UploadStatusFail
            ) |> List.ofSeq
    }
    

let getApi config = 
    {
        getLatestUserPhotos = fun userId -> 
           (List.map(fun x -> x.PhotoId)) <!> (getTopUserPhotoLinks 5 userId config.mongo)           

        getUserPhotoTrainingTag = fun userId -> 
            getTrainingPhotoTag userId config.mongo

        getBlob = fun photoId ->
            Blob.getBlob photoId config.blobContainer

        uploadToTraining = fun userId tagId streams ->
            let tag = 
                match tagId with 
                | Some x -> x |> CreateImageNewTag 
                | None -> CreateImageNewTag userId
            createImages tag streams VISION_API_PROJECT_ID config.trainingApi
            |> map mapUploadToTrainingResult
                                
        markAsReadyForTraining = fun x ->
            markAsReadyForTraining x.UserId x.TagId x.Count config.mongo            
    }


let enlistToVerify userId = 
    getApi >> enlistToVerify userId

module Errors =
    
    open DA.Doer.Users.Errors
    open DA.Doer.Domain.Errors
    open DA.Doer.Mongo.Errors

    let inline private map f x = Option.map f x

    let matchUserMustHaveAtLeast5PhotosException (e: exn) = 
        match e with
        | :? UserMustHaveAtLeast5PhotosException as ex -> Some ex.currentLength
        | _ -> None

    let userMustHaveAtLeast5PhotosException (currentLength: int) = 
        let message = sprintf "Unexpected number of photos to train in storage %i. Must be at least 5" currentLength
        new System.Exception(message) |> unexepcted       
    
    
    let getHttpError (ex: exn) =  
        [
            matchConnectionError >> (map connectionFail)
            matchUserMustHaveAtLeast5PhotosException >> (map userMustHaveAtLeast5PhotosException)
            unexepcted >> Some
        ] 
        |> List.choose(fun x -> x ex)
        |> List.head
