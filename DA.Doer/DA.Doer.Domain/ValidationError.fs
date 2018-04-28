module DA.Doer.Domain.Errors

open System

type ValidationException(errors: (string * string) list) = 
    inherit Exception("Validation error")

    member this.Errors = errors
        

let validationException x = ValidationException x


type ValidationError = ValidationError of (string * string) list

let matchValidationError (ex: exn) = 
        match ex with
        | :? ValidationException as ex ->
            Some (ex.Errors)
        | _ ->
            None
    