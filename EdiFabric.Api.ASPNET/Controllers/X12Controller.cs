using System.Text;
using EdiFabric.Api.ASPNET.Models;
using EdiFabric.Native.X12;
using Microsoft.AspNetCore.Mvc;

namespace EdiFabric.Api.ASPNET.Controllers
{
    [Route("x12")]
    [ApiController]
    public class X12Controller : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly string _apiKey;
        private readonly string _noData = "No data in request body.";

        public X12Controller(ILogger<X12Controller> logger, IConfiguration configuration)
        {
            _logger = logger;
            _apiKey = configuration["ApiKey"] ?? string.Empty;
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("No ApiKey configuration in appsettings.json.");
        }

        [Route("read")]
        [HttpPost]
        [Consumes("application/octet-stream", "text/plain")]
        public async Task<IActionResult> Read([FromQuery] ReadParameters readParameters)
        {
            if (Request.ContentLength == 0 || Request.Body == null)
            {
                _logger.LogError(_noData);
                return ErrorHandler.ToResponse(_noData);
            }

            try
            {
                Authorize();
                var edi = await ReadBodyAsync(readParameters.CharSet);
                var result = EdiFabricX12.Parse(edi, ParseMode.Json);
                return Content(result.Transactions, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return ErrorHandler.ToResponse(ex);
            }
        }

        [Route("write")]
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Write([FromQuery] WriteParameters writeParameters)
        {
            if (Request.ContentLength == 0 || Request.Body == null)
            {
                _logger.LogError(_noData);
                return ErrorHandler.ToResponse(_noData);
            }

            try
            {
                Authorize();
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var json = await reader.ReadToEndAsync();
                var edi = EdiFabricX12.Build(json, writeParameters.Postfix);
                var result = new MemoryStream(Encoding.UTF8.GetBytes(edi));
                return File(result, writeParameters.ToContentType());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return ErrorHandler.ToResponse(ex);
            }
        }

        [Route("validate")]
        [HttpPost]
        [Consumes("application/octet-stream", "text/plain", "application/json")]
        public async Task<IActionResult> Validate([FromQuery] ValidateParameters validateParameters)
        {
            if (Request.ContentLength == 0 || Request.Body == null)
            {
                _logger.LogError(_noData);
                return ErrorHandler.ToResponse(_noData);
            }

            try
            {
                Authorize();
                var edi = await ReadEdiAsync();
                var result = EdiFabricX12.Parse(edi, ParseMode.JsonValidate, validateParameters.ToConfig());
                return Content(result.Report, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return ErrorHandler.ToResponse(ex);
            }
        }

        [Route("ack")]
        [HttpPost]
        [Consumes("application/octet-stream", "text/plain", "application/json")]
        public async Task<IActionResult> Ack([FromQuery] AckParameters ackParameters)
        {
            if (Request.ContentLength == 0 || Request.Body == null)
            {
                _logger.LogError(_noData);
                return ErrorHandler.ToResponse(_noData);
            }

            try
            {
                Authorize();
                var edi = await ReadEdiAsync();
                var result = EdiFabricX12.Parse(edi, ParseMode.JsonValidateAck, ackParameters.ToConfig());
                return Content(result.Report, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return ErrorHandler.ToResponse(ex);
            }
        }

        private void Authorize()
        {
            var apiKey = GetApiKey();
            EdiFabricX12.SetSerial(apiKey);
            //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
            //  TokenFileCache.Set(apiKey);
        }

        private async Task<byte[]> ReadEdiAsync()
        {
            if (Request.ContentType == "application/json")
            {
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var json = await reader.ReadToEndAsync();
                return Encoding.UTF8.GetBytes(EdiFabricX12.Build(json));
            }
            return await ReadBodyAsync();
        }

        private async Task<byte[]> ReadBodyAsync(string? charSet = null)
        {
            using var buffer = new MemoryStream();
            await Request.Body.CopyToAsync(buffer);
            var bytes = buffer.ToArray();

            if (string.IsNullOrEmpty(charSet))
                return bytes;

            var text = Encoding.GetEncoding(charSet).GetString(bytes);
            return Encoding.UTF8.GetBytes(text);
        }

        private string GetApiKey()
        {
            if (Request.Headers.TryGetValue("Ocp-Apim-Subscription-Key", out var apiKeys))
            {
                var headerKey = apiKeys.FirstOrDefault();
                if (!string.IsNullOrEmpty(headerKey))
                    return headerKey;
            }

            return _apiKey;
        }
    }
}
