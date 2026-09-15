<p align="center">
  <img src="assets/snepirelay-icon.svg" width="180" alt="Snepirelay Logo" />
</p>
<p align="center">
  <strong>Snepirelay</strong><br/>
  The relay that keeps Snepilatch devices in step.
</p>
<p align="center">
  <a href="https://github.com/PianoNic/Snepirelay"><img src="https://badgetrack.pianonic.ch/badge?tag=snepirelay&label=visits&color=1DB954&style=flat" alt="visits" /></a>
  <a href="docs/self-host.md"><img src="https://img.shields.io/badge/Self--Host-Instructions-1DB954.svg" alt="Self-hosting" /></a>
  <a href="https://github.com/PianoNic/Snepilatch"><img src="https://img.shields.io/badge/For-Snepilatch-1DB954.svg" alt="Snepilatch" /></a>
</p>

---

## What is Snepirelay?

Some things in [Snepilatch](https://github.com/PianoNic/Snepilatch) need phones to talk to each other: one device changes something, and others have to follow within a heartbeat. Snepirelay is the small server in the middle. Every device keeps one live connection to it, and it passes messages between them the moment they arrive.

It never plays or stores music and never sees anyone's login. It only knows which devices belong together and what they told each other.

Today it powers **jams**: friends listening together, with any account able to host. More of Snepilatch will run through it over time.

## Features

- **Survives a tunnel**: a device that drops out keeps its place for a minute.
- **Nothing on disk**: everything lives in memory, a restart starts fresh.

## Jams

A jam lets friends listen together: one person hosts, everyone hears the same song at the same moment, and guests can add to the queue or take over the controls. Usually only a paid account may host one. Through Snepirelay any account can.

Every member still plays the music on their own account and device. The relay carries the host's playback, the guests' requests and who is in the jam.

| Step | Host | Guest |
| --- | --- | --- |
| Connect | `hello` | `hello` |
| Start or join | `create` | `join` with the invite's token |
| Listen | publishes `playback` on every change | receives `playback` and follows it |
| Control | receives `command` and applies it | sends `command`, if the host allows it |
| Finish | `end` or `leave` | `leave` |

The host decides how much guests may do: everything, only add to the queue, or nothing. The host can also remove a guest.

## Get started

- 📦 **[Self-hosting guide](docs/self-host.md)** - run the image with `docker compose`.
- 🛠️ **[Developer setup](docs/dev-setup.md)** - build, run and test locally.
- 💬 **[Messages](docs/messages.md)** - every message, reason and error the relay speaks.

## License

[MIT](LICENSE). Copyright PianoNic.

---

<p align="center">Made with care by <a href="https://github.com/PianoNic">PianoNic</a></p>
