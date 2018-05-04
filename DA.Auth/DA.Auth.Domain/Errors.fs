namespace DA.Auth.Domain.Errors

type NetworkErrorResponse = {
    Uri : System.Uri
}

type NetworkError = {
    Message    : string
    Response  : NetworkErrorResponse option
}

type UserAlreadyExists = UserAlreadyExists


