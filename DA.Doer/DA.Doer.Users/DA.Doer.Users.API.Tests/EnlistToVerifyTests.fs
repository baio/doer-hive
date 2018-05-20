module EnlistToVerifyTests

open System
open Xunit
//open FSharpx.Task
open DA.Doer.Users.API.EnlistToVerify
open System.IO
open System.Text
open DA.Doer.Mongo
open Setup
open DA.HTTP.Blob
open DA.Doer.Mongo.API
open DA.FSX.Task
open DA.Doer.Domain.Auth
open DA.Doer.Users.API.IdentifyPhoto
open DA.FacePlusPlus
open DA.DataAccess.Domain
open FsUnit.Xunit

let mockApi  =
    {
        IsPrincipalAncestor = fun principalId userId -> returnM true

        GetUserPhotos = fun userId -> 
            (new MemoryStream(buffer = Encoding.UTF8.GetBytes("123")) :> Stream) |> List.replicate 5 |> returnM

        IsPhotoSetExists = fun principalId -> returnM true
        
        CreatePhotoSet = fun setId -> returnM true

        AddPhotosToSet = fun setId treams -> returnM ["777"]

        MarkAsUploaded = fun x -> returnM 1
    }


[<Fact>]
let ``Enlist to verify with mock api must work`` () =
    let principal: Principal = {
        Id = "test-principal"
        OrgId = "test-org"
        Role = "Owner"
    }
    enlistToVerify principal "test-user" mockApi


let semiMockApi = 
    {
        IsPrincipalAncestor = fun principalId userId -> returnM true

        GetUserPhotos = fun userId -> getDirectoryBlobs (Some 3) userId blobApi

        IsPhotoSetExists = fun orgId -> orgHasPhotoLink orgId mongoApi
        
        CreatePhotoSet = fun setId -> 
            createFaceSet setId faceppApi |> ``const`` true
            //returnM true

        AddPhotosToSet = fun setId streams -> 
            detectAndAddSinglePersonFaces setId streams faceppApi |> map( fun (x, _, _) -> x )
            //returnM ["100"]

        MarkAsUploaded = fun x -> 
            addUserPhotoLinks' x.OrgId x.UserId x.FaceTokenIds mongoApi

    }

let identPhotoApi  = 
    {
        IdentifyPhoto = fun orgId stream -> 
            searchFace orgId stream faceppApi 
                |> map(fun x -> 
                    // TODO : throw exception if nothing found ?
                    (x.results.[0].confidence, x.results.[0].face_token)
                 )
        FindUser      = fun faceTokenId -> getUserByPhotoId faceTokenId mongoApi
    }

let setupForEnlistTest () =    

    task {

        let! orgId = createOrg { Name = "test-org-enlist"; OwnerEmail = "test-org-enlist@mail.ru" } mongoApi

        let userDoc1: UserDoc =  {
            OrgId = orgId
            Role = "Owner"
            FirstName = "first"
            MidName = "user"
            LastName = "name"
            Email = "first_user_name@gmail.com"
            Phone = "+79772753595"
            Ancestors = []
            Avatar = ""
        } 

        let userDoc2: UserDoc =  {
            OrgId = orgId
            Role = "Master"
            FirstName = "second"
            MidName = "user"
            LastName = "name"
            Email = "second_user_name@gmail.com"
            Phone = "+79772753595"
            Ancestors = []
            Avatar = ""
        } 
       
        // create user 1
        let! user1Id = createUser userDoc1 mongoApi
        
        // create user 2
        let! user2Id = createUser userDoc2 mongoApi

        // upload user 1 blobs
        let img1 = new FileStream("./assets/lev-1.jpg", FileMode.Open);    
        let img2 = new FileStream("./assets/lev-2.jpg", FileMode.Open);    
        let! _ = 
            [
                uploadStreamToStorageDirectoty user1Id img1 blobApi
                uploadStreamToStorageDirectoty user1Id img2 blobApi
            ] |> DA.FSX.Task.sequence

        // upload user 2 blobs
        let img3 = new FileStream("./assets/max-1.jpg", FileMode.Open);    
        let! _ = uploadStreamToStorageDirectoty user2Id img3 blobApi

        return (orgId, user1Id, user2Id)
    }

let cleanForEnlistTest (orgId, user1Id, user2Id) =
    
    task {

        let! _ = removeOrg orgId mongoApi 
        
        let! _ = removeUserData user1Id mongoApi 

        let! _ = removeUserData user2Id mongoApi 
        
        let! _ = removeBlobDirectory user1Id blobApi

        let! _ = removeBlobDirectory user2Id blobApi

        let! _ = deleteFaceset orgId faceppApi

        return true
    }

// Paid or Limited !
[<Fact>]
let ``Enlist to verify with mongo and blob api must work`` () =
           
    task {

        let! orgId, user1Id, user2Id = setupForEnlistTest()

        let principal = {
                Id    = user1Id
                OrgId = orgId
                Role = "Owner"
            }

        return! task {                

            let! _ = enlistToVerify principal user1Id semiMockApi
            
            let! _ = enlistToVerify principal user2Id semiMockApi
       
            let imgUser1 = new FileStream("./assets/lev-3.jpg", FileMode.Open);    

            let! (conf1, user1) = identifyPhoto principal imgUser1 identPhotoApi

            let imgUser2 = new FileStream("./assets/max-2.png", FileMode.Open);    

            let! (conf2, user2) = identifyPhoto principal imgUser2 identPhotoApi

            // image for user must be correctly identified as image for user with high confidency
            user1.Id |> should equal user1Id
            conf1 |> should equal High

            user2.Id |> should equal user2Id
            conf2 |> should equal Medium

        } 
        |> tryFinally (fun () -> cleanForEnlistTest (orgId, user1Id, user2Id))
               
    }