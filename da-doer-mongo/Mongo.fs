module DA.Doer.Mongo.API

open MongoDB.Driver
open FSharpx.Reader
open MongoDB.Bson

type MongoConfig = {
    connection: string
    dbName: string
}

type DocWithId = { _id: BsonObjectId }

type MongoReader<'a> = Reader<MongoConfig, 'a>

let getCollection<'a> name env = 
    let client          = MongoClient(env.connection)
    let db              = client.GetDatabase(env.dbName)
    db.GetCollection<'a>(name)

let inline insert doc (x: IMongoCollection<'a>) = 
    x.InsertOneAsync(doc).ContinueWith(fun _ -> doc)

let inline remove id (x: IMongoCollection<'a>) = 
    x.DeleteOneAsync(fun (x: DocWithId) -> x._id =  BsonObjectId(ObjectId.Parse(id)))
