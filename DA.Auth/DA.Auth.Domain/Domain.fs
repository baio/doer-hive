namespace DA.Auth.Domain

type LoginInfo = {
    Email: string
    Password: string
}

type CreateUserInfo = {
    UserId: string
    OrgId: string
    Name: string
    Email: string
    Password: string
    Avatar: string
    Role: string
}

type TokensResult = {
    IdToken: string
    AccessToken: string
    RefreshToken: string
}

type RegisterUserResult = {
    UserId: string
    Tokens: TokensResult
}
