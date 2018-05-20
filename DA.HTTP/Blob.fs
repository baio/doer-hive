module DA.HTTP.Blob

open Microsoft.WindowsAzure.Storage.Auth
open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob
open System.IO

open DA.FSX.ReaderTask

type BlobStorageConfig = {
    Uri: string
    AccountName: string
    AccountKey: string
    ContainerName: string
}

type BlobApi = {
    Container: CloudBlobContainer
}

let getBlobClient config = 

    let storageCredentials = new StorageCredentials(config.AccountName, config.AccountKey)

    let uri = new Uri(config.Uri)
    // Create cloudstorage account by passing the storagecredentials
    let storageAccount = new CloudStorageAccount(storageCredentials, uri, uri, uri, uri)

    // Create the blob client.
    storageAccount.CreateCloudBlobClient()

let getBlobContainer name (client: CloudBlobClient) =
    
    // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
    client.GetContainerReference(name)

(*
let getBlobContainer config = 

    let storageCredentials = new StorageCredentials(config.AccountName, config.AccountKey)

    let uri = new Uri(config.Uri)
    // Create cloudstorage account by passing the storagecredentials
    let storageAccount = new CloudStorageAccount(storageCredentials, uri, uri, uri, uri)

    // Create the blob client.
    let blobClient = storageAccount.CreateCloudBlobClient()

    // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
    blobClient.GetContainerReference(config.ContainerName)
*)

let private normalizeUri (uri:string) =
    (*
        Remote devices can't access localhost, so we need special local address to where such devices
        could do requests.
        When image url returned, for such devices, it must have correct local address.
        Also proxy for this local address must be set 192.168.0.100:778 -> localhost:10000.
        Now device could access images by 192.168.0.100:778...
        All these addressess must be well known and correct.
        + localhost:10000 - address of local storage emulator
        + 192.168.0.100:778 - proxy to local storage emulator (must be set by nginx for example)
    *)

    #if DEBUG
        uri.Replace("localhost:10000", "192.168.0.100:778")
    #else 
        uri
    #endif

let uploadStreamToStorage blobName stream (api: BlobApi) =

        // Get the reference to the block blob from the container
        let blockBlob = api.Container.GetBlockBlobReference(blobName)

        // Upload the file
        blockBlob.UploadFromStreamAsync(stream).ContinueWith(fun _ -> 
            stream.Position <- (int64)0
            blockBlob.Uri.ToString() |> normalizeUri
        )

let getDateTimeFileName dir = sprintf "%s/%s" dir (DateTime.UtcNow.ToString("yyyyMMddHmmssfffff"))

let uploadStreamToStorageDirectoty dir = dir |> getDateTimeFileName |> uploadStreamToStorage
    
let removeBlobFromStorage blobName (api: BlobApi) =
    api.Container.GetBlockBlobReference(blobName).DeleteAsync().ContinueWith(fun _ -> true)

let removeBlobFromDirectory blobName (api: BlobApi) =
    api.Container.GetBlockBlobReference(blobName).DeleteAsync().ContinueWith(fun _ -> true)

let getBlob blobName (api: BlobApi) =
    
    // Get the reference to the block blob from the container
    let blockBlob = api.Container.GetBlockBlobReference(blobName)

    let stream = new MemoryStream()

    blockBlob.DownloadToStreamAsync(stream).ContinueWith(fun _ -> 
        stream.Position <- (int64)0
        stream :> Stream
    )


let getBlobs x = x |> List.map getBlob |> sequence

open DA.FSX.Task

let private getBlobNameFromUri (uri: Uri) = 
    uri.Segments |> Array.skip 3 |> String.concat ""

let private listBlobSegments limit dirName (api: BlobApi) =
    api.Container
        .GetDirectoryReference(dirName)
        .ListBlobsSegmentedAsync(
            true, 
            BlobListingDetails.None, 
            (match limit with | Some x -> Nullable.op_Implicit(x) | None -> System.Nullable()), 
            null, 
            null, 
            null
        )

let getDirectoryBlobNames limit dirName (api: BlobApi) =
    // blobs must be stored with names formatted as create date time 
    // https://stackoverflow.com/questions/5876519/azure-sort-order-of-list-operation-on-blob-container?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
    listBlobSegments limit dirName api
    |> map (fun x -> 
        x.Results 
        |> Seq.map(fun x -> x.Uri |> getBlobNameFromUri) 
        |> Seq.toList
    )

let getDirectoryBlobs limit dirName  = 
    readerTask {
        let! names = getDirectoryBlobNames limit dirName
        return! getBlobs names
    }


let removeBlobDirectory dirName =
    readerTask {
        let! names = getDirectoryBlobNames None dirName
        return! names |> List.map(removeBlobFromStorage) |> DA.FSX.ReaderTask.sequence
    }

    

    
