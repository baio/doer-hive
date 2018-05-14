module EnlistToVerifyTests

open System
open Xunit
//open FSharpx.Task
open DA.FSX.Task
open DA.Doer.Users.API.EnlistToVerify
open System.IO
open System.Text
open DA.Doer.Mongo
open Setup
open DA.HTTP.Blob
open DA.Doer.Mongo.API
open Microsoft.Cognitive.CustomVision
open DA.VisionAPI
open FSharpx.Task

let VISION_API_PROJECT_ID = Guid.Parse "7e6f1b23-befc-4fa2-a1aa-b2cf497a3073"

let mapUploadToTrainingResult (tagId: Guid, summary: Models.CreateImageSummaryModel, photosCount: int) = 
    {
        TagId            = tagId.ToString()
        TotalPhotosCount = photosCount
        Results          =     summary.Images 
            |> Seq.map(fun x -> 
                if x.Status = "OK" then UploadStatusOk else UploadStatusFail
            ) |> List.ofSeq
    }    

let mockApi  =
    {
        getLatestUserPhotos = fun userId ->
           if userId = "test-user" then
                returnM ["1"; "2"; "7"; "8"; "9" ]
           else
                failwith "user not found"

        getUserPhotoTrainingTag = fun userId -> returnM None
            
        getBlob = fun photoId ->
            (new MemoryStream(buffer = Encoding.UTF8.GetBytes(photoId)) :> Stream) |> returnM

        uploadToTraining = fun userId tagId streams ->
           if userId = "test-user" then
                returnM { TagId = Guid.NewGuid().ToString(); Results = [ UploadStatusOk ]; TotalPhotosCount = 1 }
           else
                failwith "user not found"

        markAsReadyForTraining = fun x ->
           if x.UserId = "test-user" then
                returnM true
           else
                failwith "user not found"
    }


[<Fact>]
let ``Enlist to verify with mock api must work`` () =
    enlistToVerify "test-user" mockApi

let semiMockApi = 
    {
        getLatestUserPhotos = fun userId -> 
           getDirectoryBlobNames (Some 10) userId blobContainer

        getUserPhotoTrainingTag = fun userId -> 
            getTrainingPhotoTag userId mongoConfig

        getBlob = fun photoId ->
            getBlob photoId blobContainer
                
        uploadToTraining = fun userId tagId streams ->
            returnM { TagId = Guid.NewGuid().ToString(); Results =  List.replicate 5 UploadStatusOk ; TotalPhotosCount = 5 }
            (*
            let tag = 
                match tagId with 
                | Some x -> x |> Guid.Parse |> CreateImageExistentTag 
                | None -> CreateImageNewTag userId
            createImages tag streams VISION_API_PROJECT_ID trainingApi
            |> map mapUploadToTrainingResult
            *)
        
        markAsReadyForTraining = fun x ->
            markAsReadyForTraining x.UserId x.TagId x.Count mongoConfig
    }


let setupForEnlistTest userId =    

    task {

        //upload blobs
        let stream = new MemoryStream(buffer = Encoding.UTF8.GetBytes("xxx")) :> Stream
        let! _ = uploadStreamToStorage blobStorageConfig stream (userId + "/1")
        let! _ = uploadStreamToStorage blobStorageConfig stream (userId + "/2")
        let! _ = uploadStreamToStorage blobStorageConfig stream (userId + "/3")
        let! _ = uploadStreamToStorage blobStorageConfig stream (userId + "/4")
        let! _ = uploadStreamToStorage blobStorageConfig stream (userId + "/5")       

        return true
    }

let cleanForEnlistTest userId =
    removeBlobDirectory userId blobContainer

[<Fact>]
let ``Enlist to verify with mongo and blob api must work`` () =

    let userId = "22ecbeab15db5a2a4c145e70"
    
    task {

        let! _ = setupForEnlistTest userId

        let! _ = enlistToVerify userId semiMockApi

        let! _ = cleanForEnlistTest userId

        return true
    }
