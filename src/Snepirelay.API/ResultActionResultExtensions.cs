using Microsoft.AspNetCore.Mvc;
using Snepirelay.Application.Dtos.Relay;
using Snepirelay.Application.Models;

namespace Snepirelay.API
{
    public static class ResultActionResultExtensions
    {
        private static readonly HashSet<string> NotFoundErrors = [RelayErrors.SessionNotFound, RelayErrors.MemberNotFound];

        public static IActionResult ToActionResult(this Result result) =>
            result.IsSuccess ? new NoContentResult() : Problem(result.Error, result.ErrorValues);

        public static IActionResult ToActionResult<T>(this Result<T> result) =>
            result.IsSuccess ? new OkObjectResult(result.Value) : Problem(result.Error, result.ErrorValues);

        public static RelayEventDto? ToRelayReply(this Result result, string? rid) =>
            result.IsFailure
                ? new ErrorDto(result.Error ?? RelayErrors.InvalidMessage, result.ErrorValues) { Rid = rid }
                : rid is null ? null : new OkDto { Rid = rid };

        private static IActionResult Problem(string? error, IReadOnlyDictionary<string, string>? values)
        {
            var body = new { error, values };
            return error is not null && NotFoundErrors.Contains(error)
                ? new NotFoundObjectResult(body)
                : new BadRequestObjectResult(body);
        }
    }
}
