module Setup

open DA.Doer.Mongo
open FSharpx.Task
open DA.FSX.ReaderTask
open DA.Doer.Users
open DA.HTTP.Blob
open DA.FacePlusPlus

let getConfig () = 
    [
        "mongo:connection"
        "mongo:dbName"
        "blob:uri"
        "blob:accountName"
        "blob:accountKey"
        "facepp:apiKey"
        "facepp:apiSecret"

    ] 
    |> DA.AzureKeyVault.getConfigSync "azureKeyVault:name"
    |> fun x -> 
        {
            Connection = x.[0]
            DbName = x.[1]
        },
        {   
            Uri = x.[2]
            AccountName = x.[3]
            AccountKey = x.[4]
            ContainerName = "user-photos"
        },
        {
            ApiKey =    x.[5]
            ApiSecret = x.[6]
        }

let mongoConfig, blobStorageConfig, faceppConfig = getConfig()

let faceppApi = {
    Config = faceppConfig
    Http = DA.Http.HttpTask.HttpClient.httpClientRequest
}

let mongoApi = {
    Db = getDb mongoConfig
}

let blobApi = {
    Container = blobStorageConfig |> getBlobClient |> getBlobContainer "user-photos"
}
