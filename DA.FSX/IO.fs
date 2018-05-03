module DA.FSX.IO

open FSharpx.Task
open DA.FSX.Task
open Newtonsoft.Json
open System.IO

let readString (stream: System.IO.Stream) =

    use reader = new System.IO.StreamReader(stream)

    reader.ReadToEndAsync()

let jsonFromStream<'T> (stream: System.IO.Stream) =

    JsonConvert.DeserializeObject<'T> <!> readString stream

let copyStream f (srcStream: Stream) =
    let distStream = new MemoryStream()
    f distStream
    srcStream.Position <- (int64)0
    distStream.Position <- (int64)0
    distStream
    