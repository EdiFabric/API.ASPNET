﻿using Microsoft.AspNetCore.Mvc;
using EdiFabric.Api.ASPNET.Models;

namespace EdiFabric.Api.ASPNET.Controllers
{
    [Route("edifact")]
    [ApiController]
    public class EdifactController : ControllerBase
    {
        private readonly IEdifactService _edifactService;
        private readonly ILogger _logger;
        private readonly string _apiKey;
        private readonly string _noData = "No data in request body.";
        public EdifactController(IEdifactService edifactService, ILogger<EdifactController> logger, IConfiguration configuration)
        {
            _edifactService = edifactService;
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
                TokenFileCache.Set(_apiKey);
                return Content(await _edifactService.ReadAsync(Request.Body, _apiKey, readParameters.ToReadParams()), "application/json");
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

            try
            {
                TokenFileCache.Set(_apiKey);
                var result = new MemoryStream();
                var parameters = writeParameters.ToWriteParams();
                await _edifactService.WriteAsync(Request.Body, result, _apiKey, parameters);
                result.Position = 0;
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
           
            try
            {
                TokenFileCache.Set(_apiKey);
                return Content(await _edifactService.ValidateAsync(Request.Body, _apiKey, validateParameters.ToValidateParams()), "application/json");
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

            try
            {
                TokenFileCache.Set(_apiKey);
                return Content(await _edifactService.GenerateAckAsync(Request.Body, _apiKey, ackParameters.ToAckParams()), "application/json");
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
            
            try
            {
                TokenFileCache.Set(_apiKey);
                return Content(await _edifactService.AnalyzeAsync(Request.Body, _apiKey, analyzeParameters.ToAnalyzeParams()), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ErrorHandler.ToResponse(ex);
            }
        }

    }
}
