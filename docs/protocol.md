# Relay protocol

Version 1. One WebSocket per device at `/api/relay`, one JSON object per text frame. Every object has a `type`. Fields are camelCase, enums are camelCase strings, times are Unix milliseconds on the server clock.

Any client message may carry a `rid`. The server then answers it with `ok` or `error` carrying the same `rid`. Without a `rid`, only failures are answered.

## Connecting

The first message must be `hello`. Anything else gets `HELLO_REQUIRED` and the socket closes.

```json
{
  "type": "hello",
  "protocol": 1,
  "installId": "made once per install, 8 to 128 characters",
  "secret": "made once per install, 32 to 256 characters",
  "profile": { "userId": "spfy user id", "displayName": "Nic", "imageUrl": "https://..." },
  "device": { "deviceId": "connect device id", "name": "S25 Ultra", "type": "Smartphone" }
}
```

The server answers with `welcome`:

```json
{ "type": "welcome", "protocol": 1, "memberId": "...", "serverTime": 1789473600000, "session": null, "playback": null }
```

`installId` and `secret` are the device's identity. A later `hello` with the same pair gets the same `memberId` and, within the reconnect grace, the jam it was in: `session` is filled in, and for a guest `playback` holds the host's last state. The wrong secret for a known install gets `UNAUTHORIZED` and a closed socket. A second connection for the same install replaces the first, which is closed with the reason `replaced`.

`ping` with a `clientTime` gets `pong` with the same `clientTime` and the `serverTime`, which is enough to estimate the clock offset.

## Jams

| Client sends | Who may | What happens |
| --- | --- | --- |
| `create` | anyone | Leaves the current jam, starts one, answers `YOU_JOINED` |
| `join` with `joinToken` | anyone | Leaves the current jam, joins, answers `YOU_JOINED` and the host's playback |
| `leave` | a member | A guest leaves; the host leaving ends the jam |
| `end` | the host | Ends the jam for everyone |
| `kick` with `memberId` | the host | Removes that guest |
| `settings` with `guestControl` | the host | `full`, `queueOnly` or `none` |

Changes reach members as `session_update`:

```json
{ "type": "session_update", "reason": "USER_JOINED", "session": { }, "memberIds": ["..."] }
```

| Reason | Sent to | `session` |
| --- | --- | --- |
| `YOU_JOINED` | the device that created or joined | the jam |
| `USER_JOINED`, `USER_LEFT`, `USER_KICKED` | everyone else | the jam |
| `USER_UPDATED` | everyone else | the jam, with the member's `listening` changed |
| `SETTINGS_UPDATED` | everyone | the jam |
| `YOU_LEFT`, `YOU_WERE_KICKED` | the device concerned | null |
| `SESSION_DELETED` | everyone left in the jam | null |

A jam looks like this:

```json
{
  "sessionId": "...",
  "joinToken": "12 characters, what invite links carry",
  "ownerId": "host member id",
  "isOwner": false,
  "members": [
    {
      "id": "...", "userId": "...", "displayName": "Nic", "imageUrl": null,
      "device": { "deviceId": "...", "name": "S25 Ultra", "type": "Smartphone" },
      "isHost": true, "isCurrentUser": false, "listening": true, "joinedAt": 1789473600000
    }
  ],
  "guestControl": "full",
  "maxMemberCount": 32,
  "timestamp": 1789473600000
}
```

`timestamp` only grows. A client can drop an update older than the jam it already shows.

## Playback

The host is the source of truth. It sends `playback` whenever its state changes:

```json
{
  "type": "playback",
  "state": {
    "trackUri": "spotify:track:...",
    "contextUri": "spotify:playlist:...",
    "positionMs": 42000,
    "durationMs": 200000,
    "paused": false,
    "shuffle": false,
    "repeat": "off",
    "queue": [{ "uri": "spotify:track:...", "uid": "...", "addedBy": "member id or null" }],
    "sampledAt": 1789473600000
  }
}
```

`positionMs` is the position at `sampledAt`. The host can leave `sampledAt` out, and the server then uses the time it received the state. A value more than 10 seconds away from the server clock is replaced the same way. Guests receive the state as `playback` with the `serverTime` it was sent at, so the position now is `positionMs + (serverNow - sampledAt)` while not paused.

## Guest commands

A guest sends `command`. The server checks it and forwards it to the host as `command` with `from` set to the guest's member id. The host applies it and publishes its new playback.

| `kind` | Needs | `queueOnly` allows |
| --- | --- | --- |
| `pause`, `resume`, `next`, `previous` | nothing | no |
| `seek` | `positionMs` | no |
| `play` | `uri` or `contextUri` | no |
| `addToQueue` | `uri` | yes |
| `removeFromQueue` | `uid` or `index` | no |
| `moveQueue` | `index` and `toIndex` | no |

With `guestControl` set to `none`, guests may send nothing. The host itself gets `NOT_ALLOWED` for a command, and a guest gets `HOST_OFFLINE` while the host's connection is down.

## Dropped connections

A dropped connection keeps its place for the reconnect grace, 60 seconds by default, and the others see its `listening` turn false. Coming back within the grace restores it. After the grace a guest is removed with `USER_LEFT`, and a host ends the jam with `SESSION_DELETED`.

## Errors

```json
{ "type": "error", "rid": "...", "error": "SESSION_FULL", "values": { "maxMemberCount": "32" } }
```

| Error | Meaning |
| --- | --- |
| `HELLO_REQUIRED` | the first message was not `hello`; the socket closes |
| `UNSUPPORTED_PROTOCOL` | `values.supported` is the version to use; the socket closes |
| `UNAUTHORIZED` | the wrong secret for this install; the socket closes |
| `INVALID_MESSAGE` | malformed or missing fields, `values.reason` says which |
| `UNKNOWN_TYPE` | a `type` this server does not know |
| `MESSAGE_TOO_LARGE` | over 64 KiB; the socket closes |
| `NOT_IN_SESSION` | the action needs a jam |
| `SESSION_NOT_FOUND` | no jam has that join token |
| `SESSION_FULL` | the jam is at `values.maxMemberCount` |
| `NOT_HOST` | the action is the host's |
| `NOT_ALLOWED` | guest control forbids it, or the host sent a command |
| `MEMBER_NOT_FOUND` | the kicked member is not in the jam |
| `HOST_OFFLINE` | the host's connection is down |

## HTTP

| Route | Answer |
| --- | --- |
| `GET /api/jams/{joinToken}` | `{ hostDisplayName, hostImageUrl, memberCount, maxMemberCount }`, or 404 with `{ error, values }` |
| `GET /api/app` | `{ version, protocol }` |
| `GET /health/live` | 200 while the process runs |
