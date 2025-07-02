using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Models.Dtos;
using VitoriaAirlinesWeb.Responses;
using VitoriaAirlinesWeb.Services;

namespace VitoriaAirlinesWeb.Controllers
{
    public class AssistBotController : Controller
    {
        private readonly IGeminiApiService _geminiService;
        private readonly IUserHelper _userHelper;

        public AssistBotController(
            IGeminiApiService geminiService,
            IUserHelper userHelper)
        {
            _geminiService = geminiService;
            _userHelper = userHelper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [Route("AssistBot/Ask")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Prompt))
            {
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Prompt is required."
                });
            }

            try
            {
                var user = await _userHelper.GetUserAsync(User);
                string role = "Anonymous";

                if (user != null)
                {
                    var roles = await _userHelper.GetUserRolesAsync(user);
                    role = roles.FirstOrDefault() ?? "Anonymous";
                }

                var result = await _geminiService.AskAsync(dto.Prompt, dto.History, role);

                string jsonResponse;

                if (result.IsSuccess && result.Results is string resultText)
                {
                    var escapedResultText = JsonEncodedText.Encode(resultText);

                    jsonResponse = $@"{{
                        ""isSuccess"": true,
                        ""message"": ""Request successful."",
                        ""results"": ""{escapedResultText}""
                    }}";
                }
                else
                {
                    var escapedMessage = JsonEncodedText.Encode(result.Message ?? "Unknown error.");
                    jsonResponse = $@"{{
                        ""isSuccess"": false,
                        ""message"": ""{escapedMessage}"",
                        ""results"": null
                    }}";
                }

                return Content(jsonResponse, "application/json");
                //return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                });
            }
        }


    }

}
