using BingSpellCheckBot.Models;
using BingSpellCheckBot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BingSpellCheckBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly ISpellingService _spellingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public EchoBot(ISpellingService spellingService, IHttpContextAccessor httpContextAccessor)
        {
            _spellingService = spellingService;
            _httpContextAccessor = httpContextAccessor;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                string clientId = null;
                string clientIdCookie = null;
                _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("ClientId", out clientIdCookie);
                clientId = clientIdCookie ?? "";

                string userText = turnContext.Activity.Text;
                SpellingResult spelledResult = _spellingService.CheckSpellingAsync(userText, clientId).Result;

                if (spelledResult.ClientId != null)
                {
                    _httpContextAccessor.HttpContext.Response.Cookies.Append("ClientId", spelledResult.ClientId);
                }

                string spelledText = spelledResult.Text;
                if (!string.IsNullOrEmpty(spelledText))
                {
                    string replyText = $"Echo: {spelledText}";
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
                }
            }
            catch (System.Exception e)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("error", "error"), cancellationToken);
            }
        }
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
