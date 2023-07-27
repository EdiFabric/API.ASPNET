using Microsoft.AspNetCore.Mvc;
using EdiFabric.Api.ASPNET.Models;

namespace EdiFabric.Api.ASPNET.Controllers
{
    [Route("vda")]
    [ApiController]
    public class VdaController : ControllerBase
    {
        private readonly IVdaService _vdaService;
        private readonly ILogger _logger;
        private readonly string _apiKey;
        private readonly string _noData = "No data in request body.";
        public VdaController(IVdaService vdaService, ILogger<VdaController> logger, IConfiguration configuration)
        {
            _vdaService = vdaService;
            _logger = logger;
            _apiKey = configuration["ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("No ApiKey configuration in appsettings.json.");
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

            try
            {
                SerialKey.Set(_apiKey);
                //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
                //  TokenFileCache.Set(_apiKey);
                return Content(await _vdaService.ReadAsync(Request.Body, _apiKey, readParameters.ToReadParams()), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
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

            try
            {
                SerialKey.Set(_apiKey);
                //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
                //  TokenFileCache.Set(_apiKey);
                var result = new MemoryStream();
                var parameters = writeParameters.ToWriteParams();
                await _vdaService.WriteAsync(Request.Body, result, _apiKey, parameters);
                result.Position = 0;
                return File(result, parameters.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
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

            try
            {
                SerialKey.Set(_apiKey);
                //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
                //  TokenFileCache.Set(_apiKey);
                return Content(await _vdaService.ValidateAsync(Request.Body, _apiKey, validateParameters.ToValidateParams()), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
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

            try
            {
                SerialKey.Set(_apiKey);
                //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
                //  TokenFileCache.Set(_apiKey);
                return Content(await _vdaService.GenerateAckAsync(Request.Body, _apiKey, ackParameters.ToAckParams()), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
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

            try
            {
                SerialKey.Set(_apiKey);
                //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
                //  TokenFileCache.Set(_apiKey);
                return Content(await _vdaService.AnalyzeAsync(Request.Body, _apiKey, analyzeParameters.ToAnalyzeParams()), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return ErrorHandler.ToResponse(ex);
            }
        }

    }
}
