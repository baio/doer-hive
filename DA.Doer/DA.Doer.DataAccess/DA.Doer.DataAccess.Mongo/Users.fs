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

type UserDocDTO = {
    id: BsonObjectId
    orgId: string
    role: string
    firstName: string
    midName: string 
    lastName: string
    email: string
    phone: string
    ancestors: string seq
    avatar: string
}

let createInsDoc (doc: UserDoc) =    
    let id = BsonObjectId(ObjectId.GenerateNewId())
    { 
        id = id
        orgId = doc.OrgId
        role = doc.Role
        firstName = doc.FirstName
        midName = doc.MidName
        lastName = doc.LastName
        email = doc.Email
        phone = doc.Phone
        ancestors = doc.Ancestors
        avatar = doc.Avatar
    }

let createUser doc =    
    let insDoc = createInsDoc doc
    (insert insDoc <!> getCollection USERS_COLLECTION_NAME) |> ReaderTask.mapc(insDoc.id.AsObjectId.ToString())

let removeUser (id: string) =    
    (remove id <!> getCollection USERS_COLLECTION_NAME) |> ReaderTask.mapc(id)

let removeUserData (id: string) =    
    ReaderTask.readerTask {
        let! _ = removeUserPhotoLinks id
        return! removeUser id
    }

let updateUserAvatar id url =  
    let fr = idFilter id
    let upd = setter "avatar" url
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
    coll.Find<UserDocDTO>(idFilter userId)
    |> firstOrException (USERS_COLLECTION_NAME, userId)
    |> Task.map(
        fun (x: UserDocDTO) -> 
        ({
            Id = bsonId2String x.id
            OrgId = x.orgId
            Role = roleFromString x.role
            FirstName = x.firstName
            MidName = x.midName
            LastName = x.lastName
            Email = x.email
            Phone = x.phone
            Ancestors = x.ancestors
            Avatar = x.avatar
        }: User)
    )

let getUserByPhotoId photoId = 
    ReaderTask.readerTask {
        let! userId = getUserIdByPhotoId photoId   
        return! getUser userId
    }