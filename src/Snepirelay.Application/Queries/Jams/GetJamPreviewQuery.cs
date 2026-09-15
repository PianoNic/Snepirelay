using Mediator;
using Microsoft.Extensions.Options;
using Snepirelay.Application.Dtos.Jams;
using Snepirelay.Application.Mappings.Jams;
using Snepirelay.Application.Models;
using Snepirelay.Infrastructure.Interfaces;

namespace Snepirelay.Application.Queries.Jams
{
    public record GetJamPreviewQuery(string JoinToken) : IQuery<Result<JamPreviewDto>>;

    public class GetJamPreviewQueryHandler(IJamStore store, IOptions<RelayOptions> options)
        : IQueryHandler<GetJamPreviewQuery, Result<JamPreviewDto>>
    {
        public ValueTask<Result<JamPreviewDto>> Handle(GetJamPreviewQuery query, CancellationToken cancellationToken) =>
            ValueTask.FromResult(store.FindJamByToken(query.JoinToken) is { } jam
                ? Result.Success(jam.ToPreviewDto(store, options.Value.MaxMemberCount))
                : Result.Failure<JamPreviewDto>(RelayErrors.SessionNotFound));
    }
}
