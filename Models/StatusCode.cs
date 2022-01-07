public enum Status
{
    OK = 200,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    PreconditionFailed = 412,
    SystemError = 500,
// --------------------------//
    TokenInvalid = 1,
    TokenExpired = 2,
    TokenNotFound = 3,
    TokenChanged = 4,

}