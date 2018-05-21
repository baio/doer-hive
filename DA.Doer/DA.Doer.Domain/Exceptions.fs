module Exceptions

open System

type AccessDeniedException () =
    inherit Exception()
