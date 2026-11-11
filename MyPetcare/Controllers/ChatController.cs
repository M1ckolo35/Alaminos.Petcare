using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MyPetcare.Controllers
{
    public class ChatController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<ChatController> _logger;

        private const string SystemPrompt =
            "You are the friendly assistant of MyPetcare, a pet care service site. " +
            "Answer any question about animals: dogs, cats, rabbits, birds, fish, reptiles, livestock, " +
            "wildlife, and exotic pets. Cover care, feeding, behavior, training, breeds, grooming, " +
            "vaccination, common illnesses, lifespan, and fun facts. Give a different, specific answer " +
            "for each question, in the same language the user writes in (English, Tagalog, or Taglish). " +
            "Keep replies short (under 150 words) unless the user asks for detail. For symptoms that " +
            "sound serious (poisoning, bleeding, trouble breathing, seizures), tell the user to see a " +
            "veterinarian right away. Never give exact medication doses. If a question has nothing to " +
            "do with animals, politely say you only help with animal and pet topics.";

        public ChatController(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ChatController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Message))
            {
                return Json(new { success = false, reply = "Please ask a question about animals or pet care." });
            }

            var apiKey = _config["ANTHROPIC_API_KEY"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("ANTHROPIC_API_KEY is not set.");
                return Json(new { success = false, reply = "The chatbot is not configured yet." });
            }

            // Conversation: last 10 turns of history (optional) + the new message
            var messages = new List<object>();
            if (model.History != null)
            {
                foreach (var h in model.History.TakeLast(10))
                {
                    if (string.IsNullOrWhiteSpace(h.Content)) continue;
                    var role = h.Role == "assistant" ? "assistant" : "user";
                    messages.Add(new { role, content = h.Content });
                }
            }
            messages.Add(new { role = "user", content = model.Message.Trim() });

            var body = new
            {
                model = "claude-haiku-5-5",
                max_tokens = 400,
                system = SystemPrompt,
                messages
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
                {
                    Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                };
                request.Headers.Add("x-api-key", apiKey);
                request.Headers.Add("anthropic-version", "2023-06-01");

                var response = await client.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Claude API error {Status}: {Body}", (int)response.StatusCode, json);
                    return Json(new { success = false, reply = "Sorry, I couldn't answer right now. Please try again." });
                }

                using var doc = JsonDocument.Parse(json);
                var reply = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();

                return Json(new { success = true, reply });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat failed");
                return Json(new { success = false, reply = "Sorry, something went wrong. Please try again." });
            }
        }
    }

    public class ChatModel
    {
        public string? Message { get; set; }
        public List<ChatTurn>? History { get; set; }   // optional, for follow-up questions
    }

    public class ChatTurn
    {
        public string Role { get; set; } = "user";      // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
    }
}