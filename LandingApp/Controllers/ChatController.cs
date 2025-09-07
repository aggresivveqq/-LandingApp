using LandingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;

namespace LandingApp.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService; 

        public ChatController(IChatService chatService) 
        {
            _chatService = chatService;
        }
        [EnableRateLimiting("ChatLimiter")]
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { reply = "Пожалуйста, введите сообщение." });
            }

            var reply = await _chatService.GetChatResponseAsync(request.Message);
            return Ok(new { reply });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
