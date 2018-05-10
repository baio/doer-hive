module Setup

open DA.Doer.Mongo
open FSharpx.Task
open DA.FSX.ReaderTask
open DA.Doer.Users
open DA.HTTP.Blob



let mongoConfig = {
    connection = "mongodb://localhost"
    dbName = "doer-local"
}

let blobStorageConfig: BlobStorageConfig = {
    Uri = "http://127.0.0.1:10000/devstoreaccount1"
    AccountName = "devstoreaccount1"
    AccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="
    ContainerName = "test-images"
}

let blobContainer = getBlobContainer blobStorageConfig
