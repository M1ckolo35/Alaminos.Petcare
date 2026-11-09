using Microsoft.AspNetCore.Mvc;

namespace MyPetcare.Controllers // Palitan ito ayon sa exact namespace ng iyong project kung kinakailangan
{
    public class ChatController : Controller
    {
        [HttpPost]
        public IActionResult SendMessage([FromBody] ChatModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Message))
            {
                return Json(new { reply = "Please ask me any question about animals, pets, or veterinary care!" });
            }

            string userMsg = model.Message.ToLower();
            string botReply = "";

            // Comprehensive animal knowledge base logic
            if (userMsg.Contains("dog dont want") || userMsg.Contains("hate") || userMsg.Contains("dislike") || userMsg.Contains("things that dog"))
            {
                botReply = "Dogs generally dislike tight hugs, loud sudden noises, strong chemical scents like citrus or vinegar, being disturbed while eating, and having their nose or sensitive paws touched roughly.";
            }
            else if (userMsg.Contains("cat") || userMsg.Contains("pusa"))
            {
                botReply = "Cats are independent animals that communicate through purring, meowing, and tail movements. They need fresh water, high-protein diets, scratching posts, and regular litter box maintenance!";
            }
            else if (userMsg.Contains("eat") || userMsg.Contains("food") || userMsg.Contains("feed") || userMsg.Contains("poison") || userMsg.Contains("chocolate"))
            {
                botReply = "Many common foods are dangerous to animals! For example, chocolate, onions, garlic, grapes, and xylitol (artificial sweetener) are toxic to dogs and cats. Always check if a food is safe before feeding your pets.";
            }
            else if (userMsg.Contains("vaccine") || userMsg.Contains("rabies") || userMsg.Contains("shot"))
            {
                botReply = "Core vaccines protect pets from severe illnesses. Dogs need Rabies, Parvovirus, and Distemper shots, while cats need FVRCP and Rabies vaccines, typically starting at 6 to 8 weeks of age.";
            }
            else if (userMsg.Contains("sleep") || userMsg.Contains("rest"))
            {
                botReply = "Most adult dogs sleep around 12 to 14 hours a day, while cats can sleep up to 15 hours a day. Puppies and kittens need even more sleep—up to 18-20 hours—to support their rapid growth!";
            }
            else if (userMsg.Contains("groom") || userMsg.Contains("bath") || userMsg.Contains("ligo"))
            {
                botReply = "Regular grooming keeps animals healthy. Dogs usually need bathing every 3 to 4 weeks depending on their coat, while cats mostly groom themselves but benefit from weekly brushing to prevent hairballs.";
            }
            else
            {
                botReply = $"That's a very interesting question about '{model.Message}'. Animals have unique behaviors, health needs, and instincts. For specific medical diagnoses or tailored care routines regarding this animal, our clinic experts are always ready to help!";
            }

            return Json(new { reply = botReply });
        }
    }

    public class ChatModel
    {
        public string? Message { get; set; }
    }
}