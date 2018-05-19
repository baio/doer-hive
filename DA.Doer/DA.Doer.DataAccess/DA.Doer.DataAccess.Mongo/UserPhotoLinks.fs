[<AutoOpen>]
module DA.Doer.Mongo.UserPhotoLinks

open MongoDB.Bson
open DA.FSX
open DA.Doer.Mongo.API
open DA.DataAccess.Domain
open System
open MongoDB.Driver

let USER_PHOTO_LINKS_COLLECTION_NAME = "user_photo_links"

type UserPhotoLinkDocDTO = {
    id: BsonObjectId
    orgId: string
    userId: string
    photoId: string
    created: DateTime
}

let addUserPhotoLink orgId userId photoId =
    let id = BsonObjectId(ObjectId.GenerateNewId())
    let doc = {
        id = id
        orgId = orgId
        userId = userId
        photoId = photoId
        created = System.DateTime.UtcNow
    }
    getCollection USER_PHOTO_LINKS_COLLECTION_NAME 
    |> FSharpx.Reader.map (insert doc)
    |> ReaderTask.mapc(bsonId2String id)

let addUserPhotoLinks orgId userId photoIds =
    photoIds |> List.map (addUserPhotoLink orgId userId) |> ReaderTask.sequence

let getUserIdByPhotoId photoId config =
    let coll = getCollection USER_PHOTO_LINKS_COLLECTION_NAME config
    coll.Find(fun x -> x.photoId = photoId).Project(fun x -> x.userId) 
    |> firstOrException (USER_PHOTO_LINKS_COLLECTION_NAME, photoId)

let getUserPhotoLinksCount userId config =
    // let userId = bsonId userId
    let coll = getCollection USER_PHOTO_LINKS_COLLECTION_NAME config
    coll.Find(fun x -> x.userId = userId).CountAsync()

// Add user photos and then returns total links count for user
let addUserPhotoLinks' orgId userId photoIds = 
    ReaderTask.readerTask {
        let! _ = addUserPhotoLinks orgId userId photoIds
        let! count = getUserPhotoLinksCount userId
        return (int)count
    }

let removeUserPhotoLinks userId config =
    let coll = getCollection USER_PHOTO_LINKS_COLLECTION_NAME config
    coll.DeleteManyAsync(fun x -> x.userId = userId)

let orgHasPhotoLink orgId config =
    let coll = getCollection USER_PHOTO_LINKS_COLLECTION_NAME config
    coll.CountAsync(fun x -> x.orgId = orgId)   
    |> Task.map (fun x -> x > (int64)0)

(*    
let getTopUserPhotoLinks (limit: int) userId config =
    let coll = getCollection USER_PHOTO_LINKS_COLLECTION_NAME config
    coll.Find(fun x -> x.UserId = userId)
        .Limit(Nullable.op_Implicit limit)
        .SortByDescending(fun x -> x.Created :> obj)
        .ToListAsync()
        |> DA.FSX.Task.map(List.ofSeq)
*)
    