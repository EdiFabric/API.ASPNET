using Microsoft.AspNetCore.Mvc;
using EdiFabric.Api.ASPNET.Models;

namespace EdiFabric.Api.ASPNET.Controllers
{
    [Route("x12")]
    [ApiController]
    public class X12Controller : ControllerBase
    {
        private readonly IX12Service _x12Service;
        private readonly ILogger _logger;
        private readonly string _apiKey = "Ocp-Apim-Subscription-Key";
        private readonly string _noApiKey = "No Ocp-Apim-Subscription-Key in header.";
        private readonly string _noData = "No data in request body.";
        public X12Controller(IX12Service x12Service, ILogger<X12Controller> logger)
        {
            _x12Service = x12Service;
            _logger = logger;
        }

        [Route("read")]
        [HttpPost]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> Read([FromQuery]ReadParameters readParameters)
        {
            if (Request.ContentLength == 0 || Request.Body == null)
            {
                _logger.LogError(_noData);
                return ErrorHandler.ToResponse(_noData);
            }

            if (!Request.Headers.TryGetValue(_apiKey, out var apiKeys) || apiKeys.FirstOrDefault() == null)
            {
                _logger.LogError(_noApiKey);
                return ErrorHandler.ToResponse(_noApiKey);
            }

            try
            {
                return Content(await _x12Service.ReadAsync(Request.Body, apiKeys.First(), readParameters.ToReadParams()), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ErrorHandler.ToResponse(ex);
            }
        }

        [Route("write")]
        [HttpPost]
        public async Task<IActionResult> Write([FromQuery] WriteParameters writeParameters)
        {
            if (Request.ContentLength == 0 || Request.Body == null)
            {
                _logger.LogError(_noData);
                return ErrorHandler.ToResponse(_noData);
            }

            if (!Request.Headers.TryGetValue(_apiKey, out var apiKeys) || apiKeys.FirstOrDefault() == null)
            {
                _logger.LogError(_noApiKey);
                return ErrorHandler.ToResponse(_noApiKey);
            }

            try
            {
                var result = new MemoryStream();
                var parameters = writeParameters.ToWriteParams();
                await _x12Service.WriteAsync(Request.Body, result, apiKeys.First(), parameters);
                return File(result, parameters.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ErrorHandler.ToResponse(ex);
            }
        }

        [Route("validate")]
        [HttpPost]
        public async Task<IActionResult> Validate([FromQuery] ValidateParameters validateParameters)
        {
            if (Request.ContentLength == 0 || Request.Body == null)
            {
                _logger.LogError(_noData);
                return ErrorHandler.ToResponse(_noData);
            }

            if (!Request.Headers.TryGetValue(_apiKey, out var apiKeys) || apiKeys.FirstOrDefault() == null)
            {
                _logger.LogError(_noApiKey);
                return ErrorHandler.ToResponse(_noApiKey);
            }

            try
            {
                return Content(await _x12Service.ValidateAsync(Request.Body, apiKeys.First(), validateParameters.ToValidateParams()), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ErrorHandler.ToResponse(ex);
            }
        }

        [Route("ack")]
        [HttpPost]
        public async Task<IActionResult> Ack([FromQuery] AckParameters ackParameters)
        {
            if (Request.ContentLength == 0 || Request.Body == null)
            {
                _logger.LogError(_noData);
                return ErrorHandler.ToResponse(_noData);
            }

            if (!Request.Headers.TryGetValue(_apiKey, out var apiKeys) || apiKeys.FirstOrDefault() == null)
            {
                _logger.LogError(_noApiKey);
                return ErrorHandler.ToResponse(_noApiKey);
            }

            try
            {
                return Content(await _x12Service.GenerateAckAsync(Request.Body, apiKeys.First(), ackParameters.ToAckParams()), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ErrorHandler.ToResponse(ex);
            }
        }

        [Route("analyze")]
        [HttpPost]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> Analyze([FromQuery] AnalyzeParameters analyzeParameters)
        {
            if (Request.ContentLength == 0 || Request.Body == null)
            {
                _logger.LogError(_noData);
                return ErrorHandler.ToResponse(_noData);
            }

            if (!Request.Headers.TryGetValue(_apiKey, out var apiKeys) || apiKeys.FirstOrDefault() == null)
            {
                _logger.LogError(_noApiKey);
                return ErrorHandler.ToResponse(_noApiKey);
            }

            try
            {
                return Content(await _x12Service.AnalyzeAsync(Request.Body, apiKeys.First(), analyzeParameters.ToAnalyzeParams()), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ErrorHandler.ToResponse(ex);
            }
        }

    }
}
