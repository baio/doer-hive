module DA.HTTP.Blob

open Microsoft.WindowsAzure.Storage.Auth
open System
open Microsoft.WindowsAzure.Storage

type StorageConfig = {
    Uri: string
    AccountName: string
    AccountKey: string
    ContainerName: string
}

let uploadStreamToStorage (config: StorageConfig) stream blobName =

        let storageCredentials = new StorageCredentials(config.AccountName, config.AccountKey)

        let uri = new Uri(config.Uri)
        // Create cloudstorage account by passing the storagecredentials
        let storageAccount = new CloudStorageAccount(storageCredentials, uri, uri, uri, uri)

        // Create the blob client.
        let blobClient = storageAccount.CreateCloudBlobClient()

        // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
        let container = blobClient.GetContainerReference(config.ContainerName)

        // Get the reference to the block blob from the container
        let blockBlob = container.GetBlockBlobReference(blobName)

        // Upload the file
        blockBlob.UploadFromStreamAsync(stream).ContinueWith(fun _ -> true)
