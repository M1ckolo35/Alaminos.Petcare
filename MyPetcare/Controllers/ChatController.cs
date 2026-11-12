using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Alaminos.Petcare.Controllers
{
    [Route("api/[controller]")]
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
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost("send")] // Adjust route if your JS calls /Chatbot/Ask or /api/Chatbot
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { reply = "Please type a question about your pet or any animal!" });

            var userMessage = request.Message.Trim();
            var apiKey = _configuration["Gemini:ApiKey"];

            // If API key is configured, use real AI for unlimited dynamic answers
            if (!string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_GEMINI_API_KEY")
            {
                try
                {
                    var aiReply = await GetGeminiAnimalResponse(userMessage, apiKey);
                    return Ok(new { reply = aiReply });
                }
                catch
                {
                    // Fallback to local smart matcher if internet/API fails
                    return Ok(new { reply = GetLocalAnimalFallback(userMessage) });
                }
            }

            // Local smart fallback (works offline without API key)
            return Ok(new { reply = GetLocalAnimalFallback(userMessage) });
        }

        private async Task<string> GetGeminiAnimalResponse(string userMessage, string apiKey)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var systemInstruction =
                "You are PetBot, the official veterinary and animal care assistant for Alaminos.Petcare. " +
                "Answer ONLY questions related to animals, pets, veterinary care, nutrition, symptoms, vaccines, breeds, and wildlife. " +
                "Give specific, accurate, and varied answers tailored directly to what the user asked. " +
                "Keep responses concise (3-5 sentences), friendly, and remind users to book an appointment at Alaminos Veterinary Office for serious symptoms. " +
                "If the user greets you (e.g., 'hi', 'hello'), greet them warmly and ask how you can help with their pets today. " +
                "If the question is completely unrelated to animals (e.g., math, politics, coding), politely decline and steer back to animal care.";

            var payload = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemInstruction } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = userMessage } }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var reply = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return reply ?? "I couldn't process that right now. Please try asking again!";
        }

        // Offline / Backup Intent Matcher (Handles distinct topics instead of one generic reply)
        private string GetLocalAnimalFallback(string msg)
        {
            var q = msg.ToLower();

            if (q == "hi" || q == "hello" || q == "he" || q == "hey")
                return "Hello! I'm PetBot. Ask me anything about dogs, cats, birds, livestock, pet nutrition, vaccines, or clinic appointments!";

            if (q.Contains("food") || q.Contains("eat") || q.Contains("diet") || q.Contains("toxic"))
            {
                if (q.Contains("cat"))
                    return "Cats are obligate carnivores and need high-protein meat diets with taurine. Avoid giving them milk, onions, garlic, chocolate, or raw dough.";
                return "Dogs do best on balanced kibble or lean cooked meats (chicken, beef) with vegetables like carrots. Never feed them chocolate, grapes, raisins, onions, or xylitol.";
            }

            if (q.Contains("sick") || q.Contains("vomit") || q.Contains("diarrhea") || q.Contains("fever") || q.Contains("symptom"))
                return "Common signs of illness in pets include lethargy, loss of appetite, vomiting, diarrhea, or pale gums. Please book an appointment through our dashboard so a veterinarian can examine your pet properly.";

            if (q.Contains("vaccine") || q.Contains("rabies") || q.Contains("parvo") || q.Contains("shot"))
                return "Puppies and kittens should start core vaccinations (5-in-1 / 4-in-1) at 6–8 weeks old, followed by anti-rabies at 3 months and annual boosters.";

            if (q.Contains("cat") || q.Contains("kitten"))
                return "For cats, ensure fresh water daily, clean litter boxes, regular deworming every 3 months, and annual FVRCP + anti-rabies vaccines. What specific question do you have about cats?";

            if (q.Contains("dog") || q.Contains("puppy"))
                return "For dogs, regular exercise, parasite prevention (ticks/fleas/heartworm), clean water, and yearly checkups keep them healthy. What would you like to know about dogs?";

            if (q.Contains("bird") || q.Contains("rabbit") || q.Contains("hamster") || q.Contains("fish"))
                return "Exotic and small pets need species-specific enclosures, temperature control, and specialized diets. Let me know which specific animal you're asking about!";

            if (q.Contains("appointment") || q.Contains("book") || q.Contains("schedule"))
                return "You can schedule a veterinary visit directly from the 'Book Appointment' tab in your Alaminos.Petcare dashboard.";

            return "I'm ready to help with any animal or pet care question! Try asking about pet diets, symptoms, vaccines, breeds, or booking a vet visit.";
        }
    }
}