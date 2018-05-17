module DA.FSX.Task

open System.Threading
open System.Threading.Tasks
open FSharpx.Task
open FSharpx.Functional

open FSharp.Core
open System.Threading.Tasks
open System

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

let inline bindWithOptions (token: CancellationToken) (continuationOptions: TaskContinuationOptions) (scheduler: TaskScheduler) (f: 'T -> Task<'U>) (m: Task<'T>) =
    m.ContinueWith((fun (t: Task<_>) -> 
        if t.IsFaulted then Task.FromException<_>(t.Exception.InnerException) else f t.Result 
    ), token, continuationOptions, scheduler).Unwrap()


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
let sequence x = x |> List.map(fun t -> fun () -> t) |> FSharpx.Task.Parallel |> map Array.toList

type TaskBuilder(?continuationOptions, ?scheduler, ?cancellationToken) =
    
    let contOptions = defaultArg continuationOptions TaskContinuationOptions.None
    let scheduler = defaultArg scheduler TaskScheduler.Default
    let cancellationToken = defaultArg cancellationToken CancellationToken.None

    member this.Return x = returnM x

    member this.Bind(m, f) = bindWithOptions cancellationToken contOptions scheduler f m

    member this.Zero() : Task<unit> = this.Return ()

    member this.ReturnFrom (a: Task<'a>) = a

    member this.Run (body : unit -> Task<'a>) = body()

    member this.Delay (body : unit -> Task<'a>) : unit -> Task<'a> = fun () -> this.Bind(this.Return(), body)

    member this.Combine(t1:Task<unit>, t2 : unit -> Task<'b>) : Task<'b> = this.Bind(t1, t2)

    member this.While(guard, body : unit -> Task<unit>) : Task<unit> =
        if not(guard())
        then this.Zero()
        else this.Bind(body(), fun () -> this.While(guard, body))

    member this.TryWith(body : unit -> Task<'a>, catchFn:exn -> Task<'a>) : Task<'a> =
        let continuation (t:Task<'a>) : Task<'a> =
            if t.IsFaulted
            then catchFn(t.Exception.GetBaseException())
            else this.Return(t.Result)

        try body().ContinueWith(continuation).Unwrap()
        with e -> catchFn(e)

    member this.TryFinally(body : unit -> Task<'a>, compensation) : Task<'a> =
        let wrapOk (x:'a) : Task<'a> =
            compensation()
            this.Return x

        let wrapCrash (e:exn) : Task<'a> =
            compensation()
            reraise' e

        this.Bind(this.TryWith(body, wrapCrash), wrapOk)

    member this.Using(res:#IDisposable, body : #IDisposable -> Task<'a>) : Task<'a> =
        let compensation() =
            match res with
            | null -> ()
            | disp -> disp.Dispose()

        this.TryFinally((fun () -> body res), compensation)

    member this.For(sequence:seq<'a>, body : 'a -> Task<unit>) : Task<unit> =
        this.Using( sequence.GetEnumerator()
                    , fun enum -> this.While( enum.MoveNext
                                            , fun () -> body enum.Current
                                            )
                    )


let task = TaskBuilder()

let tryFinally (finalize: unit -> Task<_>) (body: Task<'x>) : Task<'x> = 
    task {        
        let! result = body |> map Choice1Of2 |> bindError(Choice2Of2 >> returnM)
        let! _ = finalize()
        return match result with
               | Choice1Of2 value -> value
               | Choice2Of2 exn -> raise exn
    }
