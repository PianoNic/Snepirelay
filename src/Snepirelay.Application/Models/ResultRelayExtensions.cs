using Snepirelay.Application.Dtos.Relay;

namespace Snepirelay.Application.Models
{
    public static class ResultRelayExtensions
    {
        public static RelayEventDto? ToRelayReply(this Result result, string? rid) =>
            result.IsFailure
                ? new ErrorDto(result.Error ?? RelayErrors.InvalidMessage, result.ErrorValues) { Rid = rid }
                : rid is null ? null : new OkDto { Rid = rid };
    }
}
