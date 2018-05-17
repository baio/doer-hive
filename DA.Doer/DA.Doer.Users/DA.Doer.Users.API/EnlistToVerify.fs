module DA.Doer.Users.API.EnlistToVerify

open System.IO
open System.Threading.Tasks
open FSharpx.Task
open DA.FSX.Task
open System
open DA.Doer.Domain.Auth

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
    FaceTokenIds     : FaceTokenId list
}

///

type UserPhotosMinimalLimitException (requiredLength, currentLength) =
    inherit Exception()
    member this.requiredLength = requiredLength
    member this.currentLength = currentLength


type AccessDeniedException () =
    inherit Exception()
       
type API = {    
    isPrincipalAncestor     : PrincipalId -> UserId -> Task<bool>
    getUserPhotos           : UserId -> Task<Stream list>
    isPhotoSetExists        : OrgId -> Task<bool>
    createPhotoSet          : SetId -> Task<bool>
    addPhotosToSet          : SetId -> Stream list -> Task<FaceTokenId list>
    // must return total user number of user faces
    markAsUploaded          : UploadedPhotoParams -> Task<int>
}

let enlistToVerify (principal: Principal) userId api = 
    
    let principalId = principal.Id

    let orgId = principal.OrgId
    
    task {

        // validate principal is ancestor of user
        let! isPrincipalAncestor = api.isPrincipalAncestor principalId userId

        if not isPrincipalAncestor then
            raise (new AccessDeniedException())
                    
        // read user photos
        let! userPhotos = api.getUserPhotos userId

        // validate minimal required user photos quantity
        if userPhotos.Length < 1 then
            raise (new UserPhotosMinimalLimitException(1, userPhotos.Length))

        // get photo set (by principalId) and if it is already exists
        let! photoSetExists = api.isPhotoSetExists principal.OrgId

        // create photo set if not exists, assign orgId as setId        
        let! _ = if not photoSetExists then api.createPhotoSet orgId else returnM true       

        // add photos to set
        let! faceTokenIds = api.addPhotosToSet orgId userPhotos

        // mark user - photos added to set
        let! totalUserFacesCount = api.markAsUploaded({ OrgId = orgId; UserId = userId; FaceTokenIds = faceTokenIds })

        // return total number of photos for user
        return totalUserFacesCount
    }
        