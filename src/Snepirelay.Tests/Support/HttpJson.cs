using System.Text.Json;
using Snepirelay.Tests.Support;

namespace Snepirelay.Tests.Support
{
    internal static class HttpJson
    {
        public static async Task<JsonElement> GetFromJsonElementAsync(this HttpClient client, string path)
        {
            var response = await client.GetAsync(path);
            response.EnsureSuccessStatusCode();
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
        }
    }
}
