[<AutoOpen>]
module DA.Drawing.IamgeSharp.ResizeImage

open DA.FSX.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Processing
open SixLabors.ImageSharp.Processing.Transforms
open System.IO

let resizeImage srcStream (width, height)  = 
    let image = Image.Load(stream = srcStream, decoder = SixLabors.ImageSharp.Formats.Jpeg.JpegDecoder())
    image.Mutate(fun x -> x.Resize(width = width, height = height) |> ignore)
    let distStream = new MemoryStream()
    image.SaveAsJpeg(distStream)
    srcStream.Position <- (int64)0
    distStream.Position <- (int64)0
    distStream :> Stream




