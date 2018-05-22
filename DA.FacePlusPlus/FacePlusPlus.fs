[<AutoOpen>]
module DA.FacePlusPlus.Api

open DA.FSX
open DA.FSX.HttpTask
open System.Threading.Tasks
open DA.FSX.ReaderTask

let private FACEPP_BASE_URL = "https://api-us.faceplusplus.com/facepp/v3/"

let private getUrl = sprintf "%s%s" FACEPP_BASE_URL

type FacePlusPlusConfig = {
    ApiKey: string
    ApiSecret: string
}

type FacePlusPlusApi = {
    Config: FacePlusPlusConfig
    Http: Request -> Task<string>
}

type API<'a> = ReaderTask<FacePlusPlusApi, 'a>

let private apiKeysPayload api = 
    [ ("api_key", api.Config.ApiKey); ("api_secret", api.Config.ApiSecret) ]

let private http<'a> api = api.Http >> Task.map str2json<'a>

type DetectFaceRectangle = {
    width: int
    top: int
    left: int
    height: int
}

type DetectFace = {
    face_rectangle: DetectFaceRectangle
    face_token: string
}

type DetectFaceResponse = {
    faces: DetectFace list
    image_id: string
    request_id: string
    time_used: int
}

// https://console.faceplusplus.com/documents/5679127
// Returns face id
let detectFace stream: API<DetectFaceResponse> = fun api ->
    {
        Url = getUrl "detect"
        HttpMethod = HttpMethod.POST
        Payload = ([("image_file", stream)], (apiKeysPayload api)) |> FormMultipartPayload 
        Headers = []
        QueryString = []
    }
    |> http api

type CreateFaceSetResponse = {
    face_count: int
}

// https://console.faceplusplus.com/documents/6329329
let createFaceSet faceSetId: API<CreateFaceSetResponse> = fun api ->
    {
        Url = getUrl "faceset/create"
        HttpMethod = HttpMethod.POST
        Payload = (apiKeysPayload api)@[("outer_id", faceSetId)] |> FormPayload 
        Headers = []
        QueryString = []
    }
    |> http api

type AddFaceResponse = {
    request_id: string
    time_used: int
    face_count: int
    face_added: int
}

// https://console.faceplusplus.com/documents/6329371
let addFaces faceSetId faceTokens: API<AddFaceResponse> = fun api ->
    {
        Url = getUrl "faceset/addface"
        HttpMethod = HttpMethod.POST
        Payload = 
            (apiKeysPayload api)@[
                ("outer_id", faceSetId)
                ("face_tokens", faceTokens |> String.concat ",")
            ] |> FormPayload 
        Headers = []
        QueryString = []
    }
    |> http api

let detectAndAddFace faceSetId stream = 
    readerTask {
        let! detectResult = detectFace stream
        let faceTokens = detectResult.faces |> List.map(fun x -> x.face_token)
        let! addResult = addFaces faceSetId faceTokens
        return (detectResult, addResult)
    }


// Detect user faces, if more than just single person face detected on photo throw exception
(*
type PhotoWithMultiFacesException (detectFaceResponse: DetectFaceResponse list) =
    inherit System.Exception()
    member this.detectFaceResponse = detectFaceResponse
*)

let checkSingleFace' (faces: DetectFace list list) = 
    let fotoHasNoFace = faces |> List.exists(fun x -> x.Length = 0)
    if fotoHasNoFace then 
        raise (FaceNotFoundException())
    let fotoHasManyFaces = faces |> List.exists(fun x -> x.Length > 1)
    if fotoHasManyFaces then 
        raise (MultipleFacesFoundException())                    

let checkSingleFace = List.map(fun x -> x.faces) >> checkSingleFace'

let detectAndAddFaces' findSingle faceSetId streams = 
    readerTask {
        let! detectResult = streams |> List.map detectFace |> sequence         
        if findSingle then checkSingleFace detectResult
        let faceTokens = detectResult |> List.collect(fun x -> x.faces |> List.map(fun x -> x.face_token) )
        let! addResult = addFaces faceSetId faceTokens
        return (faceTokens, detectResult, addResult)
    }

let detectAndAddFaces x = x |> detectAndAddFaces' false

let detectAndAddSinglePersonFaces x = x |> detectAndAddFaces' true

type SearchFaceResult = {
    face_token: string
    confidence: float
    user_id: string
}

type SearchFaceResponse = {
    request_id: string
    results: SearchFaceResult list
    image_id: string
    time_used: int
    faces: DetectFace list
}

// https://console.faceplusplus.com/documents/5681455
let searchFace faceSetId stream: API<SearchFaceResponse> = fun api ->
    {
        Url = getUrl "search"
        HttpMethod = HttpMethod.POST
        Payload = 
            (
                [("image_file", stream)],
                (apiKeysPayload api)@[("outer_id", faceSetId)]
            ) |> FormMultipartPayload 
        Headers = []
        QueryString = []
    }
    |> http api

let searchSingleFace faceSetId stream: API<_> = 
    readerTask {
        let! searchFace = searchFace faceSetId stream
        checkSingleFace' [ searchFace.faces ]
        return searchFace.results.[0] |> fun x -> (x.confidence, x.face_token)
    }

type RemoveFacesResponse = {
    request_id: string
    faceset_token: string
    outer_id: string
    face_removed: int
    face_count: int
    time_used: int
}

// https://console.faceplusplus.com/documents/6329376
let removeFaces' faceSetId faceTokens: API<RemoveFacesResponse> = fun api ->
    {
        Url = getUrl "removeface"
        HttpMethod = HttpMethod.POST
        Payload = 
            (apiKeysPayload api)@[
                ("outer_id", faceSetId)
                ("face_tokens", faceTokens)
            ]
            |> FormPayload 
        Headers = []
        QueryString = []
    }
    |> http api

let removeFaces faceSetId = 
    String.concat "," >> removeFaces' faceSetId 

let removeAllFaces faceSetId = 
    removeFaces' faceSetId "RemoveAllFaceTokens"

type DeleteFacesetResponse = {
    request_id: string
    faceset_token: string
    outer_id: string
    time_used: int
}

// https://console.faceplusplus.com/documents/6329394
let deleteFaceset faceSetId: API<DeleteFacesetResponse> = fun api ->
    {
        Url = getUrl "faceset/delete"
        HttpMethod = HttpMethod.POST
        Payload = 
            (apiKeysPayload api)@[
                ("outer_id", faceSetId)
                // 1 - error if set is not empty
                ("check_empty", "0")
            ]
            |> FormPayload 
        Headers = []
        QueryString = []
    }
    |> http api


