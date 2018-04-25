module DA.FSX.Task

let ofResult<'a, 'b when 'b :> System.Exception > (result: Result<'a, 'b>) = 
    match result with Ok x -> FSharpx.Task.returnM x | Error e -> raise e

open System.Threading.Tasks
open FSharpx.Task
open System

// missed helpers for tasks

// type alias to distinguish between System Rersult and Task Result
type TaskResult<'a> = Result<'a>

let mapError f (m: Task<_>) =
    m.ContinueWith(
      fun (t: Task<_>) -> 
        if t.IsFaulted then            
            raise( f t.Exception.InnerException )
         else 
            t.Result
    )

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
