# Self-hosting

## Run it

```yaml
services:
  snepirelay:
    image: ghcr.io/pianonic/snepirelay:latest
    restart: unless-stopped
    ports:
      - "8080:8080"
```

```bash
docker compose up -d
```

Put it behind a reverse proxy that terminates TLS and passes WebSocket upgrades through, so the app connects to `wss://your-domain/api/relay`.

## Configuration

Every setting is an environment variable with a double underscore for the section.

| Variable | Default | Meaning |
| --- | --- | --- |
| `Relay__MaxMemberCount` | `32` | members per jam, host included |
| `Relay__MaxQueueLength` | `500` | queue entries a host may publish |
| `Relay__ReconnectGrace` | `00:01:00` | how long a dropped device keeps its place |
| `Relay__IdentityTtl` | `1.00:00:00` | how long an idle install is remembered |
| `Relay__SweepInterval` | `00:00:05` | how often expired members are cleared |
| `Relay__MaxClockSkew` | `00:00:10` | how far a host's `sampledAt` may be from the server clock |
| `Relay__MaxMessageBytes` | `65536` | largest accepted message |
| `Relay__OutboundQueueSize` | `256` | messages buffered for a slow device before it is dropped |
| `Relay__KeepAliveInterval` | `00:00:20` | WebSocket ping interval |

## Scaling

Jams live in the memory of one process, so run one instance. A restart ends every jam, and devices reconnect and start again.

The container reports its health on `/health/live`.
