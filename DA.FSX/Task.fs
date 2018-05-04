module DA.FSX.Task

open System.Threading
open System.Threading.Tasks
open FSharpx.Task

open FSharp.Core
open System.Threading.Tasks

let returnM = returnM
// missed helpers for tasks

// type alias to distinguish between System Rersult and Task Result
type TaskResult<'a> = FSharpx.Task.Result<'a>

let inline ofException (ex): Task<_> = ex |> Task.FromException<_>

let ofResult<'a, 'b when 'b :> System.Exception > (result: Result<'a, 'b>) = 
    match result with 
    Ok x -> 
        returnM x 
    | Error e -> 
        ofException e

let mapError f (m: Task<_>) =
    m.ContinueWith(
      fun (t: Task<_>) -> 
        if t.IsFaulted then            
            Task.FromException<_>( f t.Exception.InnerException )
         else 
            t
    ).Unwrap()

// override FSharpx implementation
let  bind (f: _ -> Task<_>) (x: Task<_>) = 
    x.ContinueWith(fun (t: Task<_>) -> 
        if t.IsFaulted then Task.FromException<_>(t.Exception.InnerException) else f t.Result 
    ).Unwrap()

// override FSharpx implementation
let  map (f: _ -> _) (x: Task<_>) = 
    x.ContinueWith(fun (t: Task<_>) -> 
        if t.IsFaulted then Task.FromException<_>(t.Exception.InnerException) else (f t.Result) |> returnM
    ).Unwrap()

// of untyped task
let  ofTaskU (a: _) (x: Task) = 
    x.ContinueWith(fun (t: Task) -> 
        if t.IsFaulted then Task.FromException<_>(t.Exception.InnerException) else a |> returnM
    ).Unwrap()


let bindError f (m: Task<_>) =
    m.ContinueWith(
      fun (t: Task<_>) -> 
        if t.IsFaulted then            
            f t.Exception.InnerException 
         else 
            t
    ).Unwrap()


let inline (<!>) f m = map f m

let inline (>>=) x f = bind f x

let inline (>=>) f g = fun x -> f x >>= g

let inline ``const`` x = map(fun _ -> x) 

let inline (!>) m f = ``const`` (f()) m

let memoize f =

    let dict = System.Collections.Concurrent.ConcurrentDictionary()

    fun x -> task {

        match dict.TryGetValue x with
        | true, result -> return result
        | false, _ ->
            let! result = f x
            dict.TryAdd(x, result) |> ignore
            return result
    }

// parallel reserved so sequence the same for tasks
let sequence x = x |> List.map(fun t -> fun () -> t) |> FSharpx.Task.Parallel
