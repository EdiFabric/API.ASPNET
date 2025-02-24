﻿using Microsoft.AspNetCore.Mvc;
using EdiFabric.Api.ASPNET.Models;

namespace EdiFabric.Api.ASPNET.Controllers
{
    [Route("ncpdp")]
    [ApiController]
    public class NcpdpController : ControllerBase
    {
        private readonly INcpdpService _ncpdpService;
        private readonly ILogger _logger;
        private readonly string _apiKey;
        private readonly string _noData = "No data in request body.";
        public NcpdpController(INcpdpService ncpdpService, ILogger<NcpdpController> logger, IConfiguration configuration, ILocalModelsService localModelsService)
        {
            _ncpdpService = ncpdpService;
            _logger = logger;
            _apiKey = configuration["ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("No ApiKey configuration in appsettings.json.");
            //  Uncomment if you wish to use distributed cache for models
            //  localModelsService.Load(_apiKey, Directory.GetCurrentDirectory() + @"\EDI");
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
                var apiKey = GetApiKey();
                SerialKey.Set(apiKey);
                //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
                //  TokenFileCache.Set(apiKey);
                return Content(await _ncpdpService.ReadAsync(Request.Body, apiKey, readParameters.ToReadParams()), "application/json");
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
                var apiKey = GetApiKey();
                SerialKey.Set(apiKey);
                //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
                //  TokenFileCache.Set(apiKey);
                var result = new MemoryStream();
                var parameters = writeParameters.ToWriteParams();
                await _ncpdpService.WriteAsync(Request.Body, result, apiKey, parameters);
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
                var apiKey = GetApiKey();
                SerialKey.Set(apiKey);
                //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
                //  TokenFileCache.Set(apiKey);
                return Content(await _ncpdpService.ValidateAsync(Request.Body, apiKey, validateParameters.ToValidateParams()), "application/json");
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
                var apiKey = GetApiKey();
                SerialKey.Set(apiKey);
                //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
                //  TokenFileCache.Set(apiKey);
                return Content(await _ncpdpService.GenerateAckAsync(Request.Body, apiKey, ackParameters.ToAckParams()), "application/json");
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
                var apiKey = GetApiKey();
                SerialKey.Set(apiKey);
                //  Uncomment and then comment the line above if you wish to use distributed cache for tokens
                //  TokenFileCache.Set(apiKey);
                return Content(await _ncpdpService.AnalyzeAsync(Request.Body, apiKey, analyzeParameters.ToAnalyzeParams()), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
                return ErrorHandler.ToResponse(ex);
            }
        }

        private string GetApiKey()
        {
            if (Request.Headers.TryGetValue("Ocp-Apim-Subscription-Key", out var apiKeys) && apiKeys.FirstOrDefault() == null)
                return apiKeys.First();

            return _apiKey;
        }
    }
}
