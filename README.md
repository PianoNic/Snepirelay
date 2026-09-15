<p align="center">
  <img src="assets/snepirelay-icon.svg" width="180" alt="Snepirelay Logo" />
</p>
<p align="center">
  <strong>Snepirelay</strong><br/>
  Jams for every account, free ones included.
</p>
<p align="center">
  <a href="https://github.com/PianoNic/Snepirelay"><img src="https://badgetrack.pianonic.ch/badge?tag=snepirelay&label=visits&color=1DB954&style=flat" alt="visits" /></a>
  <a href="docs/self-host.md"><img src="https://img.shields.io/badge/Self--Host-Instructions-1DB954.svg" alt="Self-hosting" /></a>
  <a href="docs/protocol.md"><img src="https://img.shields.io/badge/Protocol-v1-1DB954.svg" alt="Protocol" /></a>
  <a href="https://github.com/PianoNic/Snepilatch"><img src="https://img.shields.io/badge/For-Snepilatch-1DB954.svg" alt="Snepilatch" /></a>
</p>

---

## What is Snepirelay?

A jam lets friends listen together: one person hosts, everyone hears the same song at the same moment, and guests can add to the queue or take over the controls. Usually only a paid account may host one.

Snepirelay is the server that lets [Snepilatch](https://github.com/PianoNic/Snepilatch) host jams for any account. Every member still plays the music on their own account and their own device. The relay only carries what keeps them together: the host's playback, the guests' requests, and who is in the jam.

No audio passes through it, and it never sees anyone's login.

## Features

- **Anyone can host**: a free account starts a jam the same way a premium one does.
- **The official jam, rebuilt**: join with the token an invite carries, guests queue songs or control playback, the host can kick guests or limit them to the queue, and membership changes use the same reasons an official jam does.
- **In sync**: the host's position comes with a server timestamp, and a ping gives each device its clock offset, so guests land on the same second.
- **Survives a tunnel**: a dropped connection keeps its place for a minute and comes back into the same jam.
- **Nothing stored**: jams live in memory and are gone when they end.
- **Tiny footprint**: one small container, a few kilobytes per message, no database.

## How a jam works

| Step | Host | Guest |
| --- | --- | --- |
| Connect | `hello` | `hello` |
| Start or join | `create` | `join` with the join token |
| Listen | publishes `playback` on every change | receives `playback` and follows it |
| Control | receives `command` and applies it | sends `command`, within the guest control |
| Finish | `end` or `leave` | `leave` |

The full message reference is in [the protocol](docs/protocol.md).

## Get started

- 📦 **[Self-hosting guide](docs/self-host.md)** - run the image with `docker compose`.
- 🛠️ **[Developer setup](docs/dev-setup.md)** - build, run and test locally.
- 🧩 **[Protocol](docs/protocol.md)** - every message, reason and error.

## License

[MIT](LICENSE). Copyright PianoNic.

---

<p align="center">Made with care by <a href="https://github.com/PianoNic">PianoNic</a></p>
