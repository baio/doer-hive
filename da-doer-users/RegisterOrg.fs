module DA.Doer.Users.RegisterOrg

open DA.Doer.Users.API
open DA.Doer.Users.RegisterUser
open DA.Doer.Mongo

// collide the worlds!

let getDataAccess config = {
    insertDoc = function
        | User doc -> Users.createUser doc config
        | Org doc -> Orgs.createOrg doc config            
}

//

(*
open DA.Auth0.API

let getAuth token config = {
    registerUser = DA.Auth0.API.
}
*)


let registerOrg token info = 
    API.registerOrg info
