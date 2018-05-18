module DA.Doer.Users.API.IdentifyPhoto

open System.IO
open System.Threading.Tasks
open DA.FSX.Task
open System
open DA.Doer.Domain.Auth
open DA.Doer.Domain.Users


type OrgId          = string
type FaceTokenId    = string

type Confidence = High | Medium | Low | Uncertain
       
type API = {    
    IdentifyPhoto : OrgId -> Stream -> Task<float * FaceTokenId>
    FindUser      : FaceTokenId -> Task<User>
}

let private pointsToConfidence (points: float) = 
    if points > 90. then
        High
    elif points > 75. then 
        Medium
    elif points > 60. then    
        Low
    else
        Uncertain

let identifyPhoto (principal: Principal) stream (api: API) = 

    let orgId = principal.OrgId

    task {
        let! (points, faceTokenId) = api.IdentifyPhoto orgId stream
        let confidence = pointsToConfidence points
        let! user = api.FindUser faceTokenId
        return (confidence, user)
    }
    