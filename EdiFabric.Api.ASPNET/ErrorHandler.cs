using EdiFabric.Native.X12;
using Microsoft.AspNetCore.Mvc;

namespace EdiFabric.Api.ASPNET
{
    static class ErrorHandler
    {
        public static ObjectResult ToResponse(string message, int statusCode = 400)
        {
            return new ObjectResult(new ErrorDetails { Code = statusCode, Details = new List<string> { message } })
            {
                StatusCode = statusCode,
            };
        }

        public static ObjectResult ToResponse(Exception ex)
        {
            return ToResponse(ex.Message, StatusCodeFor(ex));
        }

        private static int StatusCodeFor(Exception ex)
        {
            if (ex is InvalidDataException)
                return 400;

            if (ex is EdiFabricException ediEx)
            {
                return ediEx.Code is
                    (int)EdiFabricErrorCode.IncorrectInput or
                    (int)EdiFabricErrorCode.MapNotSet or
                    (int)EdiFabricErrorCode.IncorrectMode or
                    (int)EdiFabricErrorCode.ConfigDeserialization or
                    (int)EdiFabricErrorCode.SplitSegmentIdMissing
                    ? 400
                    : 500;
            }

            return 500;
        }
    }
}
