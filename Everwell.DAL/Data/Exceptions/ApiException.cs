using System.Net;

namespace Everwell.DAL.Data.Exceptions;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ApiException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(message, HttpStatusCode.NotFound)
    {
    }
}

public class BadRequestException : ApiException
{
    public BadRequestException(string message)
        : base(message, HttpStatusCode.BadRequest)
    {
    }
}

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message)
        : base(message, HttpStatusCode.Unauthorized)
    {
    }
}
public class BusinessException : ApiException
{
    public BusinessException(string message)
        : base(message, HttpStatusCode.BadRequest) // or another appropriate status code
    {
    }
}

public class ValidationException : ApiException
{
    public ValidationException(string message)
        : base(message, HttpStatusCode.UnprocessableEntity) // or another appropriate status code
    {
    }
}

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message)
        : base(message, HttpStatusCode.Forbidden)
    {
    }
}