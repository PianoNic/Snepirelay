# Developer setup

## Prerequisites

- .NET 10 SDK
- Docker, only to build the image

## Run it

```bash
dotnet run --project src/Snepirelay.API        # relay on :5180, socket at ws://localhost:5180/api/relay
```

## Tests

```bash
dotnet test
```

The suite starts the real pipeline in-process and drives it over WebSockets: a host with guests, guest control, kicks, reconnects within and past the grace, identity checks and every error the socket answers. Time runs on a fake clock, so nothing waits.

::: warning `dotnet test` needs the `test.runner` opt-in in `global.json`
TUnit is a Microsoft.Testing.Platform framework and the .NET 10 SDK dropped the VSTest bridge.
:::

## Layout

| Project | Holds |
| --- | --- |
| `Snepirelay.Domain` | jams, members and the playback value objects |
| `Snepirelay.Infrastructure` | the in-memory store |
| `Snepirelay.Application` | commands, queries and handlers, the `Result` type, the relay messages and the socket runner |
| `Snepirelay.API` | `Program.cs`, `Extensions` and `Controllers`, the WebSocket included |
| `Snepirelay.Tests` | the end-to-end suite |

Every socket message becomes a Mediator command, and every handler returns a `Result`. The same `Result` answers both sides: `ToActionResult` for HTTP and `ToRelayReply` for the socket, with the same `{ error, values }` shape. One pipeline behavior runs messages one at a time, so handlers never race over a jam.

## Versioning

`application.properties` holds the version. Publishing a release draft tags it, and the release pipeline bumps the file to the tag and pushes the image.

## The image

```bash
docker build -f src/Snepirelay.API/Dockerfile -t snepirelay .
```
