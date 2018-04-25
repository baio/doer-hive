module DA.FSX.ValidationResult

open FSharpx.Functional.Prelude


// analogoue to  http://fsprojects.github.io/FSharpx.Extras/reference/fsharpx-validation.html
// but with result and list

let inline alt r = function
    | Ok x -> Ok x
    | Error _ -> r
    
let inline (<|>) r x = alt r x

let choose x = x |> List.reduce(<|>)

let mapFail x = Result.mapError(fun a -> [a]) x

let mapFailLabeled label x = Result.mapError(fun a -> [label, a]) x

let apa append x f = 
    match f,x with
    | Ok f, Ok x         -> Ok (f x)
    | Error e, Ok _      -> Error e
    | Ok _, Error e      -> Error e
    | Error e1, Error e2 -> Error (append e1 e2)

let inline apm (m: _ FSharpx.ISemigroup) = apa (curry m.Combine)

let inline ap x = apa List.append x

let inline (<!>) f x = Result.map f x

let inline (<*>) f x = ap x f

let inline lift2 f a b = f <!> a <*> b

let inline ( *>) a b = lift2 (fun _ z -> z) a b

let traverse f list =

    // define a "cons" function
    let cons head tail = head :: tail

    // right fold over the list
    let initState = Ok []
    let folder head tail = 
        Ok cons <*> (f head) <*> tail

    List.foldBack folder list initState 

let sequence list = traverse id list

type Validator<'a> = string * ('a -> Result<string, string>)
let validatePayload<'a, 'b> (f: 'a -> 'b) (validators: Validator<'a> list) payload : Result<'b, (string * string) list> = 
    let x = Newtonsoft.Json.JsonConvert.DeserializeObject<'a>(payload)
    (fun _ -> f x) <!> 
        (
            (validators |> List.map(fun (a, b) -> x |> b  |> mapFailLabeled a))
            |> sequence
        )
