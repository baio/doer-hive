module DA.FacePlusPlus

open DA.FSX
open DA.FSX.HttpTask
open System.Threading.Tasks
open DA.FSX.ReaderTask

let private FACEPP_BASE_URL = "https://api-us.faceplusplus.com/facepp/v3/"

let private getUrl = sprintf "%s%s" FACEPP_BASE_URL

type FacePlusPlusConfig = {
    apiKey: string
    apiSecret: string
}

type FacePlusPlusApi = {
    config: FacePlusPlusConfig
    http: Request -> Task<string>
}

type API<'a> = ReaderTask<FacePlusPlusApi, 'a>

let private apiKeysPayload api = 
    [ ("api_key", api.config.apiKey); ("api_secret", api.config.apiSecret) ]

let private http<'a> api = api.http >> Task.map str2json<'a>

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
        url = getUrl "detect"
        httpMethod = HttpMethod.POST
        payload = ([("image_file", stream)], (apiKeysPayload api)) |> FormMultipartPayload 
        headers = []
        queryString = []
    }
    |> http api

type CreateFaceSetResponse = {
    face_count: int
}

// https://console.faceplusplus.com/documents/6329329
let createFaceSet faceSetId: API<CreateFaceSetResponse> = fun api ->
    {
        url = getUrl "faceset/create"
        httpMethod = HttpMethod.POST
        payload = (apiKeysPayload api)@[("outer_id", faceSetId)] |> FormPayload 
        headers = []
        queryString = []
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
        url = getUrl "faceset/addface"
        httpMethod = HttpMethod.POST
        payload = 
            (apiKeysPayload api)@[
                ("outer_id", faceSetId)
                ("face_tokens", faceTokens |> String.concat ",")
            ] |> FormPayload 
        headers = []
        queryString = []
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

type PhotoWithMultiFacesException (detectFaceResponse: DetectFaceResponse list) =
    inherit System.Exception()
    member this.detectFaceResponse = detectFaceResponse

let detectAndAddFaces' failWhenPhotoHaveMultiFaces faceSetId streams = 
    readerTask {
        let! detectResult = streams |> List.map detectFace |> sequence 
        let multiFaces = detectResult |> List.exists(fun x -> x.faces.Length > 1)
        if failWhenPhotoHaveMultiFaces && multiFaces then raise (PhotoWithMultiFacesException detectResult)
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
        url = getUrl "search"
        httpMethod = HttpMethod.POST
        payload = 
            (
                [("image_file", stream)],
                (apiKeysPayload api)@[("outer_id", faceSetId)]
            ) |> FormMultipartPayload 
        headers = []
        queryString = []
    }
    |> http api

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
        url = getUrl "removeface"
        httpMethod = HttpMethod.POST
        payload = 
            (apiKeysPayload api)@[
                ("outer_id", faceSetId)
                ("face_tokens", faceTokens)
            ]
            |> FormPayload 
        headers = []
        queryString = []
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
        url = getUrl "faceset/delete"
        httpMethod = HttpMethod.POST
        payload = 
            (apiKeysPayload api)@[
                ("outer_id", faceSetId)
                // 1 - error if set is not empty
                ("check_empty", "0")
            ]
            |> FormPayload 
        headers = []
        queryString = []
    }
    |> http api


