module DA.Doer.Mongo.Orgs

open MongoDB.Bson
open DA.FSX
open FSharpx.Reader
open DA.Doer.Mongo.API
open DA.DataAccess.Domain

let ORGS_COLLECTION_NAME = "orgs"

    
type OrgDocIns = { 
    Id: BsonObjectId
    Name: string
    OwnerEmail: string
}

let createInsDoc (doc: OrgDoc) =    
    let id = BsonObjectId(ObjectId.GenerateNewId())
    { Id = id; Name = doc.Name; OwnerEmail = doc.OwnerEmail }

let createOrg (doc: OrgDoc) =    
    let insDoc = createInsDoc doc
    (insert insDoc <!> getCollection ORGS_COLLECTION_NAME) |> ReaderTask.mapc(insDoc.Id.AsObjectId.ToString())
   
let removeOrg (id: string) =    
    (remove id <!> getCollection ORGS_COLLECTION_NAME) |> ReaderTask.mapc(id)
   
