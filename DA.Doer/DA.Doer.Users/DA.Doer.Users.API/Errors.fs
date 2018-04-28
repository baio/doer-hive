module DA.Doer.Users.Errors


let uniqueKeyUnexpected _ = 
    409, """{ "userAlreadyExists" : true, "unexpected": true }"""

let uniqueKey _ = 
    409, """{ "userAlreadyExists" : true }"""

let mapValidationErrors errs =
    errs
    |> List.map(FSharpx.Reader.lift2(sprintf """ "%s" : "%s" """) fst snd)
    |> String.concat ","
    |> sprintf
            """
                { "validationErrors" : { %s } }
            """
let validation e = 
    400, mapValidationErrors e


let unexepcted (_: exn) = 500, ""
