module DA.FSX.IO

open FSharpx.Task
open DA.FSX.Task
open Newtonsoft.Json

let readString (stream: System.IO.Stream) =

    use reader = new System.IO.StreamReader(stream)

    reader.ReadToEndAsync()

let jsonFromStream<'T> (stream: System.IO.Stream) =

    JsonConvert.DeserializeObject<'T> <!> readString stream

