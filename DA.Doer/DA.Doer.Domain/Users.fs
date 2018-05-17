﻿namespace DA.Doer.Domain.Users

type UserRole = Owner | Master | Worker

type User = {
    Id: string
    OrgId: string
    Role: UserRole
    FirstName: string
    MidName: string 
    LastName: string
    Email: string
    Phone: string
    Ancestors: string seq
    Avatar: string
}

[<AutoOpenAttribute>]
module Helpers =
    
    let roleFromString = function 
        | "Owner" -> Owner
        | "Master" -> Master
        | "Worker" -> Worker
        | _ as x -> failwith (sprintf "Unknow user role [%s]" x)

    let roleToString = function 
        | Owner -> "Owner"
        | Master -> "Master"
        | Worker -> "Worker"
