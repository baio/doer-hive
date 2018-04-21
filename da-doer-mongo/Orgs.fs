module DA.Doer.Orgs

open MongoDB.Bson
open DA.FSX
open FSharpx.Reader
open DA.Doer.Mongo

let ORGS_COLLECTION_NAME = "orgs"

type OrgDoc = {    
    Name: string
    OwnerEmail: string
}
    
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
    (insert insDoc <!> getCollection ORGS_COLLECTION_NAME) |> ReaderTask.mapc(id.ToString())
   
