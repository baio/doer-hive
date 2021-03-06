﻿module EnlistPhotoTests

open System
open Xunit
//open FSharpx.Task
open DA.Doer.Users.API.EnlistPhoto
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

        IsPhotoSetExists = fun principalId -> returnM true
        
        CreatePhotoSet = fun setId -> returnM true

        AddPhotoToSet = fun setId treams -> returnM "777"

        StorePhoto = fun x stream -> returnM true

        MarkAsUploaded = fun x -> returnM 1
    }


[<Fact>]
let ``Enlist photo with mock api must work`` () =

    let principal: Principal = {
        Id = "test-principal"
        OrgId = "test-org"
        Role = "Owner"
    }

    let stream = (new MemoryStream(buffer = Encoding.UTF8.GetBytes("123")) :> Stream) 

    enlistPhoto principal "test-user" stream mockApi


let semiMockApi = 
    {
        IsPrincipalAncestor = fun principalId userId -> returnM true

        IsPhotoSetExists = fun orgId -> orgHasPhotoLink orgId mongoApi
        
        CreatePhotoSet = fun setId -> 
            createFaceSet setId faceppApi |> ``const`` true
            //returnM true

        AddPhotoToSet = fun setId stream -> 
            detectAndAddSinglePersonFaces setId [stream] faceppApi |> map( fun (x, _, _) -> x.[0] )
            // returnM "100"

        StorePhoto = fun x stream -> 
            uploadStreamToStorageDirectoty x.UserId stream blobApi |> ``const`` true

        MarkAsUploaded = fun x -> 
            addUserPhotoLinks' x.OrgId x.UserId [x.FaceTokenId] mongoApi

    }

let identPhotoApi  = 
    {
        IdentifyPhoto = fun orgId stream -> 
            searchFace orgId stream faceppApi 
                |> map(fun x -> 
                    // TODO : throw exception if nothing found ?
                    (x.results.[0].confidence, x.results.[0].face_token)
                 )
        FindUser = fun faceTokenId -> getUserByPhotoId faceTokenId mongoApi
        StorePhoto = fun x stream -> returnM ()
    }

let setupForEnlistTest () =    

    task {

        let! orgId = createOrg { Name = "test-org-enlist-photo"; OwnerEmail = "test-org-enlist-photo@mail.ru" } mongoApi

        let userDoc1: UserDoc =  {
            OrgId = orgId
            Role = "Owner"
            FirstName = "first"
            MidName = "user"
            LastName = "name"
            Email = "first_user_name_enlist_photo@gmail.com"
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
            Email = "second_user_name_enlist_photo@gmail.com"
            Phone = "+79772753595"
            Ancestors = []
            Avatar = ""
        } 
       
        // create user 1
        let! user1Id = createUser userDoc1 mongoApi
        
        // create user 2
        let! user2Id = createUser userDoc2 mongoApi
                        
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
let ``Enlist photo with real api must work`` () =
           
    task {

        let! orgId, user1Id, user2Id = setupForEnlistTest()

        let principal = {
                Id    = user1Id
                OrgId = orgId
                Role = "Owner"
            }

        use user1Photo = readFile "./assets/lev-1.jpg"
        
        use user2Photo = readFile "./assets/max-1.jpg"

        return! task {                

            let! _ = enlistPhoto principal user1Id user1Photo semiMockApi
            
            let! _ = enlistPhoto principal user2Id user2Photo semiMockApi
        
            use imgUser1 = readFile "./assets/lev-3.jpg"

            let! (conf1, user1) = identifyPhoto principal imgUser1 identPhotoApi

            use imgUser2 = readFile "./assets/max-2.png"

            let! (conf2, user2) = identifyPhoto principal imgUser2 identPhotoApi
            
            // image for user must be correctly identified as image for user with high confidency
            user1.Id |> should equal user1Id
            conf1 |> should equal Medium

            user2.Id |> should equal user2Id
            conf2 |> should equal Medium
        } 
        |> tryFinally (fun () -> cleanForEnlistTest (orgId, user1Id, user2Id))
               
    }

[<Fact>]
let ``Enlist foto with 2 pepople must throw exception`` () =
           
    task {

        let! orgId, user1Id, user2Id = setupForEnlistTest()

        let principal = {
                Id    = user1Id
                OrgId = orgId
                Role = "Owner"
            }

        use userPhoto = readFile "./assets/max-lev.jpg"
        
        return! task {                

            let! _ = enlistPhoto principal user1Id userPhoto semiMockApi
                    
            true |> should equal false
        } 
        |> bindError(fun err -> 
            Assert.IsType(typeof<MultipleFacesFoundException>, err)
            returnM ()
        )
        |> tryFinally (fun () -> cleanForEnlistTest (orgId, user1Id, user2Id))
               
    }

[<Fact>]
let ``Enlist foto with no person must throw exception`` () =
           
    task {

        let! orgId, user1Id, user2Id = setupForEnlistTest()

        let principal = {
                Id    = user1Id
                OrgId = orgId
                Role = "Owner"
            }

        use userPhoto = readFile "./assets/ford.jpeg"
        
        return! task {                

            let! _ = enlistPhoto principal user1Id userPhoto semiMockApi
                    
            true |> should equal false
        } 
        |> bindError(fun err -> 
            Assert.IsType(typeof<FaceNotFoundException>, err)
            returnM ()
        )
        |> tryFinally (fun () -> cleanForEnlistTest (orgId, user1Id, user2Id))
               
    }