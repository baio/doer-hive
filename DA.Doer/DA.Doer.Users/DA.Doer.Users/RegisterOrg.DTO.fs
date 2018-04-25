module DA.Doer.Users.RegisterOrg.DTO

open System
open DA.Doer.Domain.Validators
open FSharpx.Nullable
open DA.FSX.ValidationResult

type UserNameDTO = {
    firstName: obj
    lastName: obj
    middleName: obj
}

type UserDTO = {
    name: UserNameDTO
    phone: obj
    email: obj
    avatar: obj
}

type OrgDTO = {
    name: obj
}

type RegisterOrgDTO = {
    user: UserDTO option
    org: OrgDTO
    password: String option
} 


let get f x = match x with null -> null | _ -> f x

let get2 f1 f2 = get f1 >> get f2

let get3 f1 f2 f3 = get f1 >> get f2 >> get f3

let private mapDTO dto = ()

let fromDTO (dto: RegisterOrgDTO) = 

    let getUser = Option.bind(fun x -> x.user)

    mapDTO <!> 
        //(dto.user |> get2 (fun x -> x.name) (fun x -> x.firstName) |> notNullString |> mapFail)
        (dto.password |> (notNullString >=> isPassword) |> mapFail) *>
        (dto.password |> (notNullString >=> isPassword) |> mapFail)


