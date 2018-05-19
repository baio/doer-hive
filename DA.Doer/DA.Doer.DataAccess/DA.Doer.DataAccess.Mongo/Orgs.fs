[<AutoOpen>]
module DA.Doer.Mongo.Orgs

open MongoDB.Bson
open DA.FSX
open FSharpx.Reader
open DA.Doer.Mongo.API
open DA.DataAccess.Domain

let ORGS_COLLECTION_NAME = "orgs"

    
type OrgDocInsDTO = { 
    id: BsonObjectId
    name: string
    ownerEmail: string
}

let createInsDoc (doc: OrgDoc) =    
    let id = BsonObjectId(ObjectId.GenerateNewId())
    { id = id; name = doc.Name; ownerEmail = doc.OwnerEmail }

let createOrg (doc: OrgDoc) =    
    let insDoc = createInsDoc doc
    (insert insDoc <!> getCollection ORGS_COLLECTION_NAME) |> ReaderTask.mapc(insDoc.id.AsObjectId.ToString())
   
let removeOrg (id: string) =    
    (remove id <!> getCollection ORGS_COLLECTION_NAME) |> ReaderTask.mapc(id)
   
