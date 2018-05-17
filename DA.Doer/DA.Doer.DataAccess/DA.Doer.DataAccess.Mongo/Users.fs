[<AutoOpen>]
module DA.Doer.Mongo.Users

open MongoDB.Bson
open DA.FSX
open FSharpx.Reader
open DA.Doer.Mongo.API
open DA.DataAccess.Domain
open MongoDB.Driver
open System
open DA.Doer.Domain.Users

// users

let USERS_COLLECTION_NAME = "users"

type UserDocWithId = {
    Id: BsonObjectId
    OrgId: string
    Role: string
    FirstName: string
    MidName: string 
    LastName: string
    Email: string
    Phone: string
    Ancestors: string seq
    Avatar: string
}

let createInsDoc (doc: UserDoc) =    
    let id = BsonObjectId(ObjectId.GenerateNewId())
    { 
        Id = id
        OrgId = doc.OrgId
        Role = doc.Role
        FirstName = doc.FirstName
        MidName = doc.MidName
        LastName = doc.LastName
        Email = doc.Email
        Phone = doc.Phone
        Ancestors = doc.Ancestors
        Avatar = doc.Avatar
    }

let createUser doc =    
    let insDoc = createInsDoc doc
    (insert insDoc <!> getCollection USERS_COLLECTION_NAME) |> ReaderTask.mapc(insDoc.Id.AsObjectId.ToString())

let removeUser (id: string) =    
    (remove id <!> getCollection USERS_COLLECTION_NAME) |> ReaderTask.mapc(id)

let removeUserData (id: string) =    
    ReaderTask.readerTask {
        let! _ = removeUserPhotoLinks id
        return! removeUser id
    }

let updateUserAvatar id url =  
    let fr = idFilter id
    let upd = setter "Avatar" url
    (update fr upd <!> getCollection USERS_COLLECTION_NAME) 
    |> ReaderTask.mapc(true)

type MetaPhotoValidator = {
    LatestUploadTime: DateTime
    PhotosCount: int
    TagId: string
}
(*
let markAsUploadedForTraining id tagId count =  
    let fr = idFilter id
    let upd = setter "Meta.PhotoValidator" { LatestUploadTime = DateTime.UtcNow; PhotosCount = count; TagId = tagId }
    (update fr upd <!> getCollection USERS_COLLECTION_NAME) 
    |> ReaderTask.mapc(true)

// TODO : remove foto validator meta use links, links required orgId field
type UserMeta = {
    PhotoValidator: MetaPhotoValidator
}

type UserWithMeta = {
    Id: BsonObjectId
    OrgId: BsonObjectId
    Meta: UserMeta
}

let someUserHasTrainingPhotoSet orgId config =  
    let coll = getCollection USERS_COLLECTION_NAME config
    let orgId = bsonId orgId
    // if organization has one user with already defined PhotoValiadtor field, consider photo set already created
    coll.Find<UserWithMeta>(fun x -> x.OrgId = orgId && x.Meta.PhotoValidator.PhotosCount >= 0)
        .Project(fun x -> x.Meta.PhotoValidator.TagId)
        .SingleOrDefaultAsync()
        |> DA.FSX.Task.map(fun x -> if isNull x then false else true)
*)

let getUser userId config = 
    let coll = getCollection USERS_COLLECTION_NAME config
    coll.Find<UserDocWithId>(idFilter userId)
    |> firstOrException (USERS_COLLECTION_NAME, userId)
    |> Task.map(
        fun (x: UserDocWithId) -> 
        ({
            Id = bsonId2String x.Id
            OrgId = x.OrgId
            Role = roleFromString x.Role
            FirstName = x.FirstName
            MidName = x.MidName
            LastName = x.LastName
            Email = x.Email
            Phone = x.Phone
            Ancestors = x.Ancestors
            Avatar = x.Avatar
        }: User)
    )

let getUserByPhotoId photoId = 
    ReaderTask.readerTask {
        let! userId = getUserIdByPhotoId photoId   
        return! getUser userId
    }