namespace DA.DataAccess.Domain.Errors

type UniqueKeyError = {
    collection: string
    keys: string list
}

