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
    IsPrincipalAncestor     : PrincipalId -> UserId -> Task<bool>
    GetUserPhotos           : UserId -> Task<Stream list>
    IsPhotoSetExists        : OrgId -> Task<bool>
    CreatePhotoSet          : SetId -> Task<bool>
    AddPhotosToSet          : SetId -> Stream list -> Task<FaceTokenId list>
    // must return total user number of user faces
    MarkAsUploaded          : UploadedPhotoParams -> Task<int>
}

let enlistToVerify (principal: Principal) userId api = 
    
    let principalId = principal.Id

    let orgId = principal.OrgId
    
    task {

        // validate principal is ancestor of user
        let! isPrincipalAncestor = api.IsPrincipalAncestor principalId userId

        if not isPrincipalAncestor then
            raise (new AccessDeniedException())
                    
        // read user photos
        let! userPhotos = api.GetUserPhotos userId

        // validate minimal required user photos quantity
        if userPhotos.Length < 1 then
            raise (new UserPhotosMinimalLimitException(1, userPhotos.Length))

        // get photo set (by principalId) and if it is already exists
        let! photoSetExists = api.IsPhotoSetExists principal.OrgId

        // create photo set if not exists, assign orgId as setId        
        let! _ = if not photoSetExists then api.CreatePhotoSet orgId else returnM true       

        // add photos to set
        let! faceTokenIds = api.AddPhotosToSet orgId userPhotos

        // mark user - photos added to set
        let! totalUserFacesCount = api.MarkAsUploaded({ OrgId = orgId; UserId = userId; FaceTokenIds = faceTokenIds })

        // return total number of photos for user
        return totalUserFacesCount
    }
        