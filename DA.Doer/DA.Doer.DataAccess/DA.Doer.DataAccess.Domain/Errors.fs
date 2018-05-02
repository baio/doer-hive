namespace DA.DataAccess.Domain.Errors

type ConnectionError = {
    message: string
}

type UniqueKeyError = {
    collection: string
    keys: string list
}

