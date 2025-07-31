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
        /// <summary>
        /// Handles interactions with an AI-powered assistant bot, providing tailored responses based on user roles.
        /// </summary>
        private readonly IGeminiApiService _geminiService;
        private readonly IUserHelper _userHelper;
        private readonly IAdminPromptService _adminPrompt;
        private readonly IEmployeePromptService _employeePrompt;
        private readonly ICustomerPromptService _customerPrompt;
        private readonly IAnonymousPromptService _anonymousPrompt;


        /// <summary>
        /// Initializes a new instance of the AssistBotController with necessary services for AI interaction,
        /// user management, and role-specific prompt processing.
        /// </summary>
        /// <param name="geminiService">Service for interacting with the Gemini AI model.</param>
        /// <param name="userHelper">Helper for user-related operations, including role retrieval.</param>
        /// <param name="adminPrompt">Service for processing prompts from Admin users.</param>
        /// <param name="employeePrompt">Service for processing prompts from Employee users.</param>
        /// <param name="customerPrompt">Service for processing prompts from Customer users.</param>
        /// <param name="anonymousPrompt">Service for processing prompts from anonymous users.</param>
        public AssistBotController(
            IGeminiApiService geminiService,
            IUserHelper userHelper,
            IAdminPromptService adminPrompt,
            IEmployeePromptService employeePrompt,
            ICustomerPromptService customerPrompt,
            IAnonymousPromptService anonymousPrompt)
        {
            _geminiService = geminiService;
            _userHelper = userHelper;
            _adminPrompt = adminPrompt;
            _employeePrompt = employeePrompt;
            _customerPrompt = customerPrompt;
            _anonymousPrompt = anonymousPrompt;
        }


        /// <summary>
        /// Displays the main page for the AI assistant bot, providing suggested prompts based on the user's role.
        /// </summary>
        /// <returns>
        /// Task: A view displaying the bot interface with role-specific prompt suggestions.
        /// </returns>
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

                case UserRoles.Employee:
                    suggestions = new[]
                    {
                       "List all active airplanes",
                       "Schedule a flight",
                       "List all airports",
                       "View today's scheduled flights"
                    };
                    break;

                case UserRoles.Customer:
                    suggestions = new[]
                    {
                       "List all available flights",
                       "View my flights history",
                       "View my future flights",
                       "How can I edit my profile?"
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



        /// <summary>
        /// Receives a chat prompt from the client, processes it based on the user's role,
        /// and returns an AI-generated response.
        /// </summary>
        /// <param name="dto">The chat request data, including the prompt and chat history.</param>
        /// <returns>
        /// Task: An IActionResult containing a JSON response with the AI's message and results,
        /// or a BadRequest/StatusCode 500 on error.
        /// </returns>
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


                ApiResponse? response = null;
                var promptLower = dto.Prompt.ToLower();

                if (role == UserRoles.Admin)
                    response = await _adminPrompt.ProcessPromptAsync(promptLower);
                else if (role == UserRoles.Employee)
                    response = await _employeePrompt.ProcessPromptAsync(promptLower);
                else if (role == UserRoles.Customer)
                    response = await _customerPrompt.ProcessPromptAsync(promptLower);
                else
                    response = await _anonymousPrompt.ProcessPromptAsync(promptLower);


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
