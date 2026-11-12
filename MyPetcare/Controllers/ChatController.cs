using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Alaminos.Petcare.Controllers
{
    [Route("api/[controller]")]
    [Route("Chat")] // Matches your frontend fetch('/Chat/SendMessage')
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatbotController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public class ChatRequest
        {
            public string? Message { get; set; }
            public string? Prompt { get; set; }
        }

        // Handles both /api/Chatbot/SendMessage and /Chat/SendMessage
        [HttpPost("SendMessage")]
        [HttpPost("send")]
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            var userMessage = request?.Message ?? request?.Prompt;

            if (string.IsNullOrWhiteSpace(userMessage))
                return Ok(new { reply = "Please type a question about your pet or any animal!" });

            userMessage = userMessage.Trim();
            var apiKey = _configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("Gemini__ApiKey");

            // If API key is configured, use live AI for unlimited dynamic answers
            if (!string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_GEMINI_API_KEY")
            {
                try
                {
                    var aiReply = await GetGeminiAnimalResponse(userMessage, apiKey);
                    return Ok(new { reply = aiReply });
                }
                catch
                {
                    return Ok(new { reply = GetLocalAnimalFallback(userMessage) });
                }
            }

            // Local smart fallback with wide variety of distinct animal answers
            return Ok(new { reply = GetLocalAnimalFallback(userMessage) });
        }

        private async Task<string> GetGeminiAnimalResponse(string userMessage, string apiKey)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var systemInstruction =
                "You are PetBot Admin Assistant for Alaminos.Petcare. " +
                "Your job is to answer ALL distinct questions regarding animals, pets, veterinary care, nutrition, behavior, symptoms, diseases, and clinic operations accurately. " +
                "Provide varied, specific, and professional responses tailored to each unique user question. Keep answers concise (3-4 sentences).";

            var payload = new
            {
                system_instruction = new { parts = new[] { new { text = systemInstruction } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = userMessage } } } }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            return doc.RootElement.GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "I am ready to help with your animal care questions!";
        }

        // Comprehensive offline intent matcher supporting different questions with distinct answers
        private string GetLocalAnimalFallback(string msg)
        {
            var q = msg.ToLower();

            if (q.Contains("hi") || q.Contains("hello") || q.Contains("he") || q.Contains("hey"))
                return "Hello Admin! I'm PetBot. You can ask me anything about animal symptoms, pet diets, vaccination schedules, or clinic workflows.";

            if (q.Contains("dog") || q.Contains("puppy"))
            {
                if (q.Contains("food") || q.Contains("eat") || q.Contains("diet"))
                    return "Dogs thrive on high-quality dog food or lean cooked meats (chicken, beef) with safe veggies like carrots and pumpkin. Never feed them chocolate, grapes, onions, or macadamia nuts.";
                if (q.Contains("vaccine") || q.Contains("shot"))
                    return "Puppies need core vaccinations (DHPP / 5-in-1) starting at 6-8 weeks old, followed by an anti-rabies shot at 3 months and annual boosters.";
                return "Dogs require regular daily exercise, mental stimulation, parasite prevention, and routine veterinary checkups to stay healthy and active.";
            }

            if (q.Contains("cat") || q.Contains("kitten"))
            {
                if (q.Contains("food") || q.Contains("eat") || q.Contains("diet"))
                    return "Cats are obligate carnivores requiring animal protein and taurine. Avoid giving them cow's milk, raw eggs, onions, garlic, or chocolate.";
                if (q.Contains("litter") || q.Contains("toilet"))
                    return "Cats instinctively use litter boxes. Keep them clean daily and place them in a quiet, accessible area to prevent behavioral elimination issues.";
                return "Cats need fresh running water, scratching posts, indoor safety, and core FVRCP plus anti-rabies vaccinations annually.";
            }

            if (q.Contains("sick") || q.Contains("vomit") || q.Contains("diarrhea") || q.Contains("fever") || q.Contains("letharg"))
                return "Signs of illness in animals include lethargy, loss of appetite, persistent vomiting, or abnormal stool. Please review the pending appointments or log a medical record for proper veterinary examination.";

            if (q.Contains("bird") || q.Contains("parrot"))
                return "Birds require specialized seed/pellet mixes, fresh fruits, clean cages, and freedom from drafts or toxic fumes (like non-stick cookware Teflon).";

            if (q.Contains("rabbit") || q.Contains("hamster"))
                return "Small herbivores like rabbits need unlimited Timothy hay, fresh greens, and clean water daily to maintain proper dental and digestive health.";

            if (q.Contains("appointment") || q.Contains("schedule") || q.Contains("booking"))
                return "You can monitor, approve, reject, or manage all client appointment bookings directly from the Dashboard and Appointments tabs in this panel.";

            return $"That's an interesting question about '{msg}'. In veterinary practice, proper nutrition, hygiene, and timely vaccinations are key to animal wellness. Let me know if you need specific clinical guidance!";
        }
    }
}