module DA.FSX.ReaderTask

// https://github.com/fsprojects/FSharpx.Extras/blob/master/src/FSharpx.Extras/ComputationExpressions/Monad.fs#L542-542

open Task
open FSharpx.Reader
open FSharpx.Task
open System.Threading.Tasks
open DA.FSX.Task

type ReaderTask<'e, 'a> = Reader<'e, Task<'a>>

let inline returnM x: ReaderTask<_, _> =
    reader { return returnM x }

let inline map f (m: ReaderTask<_, _>): ReaderTask<_, _> =
    FSharpx.Reader.map (map f) m

let inline mapc x = map(fun _ -> x)

let inline bind f (m: ReaderTask<_, _>): ReaderTask<_, _> =
    fun env ->
        bind (fun a -> (f a) env) (m env)

let inline run x (m: ReaderTask<_, _>) = (fun env -> run(fun () -> (m env))) x

let inline ofReader r = r >> FSharpx.Task.returnM

let inline ofTask t = FSharpx.Reader.returnM t

let inline ofException (ex): ReaderTask<_, _> = ex |> Task.FromException<_> |> ofTask

let ofResult x = x |> ofResult |> ofTask

// utility function mostly to read body from azure functions and create reader task immediately
let ofStream x = x |> IO.readString |> ofTask 

let inline mapError f (m: ReaderTask<_, _>): ReaderTask<_, _> =
    FSharpx.Reader.map (mapError f) m

let inline bindError f (m: ReaderTask<_, _>): ReaderTask<_, _> =
    fun env ->
        bindError (fun a -> (f a) env) (m env)

/// Infix map
let inline (<!>) f x = map f x

/// Infix bind
let inline (>>=) x f = bind f x

/// infix ap
let inline lift2 f a b = 
    a >>= fun aa -> b >>= fun bb -> f aa bb |> returnM

/// Sequential application
let inline ap x f = lift2 id f x

/// Sequential application
let inline (<*>) f x = ap x f

let inline ( <*) a b = lift2 (fun z _ -> z) a b

let inline ( *>) a b = lift2 (fun _ z -> z) a b

let inline (!>) a b = a *> returnM b

let inline (>=>) f g = fun x -> f x >>= g

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
