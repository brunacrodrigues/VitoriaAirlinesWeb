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
        private readonly IAdminPromptService _adminPrompt;

        public AssistBotController(
            IGeminiApiService geminiService,
            IUserHelper userHelper,
            IAdminPromptService adminPrompt)
        {
            _geminiService = geminiService;
            _userHelper = userHelper;
            _adminPrompt = adminPrompt;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userHelper.GetUserAsync(User);
            string? role = null;

            if (user != null)
            {
                var roles = await _userHelper.GetUserRolesAsync(user);
                role = roles.FirstOrDefault();
            }

            string[] suggestions;

            switch (role)
            {
                case UserRoles.Admin:
                    suggestions = new[]
                    {
                        "List all registered airplanes",
                        "View today's scheduled flights",
                        "List all airports",
                        "Create an airplane model"
                    };
                    break;

                default:
                    suggestions = new[]
                    {
                        "How can I buy a ticket?",
                        "What destinations are available?",
                        "How do I create an account?"
                    };
                    break;
            }

            ViewBag.Suggestions = suggestions;

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
                 string? role = null;
                if (user != null)
                {
                    var roles = await _userHelper.GetUserRolesAsync(user);
                    role = roles.FirstOrDefault();
                }

                // 2) tenta prompt custom de acordo com a role (ou anon)
                ApiResponse? response = null;
                var promptLower = dto.Prompt.ToLower();

                if (role == UserRoles.Admin)
                    response = await _adminPrompt.ProcessPromptAsync(promptLower);
                //else if (role == UserRoles.Employee)
                //    custom = await _employeePrompt.ProcessPromptAsync(promptLower);
                //else if (role == UserRoles.Customer)
                //    custom = await _customerPrompt.ProcessPromptAsync(promptLower);
                //else
                //    custom = await _anonymousPrompt.ProcessPromptAsync(promptLower);

                if (response is null)
                    response = await _geminiService.AskAsync(dto.Prompt, dto.History, role);

                var escapedMessage = JsonEncodedText.Encode(response.Message ?? "").ToString();

                string escapedResults;
                if (response.Results is string resultText)
                {
                    escapedResults = JsonEncodedText.Encode(resultText).ToString();
                }
                else
                {
                    escapedResults = JsonEncodedText.Encode("(no results)").ToString();
                }

                var json = $@"{{
                    ""isSuccess"": {response.IsSuccess.ToString().ToLower()},
                    ""message"": ""{escapedMessage}"",
                    ""results"": ""{escapedResults}""
                }}";

                return Content(json, "application/json");

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
