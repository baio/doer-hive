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

type ReaderTaskBuilder() =

    member this.Return x = returnM x

    member this.Bind(m, f) = bind f m

    member this.Zero() : ReaderTask<_, unit> = this.Return ()

    member this.ReturnFrom (a: ReaderTask<_, 'a>) = a

    member this.Run (body : unit -> ReaderTask<_, 'a>) = body()

    member this.Delay (body : unit -> ReaderTask<_,'a>) : unit -> ReaderTask<_, 'a> = fun () -> this.Bind(this.Return(), body)

    member this.Combine(t1:ReaderTask<_, unit>, t2 : unit -> ReaderTask<_,'b>) : ReaderTask<_, 'b> = this.Bind(t1, t2)

    member this.While(guard, body : unit -> ReaderTask<_, unit>) : ReaderTask<_, unit> =
        if not(guard())
        then this.Zero()
        else this.Bind(body(), fun () -> this.While(guard, body))

let readerTask = ReaderTaskBuilder()
