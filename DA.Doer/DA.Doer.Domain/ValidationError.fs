module DA.Doer.Domain.Errors

open System

type ValidationException(errors: (string * string) list) = 
    inherit Exception("Validation error")

    member this.errors = errors


let validationException x = ValidationException x



    