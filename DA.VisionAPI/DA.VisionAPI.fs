module DA.VisionAPI

open Microsoft.Cognitive.CustomVision
open System.IO
open System.Collections.Generic
open System
open DA.FSX.ReaderTask
open DA.FSX.Task

// TODO: projectId to TrainingConfig

// https://westus.dev.cognitive.microsoft.com/docs/services/56f91f2d778daf23d8ec6739/operations/56f91f2e778daf14a499e1fa

let createTrainingApi trainingKey = 
    let trainingCredentials = new TrainingApiCredentials(trainingKey)
    new TrainingApi(trainingCredentials)

let appendImagesToTag (tagId: Guid) (streams: Stream seq) (projectId: Guid) (trainingApi: TrainingApi) =    
    trainingApi.CreateImagesFromDataAsync(projectId, streams, new List<_>([tagId]))

let createTagWithImages (tagName: string) (streams: Stream seq) (projectId: Guid) (trainingApi: TrainingApi) =     
    trainingApi.CreateTagAsync(projectId, tagName)
    >>= fun tag -> (fun res -> (tag, res)) <!> (appendImagesToTag tag.Id streams projectId trainingApi)

let getTag (tagId: Guid) (projectId: Guid) (trainingApi: TrainingApi) =     
    trainingApi.GetTagAsync(projectId, tagId)

type CreateImageTag = 
    | CreateImageNewTag of string
    | CreateImageExistentTag of Guid

let createImages' (tag: CreateImageTag) (streams: Stream seq) (projectId: Guid) (trainingApi: TrainingApi) =
    match tag with
    | CreateImageNewTag      x -> 
        createTagWithImages x streams projectId trainingApi
        |> map (fun (x, y) -> (x.Id, y))
    | CreateImageExistentTag x -> 
        appendImagesToTag   x streams projectId trainingApi
        |> map (fun res -> (x, res))

let createImages (tag: CreateImageTag) (streams: Stream seq) (projectId: Guid) =
    readerTask {
        let! (tagId, result) = createImages' tag streams projectId        
        let! tag = getTag tagId projectId
        return (tagId, result, tag.ImageCount)
    }

let train (projectId: Guid) (trainingApi: TrainingApi) =                 
    trainingApi.TrainProjectAsync(projectId)
