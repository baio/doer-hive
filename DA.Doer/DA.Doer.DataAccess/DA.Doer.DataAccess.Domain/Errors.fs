namespace DA.DataAccess.Domain.Errors

type ConnectionError = {
    Message: string
}

type UniqueKeyError = {
    Collection: string
    Keys: string list
}

