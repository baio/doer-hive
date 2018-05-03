namespace DA.FSX

module Pair = 

    type Pair<'a, 'b> = 'a * 'b
    
    let map f (m: Pair<_,_>)  = (fst m), (f (snd m))

    let bimap f1 f2 (m: Pair<_,_>)  = (f1 (fst m)), (f2 (snd m))

    let crossApply (m: Pair<_,_>)  = (snd m) (fst m)

module ListPair = 

    open Pair

    let map f = List.map (map f)

    let bimap f1 f2 = List.map (bimap f1 f2)

    let crossApply m = List.map (crossApply m)


