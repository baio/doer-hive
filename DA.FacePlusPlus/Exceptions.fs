[<AutoOpen>]
module DA.FacePlusPlus.Exceptions


open System

type FaceNotFoundException () =
    inherit Exception()

type MultipleFacesFoundException () =
    inherit Exception()
