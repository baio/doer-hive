﻿namespace DA.Doer.Mongo

open MongoDB.Driver

type MongoConfig = {
    Connection: string
    DbName: string
}

type MongoApi = {
    Db: IMongoDatabase
}

module Errors =

    open MongoDB.Driver
    open DA.DataAccess.Domain.Errors
    open System

    let matchConnectionError (e: exn) = 
        match e with
        | :? System.TimeoutException as ex when ex.Message.Contains("A timeout occured after") ->
            Some { Message = ex.Message }
        | _ -> None

    let matchUniqueKeyError (e: exn) =
        match e with
        | :? MongoWriteException as ex ->
            match ex.InnerException with
            | :? MongoBulkWriteException as ex1 when ex1.WriteErrors.Item(0).Code = 11000 ->
                let err = ex1.WriteErrors.Item(0)
                // TODO: extract collection and key
                {
                    Collection = err.Message
                    Keys = [err.Message]
                } |> Some
            | _ -> None
        | _ -> None

    // TODO: field option 
    type NotFoundException (name, id) =
        inherit Exception(sprintf "Object %s with id %s not found" name id)

open Errors

[<AutoOpen>]
module Utils = 

    let getDb (config: MongoConfig) =
        let client = MongoClient(config.Connection)
        client.GetDatabase(config.DbName)

module API =

    open System.Threading.Tasks
    open MongoDB.Driver
    open FSharpx.Reader
    open MongoDB.Bson
    open DA.FSX.Task

    type DocWithId = { _id: BsonObjectId }

    // TODO: rename MongoApi
    type MongoReader<'a> = Reader<MongoApi, 'a>

    let inline bsonId2String (id:  BsonObjectId) = id.AsObjectId.ToString()

    let inline bsonId x = x |> ObjectId.Parse |> BsonObjectId

    let inline setter (x: string) a = 
        let field = FieldDefinition<_, _>.op_Implicit(x)
        Builders.Update.Set(field, a)

    let inline getter (x: string) a = 
        let field = FieldDefinition<_>.op_Implicit(x)
        Builders.Projection.Include(field)

    let inline filterEq (x: string) a  = 
        let field = FieldDefinition<_, _>.op_Implicit(x)
        Builders.Filter.Eq(field, a)

    let inline idFilter id = filterEq "_id" (bsonId id) 

    let getCollection<'a> name env =
        env.Db.GetCollection<'a>(name)

    let inline insert doc (x: IMongoCollection<'a>) =
        x.InsertOneAsync(doc) |> ofTaskU doc

    let inline update fr upd (x: IMongoCollection<'a>) =
        x.UpdateOneAsync(fr, upd)

    let inline remove id (x: IMongoCollection<'a>) =
        x.DeleteOneAsync(idFilter id)

    let firstOrNone (find: IFindFluent<_, _>) = 
        find.FirstOrDefaultAsync() |> map(fun x -> 
            if isNull (x :> obj) then
                Some x
            else 
                None
        )

    let firstOrException a (find: IFindFluent<_, _>) = 
        find.FirstOrDefaultAsync() |> map(fun x -> 
            if isNull (x :> obj) then
                raise (new NotFoundException(a))
            x
        )


