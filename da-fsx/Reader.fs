module DA.FSX.Reader
open FSharpx.Reader

let flat (x: Reader<'a, Reader<'b, 'c>>): Reader<'a * 'b, 'c> = fun (a, b) -> (x a) b

