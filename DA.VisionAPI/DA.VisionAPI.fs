module DA.VisionAPI

open Microsoft.Cognitive.CustomVision
open System.IO
open System.Collections.Generic
open System
open System

let createTrainingApi trainingKey = 
    let trainingCredentials = new TrainingApiCredentials(trainingKey)
    new TrainingApi(trainingCredentials)

let uploadStreams (tag: string) (streams: Stream seq) (projectId: string) (trainingApi: TrainingApi) = 
    
    let projectId = Guid.Parse(projectId) 
                
    let tag = trainingApi.CreateTag(projectId, tag)

    let tags = new List<_>([tag.Id])

    trainingApi.CreateImagesFromDataAsync(projectId, streams, tags)
    
let train (projectId: Guid) (trainingApi: TrainingApi) = 
                
    trainingApi.TrainProjectAsync(projectId)
