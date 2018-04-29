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
    idToken: string
    accessToken: string
    refreshToken: string
}

type RegisterUserResult = {
    userId: string
    tokens: TokensResult
}
