using BingSpellCheckBot.Models;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BingSpellCheckBot.Services
{
    public class SpellingService : ISpellingService
    {
        private readonly IConfiguration _configuration;
        public SpellingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private string Istanbul = "Lat: 41.04206; Long: 28.99623; re:1000;";
        private string Market = "tr-TR";
        private string Mode = "spell";
        public async Task<SpellingResult> CheckSpellingAsync(string text, string clientId = "")
        {
            string Key = _configuration["BingSpellCheckKey"];
            var geoLocation = Istanbul;
            var ipAddress = GetPublicIpAddress();

            SpellCheckClient client = new SpellCheckClient(new ApiKeyServiceClientCredentials(Key));
            var response = await client.SpellCheckerWithHttpMessagesAsync(text, market: Market, clientId: clientId, clientIp: ipAddress, location: geoLocation, mode: Mode);

            HttpResponseHeaders responseHeaders = response.Response.Headers;
            var spellingResult = new SpellingResult
            {
                ClientId = GetHeaderValue(responseHeaders, "X-MSEdge-ClientID"),
                TraceId = GetHeaderValue(responseHeaders, "BingAPIs-SessionId"),
                Text = ProcessResults(text, response.Body.FlaggedTokens)
            };

            return spellingResult;
        }
        private string ProcessResults(string text, IList<SpellingFlaggedToken> flaggedTokens)
        {
            StringBuilder newText = new StringBuilder(text);
            int indexDiff = 0;

            foreach (var token in flaggedTokens)
            {
                if (token.Type == "RepeatedToken")
                {
                    newText.Remove(token.Offset - indexDiff, token.Token.Length + 1);
                    indexDiff += token.Token.Length + 1;
                }
                else
                {
                    if (token.Suggestions.Count > 0)
                    {
                        var suggestedToken = token.Suggestions.FirstOrDefault(x => x.Score >= 0.7);
                        if (suggestedToken == null)
                        {
                            break;
                        }

                        int differenceIndex = token.Offset - indexDiff;
                        newText.Remove(differenceIndex, token.Token.Length);
                        newText.Insert(differenceIndex, suggestedToken.Suggestion);

                        indexDiff += token.Token.Length - suggestedToken.Suggestion.Length;
                    }
                }
            }
            return newText.ToString();
        }
        private string GetHeaderValue(HttpResponseHeaders headers, string key)
        {
            string value = null;
            IEnumerable<string> returnedValue;
            if (headers.TryGetValues(key, out returnedValue))
            {
                value = returnedValue.FirstOrDefault();
            }
            return value;
        }
        private string GetPublicIpAddress()
        {
            string publicIp = new System.Net.WebClient().DownloadString("https://api.ipify.org");
            return publicIp;
        }
    }
}
