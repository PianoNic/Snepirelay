using System.Text;
using System.Text.Json;

namespace Snepirelay.Tests.Support
{
    public static class JsonElementExtensions
    {
        public static string? Type(this JsonElement element) => element.GetProperty("type").GetString();

        public static string? Str(this JsonElement element, params string[] path) => element.At(path).GetString();

        public static JsonElement At(this JsonElement element, params string[] path) =>
            path.Aggregate(element, (current, key) => current.GetProperty(key));
    }
}
