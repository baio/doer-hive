[<AutoOpen>]
module DA.Doer.Mongo.UserPhotoLinks

open MongoDB.Bson
open DA.FSX
open FSharpx.Reader
open DA.Doer.Mongo.API
open DA.DataAccess.Domain
open System
open MongoDB.Driver

let USER_PHOTO_LINKS_COLLECTION_NAME = "user_photo_links"

type UserPhotoLinkDoc = {
    Id: BsonObjectId
    UserId: string
    PhotoId: string
    Created: DateTime
}

let addUserPhotoLink userId photoId =
    let id = BsonObjectId(ObjectId.GenerateNewId())
    let doc = {
        Id = id
        UserId = userId
        PhotoId = photoId
        Created = System.DateTime.UtcNow
    }
    (insert doc <!> getCollection USER_PHOTO_LINKS_COLLECTION_NAME) |> ReaderTask.mapc(bsonId2String id)

let removeUserPhotoLinks userId config =
    let coll = getCollection USER_PHOTO_LINKS_COLLECTION_NAME config
    coll.DeleteManyAsync(fun x -> x.UserId = userId)
    
let getTopUserPhotoLinks (limit: int) userId config =
    let coll = getCollection USER_PHOTO_LINKS_COLLECTION_NAME config
    coll.Find(fun x -> x.UserId = userId)
        .Limit(Nullable.op_Implicit limit)
        .SortByDescending(fun x -> x.Created :> obj)
        .ToListAsync()
        |> DA.FSX.Task.map(List.ofSeq)
    