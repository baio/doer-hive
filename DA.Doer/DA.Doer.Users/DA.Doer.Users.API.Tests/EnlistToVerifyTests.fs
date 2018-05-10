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

let setupForEnlistTest () =

    task {
        
        // create mongo records
        let! l1 = addUserPhotoLink "test-user" "1" mongoConfig
        let! l2 = addUserPhotoLink "test-user" "2" mongoConfig
        let! l3 = addUserPhotoLink "test-user" "5" mongoConfig
        let! l4 = addUserPhotoLink "test-user" "7" mongoConfig
        let! l5 = addUserPhotoLink "test-user" "9" mongoConfig

        //upload blobs
        let stream = new MemoryStream(buffer = Encoding.UTF8.GetBytes("xxx")) :> Stream
        let! _ = uploadStreamToStorage blobStorageConfig stream l1
        let! _ = uploadStreamToStorage blobStorageConfig stream l2
        let! _ = uploadStreamToStorage blobStorageConfig stream l3
        let! _ = uploadStreamToStorage blobStorageConfig stream l4
        let! _ = uploadStreamToStorage blobStorageConfig stream l5       

        return true
    }

let cleanForEnlistTest () =

    task {
        
        let! docs = getTopUserPhotoLinks 5  "test-user" mongoConfig

        let ids = docs |> List.map(fun x -> x.Id |> bsonId2String)

        let! _ = ids |> List.map(fun x -> removeBlobFromStorage x blobContainer) |> sequence

        let! _ = removeUserPhotoLinks "test-user" mongoConfig

        return true
    }

[<Fact>]
let ``Enlist to verify with mongo and blob api must work`` () =
    
    task {

        let! _ = setupForEnlistTest()

        let! _ = cleanForEnlistTest()

        return true
    }
