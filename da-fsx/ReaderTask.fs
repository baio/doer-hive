module DA.FSX.ReaderTask

// https://github.com/fsprojects/FSharpx.Extras/blob/master/src/FSharpx.Extras/ComputationExpressions/Monad.fs#L542-542

open FSharpx.Reader
open FSharpx.Task
open System.Threading.Tasks
open Task

type ReaderTask<'e, 'a> = Reader<'e, Task<'a>>

let inline returnM x: ReaderTask<_, _> =
    reader { return returnM x }

let inline map f (m: ReaderTask<_, _>): ReaderTask<_, _> =
    FSharpx.Reader.map (map f) m

let inline bind f (m: ReaderTask<_, _>): ReaderTask<_, _> =
    fun env ->
        bind (fun a -> (f a) env) (m env)

let inline run x (m: ReaderTask<_, _>) = (fun env -> run(fun () -> (m env))) x

let inline ofReader r = r >> FSharpx.Task.returnM

let inline ofTask t = FSharpx.Reader.returnM t

let inline ofException (ex): ReaderTask<_, _> = ex |> Task.FromException<_> |> ofTask

let inline mapError f (m: ReaderTask<_, _>): ReaderTask<_, _> =
    FSharpx.Reader.map (mapError f) m

/// Infix map
let inline (<!>) f x = map f x
