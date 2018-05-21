module DA.Doer.Users.API.EnlistPhoto

open System.IO
open System.Threading.Tasks
open FSharpx.Task
open DA.FSX.Task
open System
open DA.Doer.Domain.Auth
open DA.Doer.Domain
open DA.Doer.Domain
open DA.Doer.Domain
open DA.Doer.Domain
open Exceptions

///
type OrgId            = string
type UserId            = string
type PrincipalId       = string
type TagId             = string
type SetId             = string
type FaceTokenId       = string

type UploadedPhotoParams = {
    OrgId            : OrgId
    UserId           : UserId
    FaceTokenId      : FaceTokenId
}
       
type Api = {    
    IsPrincipalAncestor     : PrincipalId -> UserId -> Task<bool>
    IsPhotoSetExists        : OrgId -> Task<bool>
    CreatePhotoSet          : SetId -> Task<bool>
    AddPhotoToSet           : SetId -> Stream -> Task<FaceTokenId>
    StorePhoto              : UploadedPhotoParams -> Stream -> Task<bool>
    MarkAsUploaded          : UploadedPhotoParams -> Task<int>
}

// TODO : Check same user as on perv, reset enlisted photos
let enlistPhoto (principal: Principal) userId stream api = 
    
    let principalId = principal.Id

    let orgId = principal.OrgId
    
    task {

        // validate principal is ancestor of user
        let! isPrincipalAncestor = api.IsPrincipalAncestor principalId userId

        if not isPrincipalAncestor then
            raise (new AccessDeniedException())
                    
        // get photo set (by principalId) and if it is already exists
        let! photoSetExists = api.IsPhotoSetExists principal.OrgId

        // create photo set if not exists, assign orgId as setId        
        let! _ = if not photoSetExists then api.CreatePhotoSet orgId else returnM true       

        // add photos to set
        let! faceTokenId = api.AddPhotoToSet orgId stream

        let uploadResult = { OrgId = orgId; UserId = userId; FaceTokenId = faceTokenId }

        // add photos to storage 
        let! _ = api.StorePhoto uploadResult stream 

        // mark user - photos added to set
        let! totalUserFacesCount = api.MarkAsUploaded uploadResult

        // return total number of photos for user
        return totalUserFacesCount
    }

    

