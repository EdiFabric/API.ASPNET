using EdiNation.Models;
using Microsoft.AspNetCore.Mvc;

namespace EdiFabric.Api.ASPNET
{
    static class ErrorHandler
    {
        public static ObjectResult ToResponse(string message, int statusCode = 400 )
        {
            return new ObjectResult(new ErrorDetails { Code = statusCode, Details = new List<string> { message } })
            {
                StatusCode = statusCode,
            };
        }

        public static ObjectResult ToResponse(Exception ex)
        {
            return ToResponse(ex.Message, ex is InvalidDataException ? 400 : 500);
        }
    }
}
