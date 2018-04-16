module DA.FSX.Task

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

