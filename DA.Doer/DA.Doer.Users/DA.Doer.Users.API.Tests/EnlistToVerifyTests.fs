module EnlistToVerifyTests

open System
open Xunit
open FSharpx.Task
open DA.FSX.Task
open DA.Doer.Users.API.EnlistToVerify
open System.IO
open System.Text
open DA.Doer.Mongo
open Setup
open DA.HTTP.Blob
open DA.Doer.Mongo.API

let mockApi  =
    {
        getLatestUserPhotos = fun userId ->
           if userId = "test-user" then
                returnM ["1"; "2"; "7"; "8"; "9" ]
           else
                failwith "user not found"


        getBlob = fun photoId ->
            (new MemoryStream(buffer = Encoding.UTF8.GetBytes(photoId)) :> Stream) |> returnM

        uploadToTraining = fun userId streams ->
           if userId = "test-user" then
                returnM true
           else
                failwith "user not found"

        markAsReadyForTraining = fun userId ->
           if userId = "test-user" then
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
           getDirectoryBlobNames (Some 5) userId blobContainer

        getBlob = fun photoId ->
            getBlob photoId blobContainer
                
        uploadToTraining = fun userId streams ->
            FSharpx.Task.returnM true
        
        markAsReadyForTraining = fun userId ->
            markAsReadyForTraining userId mongoConfig
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

    let userId = "5aecbeab15db5a2a4c145e70"
    
    task {

        let! _ = setupForEnlistTest userId

        let! _ = enlistToVerify userId semiMockApi

        let! _ = cleanForEnlistTest userId

        return true
    }
