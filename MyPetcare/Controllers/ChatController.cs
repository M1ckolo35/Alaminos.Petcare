using Microsoft.AspNetCore.Mvc;

namespace MyPetcare.Controllers // Palitan ito ayon sa exact namespace ng iyong project kung kinakailangan
{
    public class ChatController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Message))
            {
                return Json(new { success = false, reply = "Please ask a question about animals or pet care." });
            }

            string userQuery = model.Message.ToLower().Trim();
            string botReply = "";

            // Comprehensive animal topic handler
            if (userQuery.Contains("dog") || userQuery.Contains("puppy"))
            {
                botReply = $"Regarding dogs ({model.Message}): Dogs thrive on routine, regular exercise, balanced nutrition (rich in proteins), and mental stimulation like chew toys and fetch. Always keep toxic foods like chocolate, grapes, and onions away from them!";
            }
            else if (userQuery.Contains("cat") || userQuery.Contains("kitten"))
            {
                botReply = $"Regarding cats ({model.Message}): Cats require fresh water, high-protein diets (as obligate carnivores), clean litter boxes, and vertical spaces like scratching posts or cat trees to keep them active and stress-free.";
            }
            else if (userQuery.Contains("food") || userQuery.Contains("eat") || userQuery.Contains("diet") || userQuery.Contains("favourite"))
            {
                botReply = $"Great question about animal nutrition/preferences ('{model.Message}')! Most domestic pets require species-appropriate diets formulated with essential vitamins, minerals, and proteins. Avoid feeding them processed human junk food, bones that splinter, or toxic ingredients.";
            }
            else if (userQuery.Contains("health") || userQuery.Contains("sick") || userQuery.Contains("vaccine") || userQuery.Contains("vet"))
            {
                botReply = $"Pet health is crucial! For '{model.Message}', regular veterinary check-ups, up-to-date vaccinations, deworming, and parasite prevention are key to a long, healthy life. If symptoms persist, consulting a licensed veterinarian is always recommended.";
            }
            else
            {
                // General fallback that still provides helpful animal information instead of deflecting
                botReply = $"That's a fascinating question about animals: '{model.Message}'. Animals exhibit diverse behaviors, unique nutritional requirements, and specialized health needs. Providing a safe environment, proper diet, and love ensures optimal pet welfare!";
            }

            return Json(new { success = true, reply = botReply });
        }
    }

    public class ChatModel
    {
        public string? Message { get; set; }
    }
}