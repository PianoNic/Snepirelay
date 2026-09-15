using System.Text.Json;
using System.Text.Json.Serialization;

namespace Snepirelay.Application.Services
{
    public static class RelayJson
    {
        public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            AllowOutOfOrderMetadataProperties = true,
            RespectNullableAnnotations = true,
            RespectRequiredConstructorParameters = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
        };
    }
}
