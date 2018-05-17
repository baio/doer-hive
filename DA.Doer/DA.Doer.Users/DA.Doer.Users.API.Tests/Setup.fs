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
            connection = x.[0]
            dbName = x.[1]
        },
        {   
            Uri = x.[2]
            AccountName = x.[3]
            AccountKey = x.[4]
            ContainerName = "user-photos"
        },
        {
            apiKey =    x.[5]
            apiSecret = x.[6]
        }

let mongoConfig, blobStorageConfig, faceppConfig = getConfig()

let blobContainer = getBlobContainer blobStorageConfig

let faceppApi = {
    config = faceppConfig
    http = DA.Http.HttpTask.HttpClient.httpClientRequest
}
