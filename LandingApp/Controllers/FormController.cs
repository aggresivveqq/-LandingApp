using LandingApp.Interfaces;
using LandingApp.Models;
using LandingApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LandingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormController : ControllerBase
    {
        private readonly ILeadService _leadService;
        private readonly ILogger<FormController> _logger;

        public FormController(ILeadService leadService, ILogger<FormController> logger)
        {
            _leadService = leadService;
            _logger = logger;
        }
        [EnableRateLimiting("FormLimiter")]
        [HttpPost("Submit")]
        public async Task<IActionResult> Submit([FromForm] LeadModel model)
        {
            var clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
               ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            string deviceType = "Unknown";
            string os = "Unknown";

            if (userAgent.Contains("Mobi"))
                deviceType = "Mobile 📱";
            else if (userAgent.Contains("Tablet"))
                deviceType = "Tablet 📱";
            else
                deviceType = "Desktop 🖥️";

            if (userAgent.Contains("Windows NT"))
                os = "Windows 🪟";
            else if (userAgent.Contains("Mac OS X"))
                os = "macOS 🍎";
            else if (userAgent.Contains("Android"))
                os = "Android 🤖";
            else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                os = "iOS 🍏";
            else if (userAgent.Contains("Linux"))
                os = "Linux 🐧";

            _logger.LogInformation("Получена заявка с IP: {IP}", clientIp);
            _logger.LogInformation("Браузер: {Browser}", userAgent);
            _logger.LogInformation("Устройство: {Device}, ОС: {OS}", deviceType, os);


            if (string.IsNullOrWhiteSpace(model.TariffName))
            {
                model.TariffName = "Заявка без тарифа";
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("⚠ Невалидная модель от IP {IP}: {@Model}", clientIp, model);
                return BadRequest(new
                {
                    success = false,
                    message = "Пожалуйста, заполните все обязательные поля корректно."
                });
            }
            if (ContainsMaliciousContent(model.Name) ||
                ContainsMaliciousContent(model.Phone))
            {
                _logger.LogWarning("🚨 Обнаружена XSS-атака от IP {IP}: {@Model}", clientIp, model);
                return BadRequest(new
                {
                    success = false,
                    message = "Недопустимое содержимое в полях формы."
                });
            }

            model.Name = StripHtml(model.Name);
            model.Phone = StripHtml(model.Phone);
            if (string.IsNullOrWhiteSpace(model.City))
            {
                model.City = "Не указан";
            }
            model.City = StripHtml(model.City);
            try
            {
                await _leadService.AddAsync(model);

                _logger.LogInformation("✅ Заявка успешно сохранена от IP {IP}", clientIp);

                return Ok(new
                {
                    success = true,
                    message = "Ваша заявка успешно отправлена! Мы свяжемся с вами в ближайшее время."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при сохранении заявки от IP {IP}", clientIp);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Произошла ошибка при обработке заявки. Попробуйте позже."
                });
            }
        }

        private bool ContainsMaliciousContent(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            string pattern = @"<script|</script|javascript:|on\w+=|alert\(|document\.|window\.";
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }

        private string StripHtml(string input)
        {
            return string.IsNullOrEmpty(input)
                ? string.Empty
                : Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
