module DA.Doer.Mongo.API

open MongoDB.Driver
open FSharpx.Reader

type MongoConfig = {
    connection: string
    dbName: string
}

type MongoReader<'a> = Reader<MongoConfig, 'a>

let getCollection<'a> name env = 
    let client          = MongoClient(env.connection)
    let db              = client.GetDatabase(env.dbName)
    db.GetCollection<'a>(name)

let inline insert doc (x: IMongoCollection<'a>) = 
    x.InsertOneAsync(doc).ContinueWith(fun _ -> doc)
