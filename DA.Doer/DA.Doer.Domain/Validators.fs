module DA.Doer.Domain.Validators

open System
open System.Globalization
open System.Text.RegularExpressions
    
let private ifElse f err x =
    if f x then Ok x else Error err 
    
let inline (<!>) x f = Result.map f x
let inline (>>=) x f = Result.bind f x
let inline (>=>) f1 f2 = fun x -> x |> f1 >>= f2

//
let private NULL_ERROR = "NULL_ERROR"
let notNull (x: obj) = ifElse (isNull >> not) NULL_ERROR x
    
let private _isString (x: obj) = match x with | :? String -> true | _ -> false
let private NOT_STRING_ERROR = "NOT_STRING_ERROR"
let isString x = ifElse _isString NOT_STRING_ERROR x
    
let toString (x: obj) = x :?> string

type NotNullString = obj -> Result<string, string>
let notNullString: NotNullString = notNull >=> isString >=> (toString >> Ok) 

let NOT_EMPTY_ERROR = "NOT_EMPTY_ERROR"
let notEmpty = ifElse (String.IsNullOrEmpty >> not) NOT_EMPTY_ERROR
    
type NotEmptyString<'T> = obj -> Result<'T, string>
let notEmptyString: NotEmptyString<_> = notNullString >=> notEmpty

//
    
let private NOT_GUID_ERROR = "NOT_GUID_ERROR"
let isGuid = ifElse (Guid.TryParse >> fst) NOT_GUID_ERROR

let private domainMapper (mtch: Match) =

    // IdnMapping class with default property values.
    let idn = new IdnMapping()
    let domainName = mtch.Groups.[2].Value
    let asciiDomainName = idn.GetAscii(domainName);
    mtch.Groups.[1].Value + asciiDomainName
        

let private _isEmail (strIn: string) =

    // Use IdnMapping class to convert Unicode domain names.
    try 
        let str = Regex.Replace(strIn, @"(@)(.+)$", domainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200.))

        Regex.IsMatch(str,
            @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
            RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250.))
    with
    | _ -> false

  
let private NOT_EMAIL_ERROR = "NOT_EMAIL_ERROR"
let isEmail' = ifElse _isEmail NOT_EMAIL_ERROR
let isEmail x = x |> (notEmptyString >=> isEmail')

let private _isPhone (str: string) =
    try 
        Regex.IsMatch(str, "^(\+[0-9]{9})$")
    with
    | _ -> false

let private NOT_PHONE_ERROR = "NOT_PHONE_ERROR"
let isPhone' = ifElse _isPhone NOT_PHONE_ERROR
let isPhone x = x |> (notEmptyString >=> isPhone')

let private _isUrl (str: string) = 
    let f, uriResult = Uri.TryCreate(str, UriKind.Absolute)
    f && (uriResult.Scheme = Uri.UriSchemeHttp || uriResult.Scheme = Uri.UriSchemeHttps)

let private NOT_URL_ERROR = "NOT_URL_ERROR"
let isUrl' = ifElse _isUrl NOT_URL_ERROR
let isUrl x = x |> (notEmptyString >=> isUrl')

// TODO : check weak password
let private _isPassword (x: string) = true
let private NOT_PASSWORD_ERROR = "NOT_PASSWORD_ERROR"
let isPassword' = ifElse (_isPassword) NOT_PASSWORD_ERROR
let isPassword x = x |> (notEmptyString >=> isPassword')

// TODO : validate WebToken
let private _isWebToken (x: string) = true
let private NOT_WEB_TOKEN = "NOT_WEB_TOKEN"
let isWebToken' = ifElse (_isWebToken) NOT_WEB_TOKEN
let isWebToken x = x |> (notEmptyString >=> isWebToken')

//

let private isStrLen (len: int) (str: string) = str.Length <= len
    
let private STR_MID_LEN = 50
let private MID_STR_ERROR = "MID_STR_ERROR"
let isMidStr' = ifElse (isStrLen STR_MID_LEN) MID_STR_ERROR
let isMidStr x = x |> (notEmptyString >=> isMidStr')

let private STR_EXTRA_MID_LEN = 200
let private STR_EXTRA_MID_ERROR = "STR_EXTRA_MID_ERROR"
let isExtraMidStr' = ifElse (isStrLen STR_EXTRA_MID_LEN) STR_EXTRA_MID_ERROR
let isExtraMidStr x = x |> (notEmptyString >=> isExtraMidStr')
