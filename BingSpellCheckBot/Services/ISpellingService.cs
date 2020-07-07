using BingSpellCheckBot.Models;
using System.Threading.Tasks;

namespace BingSpellCheckBot.Services
{
    public interface ISpellingService
    {
        Task<SpellingResult> CheckSpellingAsync(string text, string clientId = "");
    }
}
