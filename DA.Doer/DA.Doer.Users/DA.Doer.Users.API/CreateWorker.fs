namespace DA.Doer.Users.API.CreateWorker

type WorkerInfo = {
    FirstName: string
    MidName: string 
    LastName: string
    Email: string
    Phone: string
}

[<AutoOpen>]
module API = 

    open DA.Doer.Domain.Users
    open System.Threading.Tasks
    open DA.FSX.Task
    open DA.DataAccess.Domain
    open DA.Doer.Domain.Auth
    open Exceptions

    type Api = {    
        PrincipalAncestors: PrincipalId -> Task<string list>
        Insert: UserDoc  -> Task<UserId>
    }

    let createWorker (principal: Principal) (info: WorkerInfo) api =
    
        if principalCanCreateWorkers principal |> not then
            raise (AccessDeniedException())
    
        task {

            let! princiaplAncestors = api.PrincipalAncestors principal.Id
        
            let doc = {
                OrgId     = principal.OrgId
                Role      = roleToString Worker
                FirstName = info.FirstName
                MidName   = info.MidName
                LastName  = info.LastName
                Email     = info.Email
                Phone     = info.Phone
                Ancestors = principal.Id::princiaplAncestors
                Avatar    = null
            }
        
            return! api.Insert doc
        }


module Payload =
    
    let private norm (x: string) = if isNull x then null else x.ToLower();

    open DA.Doer.Domain.Validators
    open DA.FSX.ValidationResult
    open FSharpx.Reader
    open DA.Doer.Domain
    open System.IO
    
    type private DTO = { 
        firstName: obj
        middleName: obj
        lastName: obj
        email: obj
        phone: obj
    }

    let private mapPayload (dto: DTO): WorkerInfo =     
        {
            FirstName = dto.firstName :?> string |> norm
            MidName = dto.middleName :?> string |> norm
            LastName = dto.lastName :?> string |> norm
            Email = dto.email :?> string |> norm
            Phone = dto.phone :?> string |> norm
        }

    let private fromPayload'' = 
        [
            "firstName", fun x -> x.firstName |> isMidStr 
            "lastName", fun x ->  x.lastName |> isMidStr 
            "middleName", fun x -> lift2(<|>) isNull' isMidStr x.middleName
            "email", fun x -> x.email |> isEmail
            "phone", fun x -> x.phone |> isPhone
        ]
        |> validatePayload mapPayload

    let fromPayload' = 
        fromPayload'' >> Result.mapError(Errors.validationException)

    let fromPayload x = 
        x |> DA.FSX.IO.readString |> DA.FSX.Task.bind (fromPayload' >> DA.FSX.Task.ofResult)