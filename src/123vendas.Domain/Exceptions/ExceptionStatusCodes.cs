using FluentValidation;
using System.Net;

namespace _123vendas.Domain.Exceptions;

public static class ExceptionStatusCodes
{
    private static Dictionary<Type, HttpStatusCode> exceptionsStatusCodes = new()
        {
            {typeof(ServiceException), HttpStatusCode.InternalServerError},
            {typeof(NotFoundException), HttpStatusCode.NotFound},
            {typeof(EntityAlreadyDeletedException), HttpStatusCode.Conflict},
            {typeof(InvalidPaginationParametersException), HttpStatusCode.BadRequest},
            {typeof(ValidationException), HttpStatusCode.BadRequest},
            {typeof(SaleAlreadyCanceledException), HttpStatusCode.Conflict},
            {typeof(SaleItemAlreadyCanceledException), HttpStatusCode.Conflict},
            {typeof(ItemOutOfStockException), HttpStatusCode.Conflict},
            {typeof(ItemQuantityLimitExceededException), HttpStatusCode.Conflict},
            {typeof(UserAlreadyExistsException), HttpStatusCode.Conflict},
            {typeof(BadRequestException), HttpStatusCode.BadRequest},
            {typeof(EntityNotFoundException), HttpStatusCode.NoContent},
            {typeof(UnauthorizedUserException), HttpStatusCode.Unauthorized}
        };

    public static HttpStatusCode GetExceptionStatusCode(Exception exception)
    {
        bool exceptionFound = exceptionsStatusCodes.TryGetValue(exception.GetType(), out HttpStatusCode statusCode);
        return exceptionFound ? statusCode : HttpStatusCode.InternalServerError;
    }
}