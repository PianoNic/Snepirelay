using System.Net;
using System.Text.Json;
using Snepirelay.Tests.Support;

namespace Snepirelay.Tests
{
    public class JamFlowTests
    {
        private static object Playback(string trackUri, long? sampledAt = null) => new
        {
            type = "playback",
            rid = "p",
            state = new
            {
                trackUri,
                contextUri = "playlist:abc",
                positionMs = 42_000,
                durationMs = 200_000,
                paused = false,
                shuffle = false,
                repeat = "off",
                queue = new[] { new { uri = "track:next", uid = "q1", addedBy = (string?)null } },
                sampledAt,
            },
        };

        private static async Task<(RelayClient Host, RelayClient Guest, string JoinToken)> JamWithGuestAsync(RelayHost relay)
        {
            var host = await relay.HelloAsync("host");
            var guest = await relay.HelloAsync("guest");

            await host.SendAsync(new { type = "create" });
            var created = await host.ExpectAsync("session_update");
            var joinToken = created.Str("session", "joinToken")!;

            await guest.SendAsync(new { type = "join", joinToken });
            await host.ExpectAsync("session_update");
            await guest.ExpectAsync("session_update");

            return (host, guest, joinToken);
        }

        [Test]
        public async Task A_host_and_two_guests_share_membership_playback_and_commands()
        {
            await using var relay = await RelayHost.StartAsync();
            var host = await relay.HelloAsync("host");
            var first = await relay.HelloAsync("first");
            var second = await relay.HelloAsync("second");

            await host.SendAsync(new { type = "create", rid = "c1" });
            var created = await host.ExpectAsync("session_update");
            await Assert.That(created.Str("reason")).IsEqualTo("YOU_JOINED");
            await Assert.That(created.At("session", "isOwner").GetBoolean()).IsTrue();
            await Assert.That((await host.ExpectAsync("ok")).Str("rid")).IsEqualTo("c1");
            var joinToken = created.Str("session", "joinToken")!;

            await first.SendAsync(new { type = "join", joinToken });
            await Assert.That((await host.ExpectAsync("session_update")).Str("reason")).IsEqualTo("USER_JOINED");
            var joined = await first.ExpectAsync("session_update");
            await Assert.That(joined.Str("reason")).IsEqualTo("YOU_JOINED");
            await Assert.That(joined.At("session", "members").GetArrayLength()).IsEqualTo(2);
            await Assert.That(joined.At("session", "isOwner").GetBoolean()).IsFalse();

            await host.SendAsync(Playback("track:one"));
            await Assert.That((await first.ExpectAsync("playback")).Str("state", "trackUri")).IsEqualTo("track:one");
            await host.ExpectAsync("ok");

            await second.SendAsync(new { type = "join", joinToken });
            await Assert.That((await host.ExpectAsync("session_update")).Str("reason")).IsEqualTo("USER_JOINED");
            await Assert.That((await first.ExpectAsync("session_update")).Str("reason")).IsEqualTo("USER_JOINED");
            await second.ExpectAsync("session_update");
            var lateState = await second.ExpectAsync("playback");
            await Assert.That(lateState.At("state", "queue").GetArrayLength()).IsEqualTo(1);
            await Assert.That(lateState.At("state", "positionMs").GetInt64()).IsEqualTo(42_000);

            await first.SendAsync(new { type = "command", command = new { kind = "pause" } });
            var forwarded = await host.ExpectAsync("command");
            await Assert.That(forwarded.Str("from")).IsEqualTo(first.MemberId);
            await Assert.That(forwarded.Str("command", "kind")).IsEqualTo("pause");

            await first.SendAsync(new { type = "command", rid = "s1", command = new { kind = "seek" } });
            var invalid = await first.ExpectAsync("error");
            await Assert.That(invalid.Str("error")).IsEqualTo("INVALID_MESSAGE");
            await Assert.That(invalid.Str("rid")).IsEqualTo("s1");

            await host.SendAsync(new { type = "command", command = new { kind = "next" } });
            await Assert.That((await host.ExpectAsync("error")).Str("error")).IsEqualTo("NOT_ALLOWED");

            var preview = await relay.Http.GetFromJsonElementAsync($"api/jams/{joinToken}");
            await Assert.That(preview.At("memberCount").GetInt32()).IsEqualTo(3);
            await Assert.That(preview.Str("hostDisplayName")).IsEqualTo("host");
        }

        [Test]
        public async Task Guest_control_limits_what_guests_may_send()
        {
            await using var relay = await RelayHost.StartAsync();
            var (host, guest, _) = await JamWithGuestAsync(relay);

            await host.SendAsync(new { type = "settings", guestControl = "queueOnly" });
            await Assert.That((await host.ExpectAsync("session_update")).Str("session", "guestControl")).IsEqualTo("queueOnly");
            await Assert.That((await guest.ExpectAsync("session_update")).Str("reason")).IsEqualTo("SETTINGS_UPDATED");

            await guest.SendAsync(new { type = "command", rid = "g1", command = new { kind = "pause" } });
            var refused = await guest.ExpectAsync("error");
            await Assert.That(refused.Str("error")).IsEqualTo("NOT_ALLOWED");
            await Assert.That(refused.Str("values", "guestControl")).IsEqualTo("QueueOnly");

            await guest.SendAsync(new { type = "command", command = new { kind = "addToQueue", uri = "track:wish" } });
            await Assert.That((await host.ExpectAsync("command")).Str("command", "uri")).IsEqualTo("track:wish");

            await host.SendAsync(new { type = "settings", guestControl = "none" });
            await host.ExpectAsync("session_update");
            await guest.ExpectAsync("session_update");

            await guest.SendAsync(new { type = "command", command = new { kind = "addToQueue", uri = "track:wish" } });
            await Assert.That((await guest.ExpectAsync("error")).Str("error")).IsEqualTo("NOT_ALLOWED");
        }

        [Test]
        public async Task The_host_kicks_and_ends_while_guests_cannot()
        {
            await using var relay = await RelayHost.StartAsync();
            var (host, kicked, joinToken) = await JamWithGuestAsync(relay);
            var stays = await relay.HelloAsync("stays");
            await stays.SendAsync(new { type = "join", joinToken });
            await host.ExpectAsync("session_update");
            await kicked.ExpectAsync("session_update");
            await stays.ExpectAsync("session_update");

            await stays.SendAsync(new { type = "kick", memberId = host.MemberId });
            await Assert.That((await stays.ExpectAsync("error")).Str("error")).IsEqualTo("NOT_HOST");

            await host.SendAsync(new { type = "kick", memberId = kicked.MemberId });
            var gone = await kicked.ExpectAsync("session_update");
            await Assert.That(gone.Str("reason")).IsEqualTo("YOU_WERE_KICKED");
            await Assert.That(gone.At("session").ValueKind).IsEqualTo(JsonValueKind.Null);
            await Assert.That((await host.ExpectAsync("session_update")).Str("reason")).IsEqualTo("USER_KICKED");
            await Assert.That((await stays.ExpectAsync("session_update")).Str("reason")).IsEqualTo("USER_KICKED");

            await kicked.SendAsync(new { type = "leave" });
            await Assert.That((await kicked.ExpectAsync("error")).Str("error")).IsEqualTo("NOT_IN_SESSION");

            await host.SendAsync(new { type = "end" });
            await Assert.That((await host.ExpectAsync("session_update")).Str("reason")).IsEqualTo("SESSION_DELETED");
            await Assert.That((await stays.ExpectAsync("session_update")).Str("reason")).IsEqualTo("SESSION_DELETED");

            var response = await relay.Http.GetAsync($"api/jams/{joinToken}");
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
            var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            await Assert.That(body.Str("error")).IsEqualTo("SESSION_NOT_FOUND");
        }

        [Test]
        public async Task A_guest_leaving_stays_quiet_and_the_host_leaving_ends_the_jam()
        {
            await using var relay = await RelayHost.StartAsync();
            var (host, guest, joinToken) = await JamWithGuestAsync(relay);

            await guest.SendAsync(new { type = "leave" });
            await Assert.That((await guest.ExpectAsync("session_update")).Str("reason")).IsEqualTo("YOU_LEFT");
            await Assert.That((await host.ExpectAsync("session_update")).Str("reason")).IsEqualTo("USER_LEFT");

            await guest.SendAsync(new { type = "join", joinToken });
            await host.ExpectAsync("session_update");
            await guest.ExpectAsync("session_update");

            await host.SendAsync(new { type = "leave" });
            await Assert.That((await host.ExpectAsync("session_update")).Str("reason")).IsEqualTo("YOU_LEFT");
            await Assert.That((await guest.ExpectAsync("session_update")).Str("reason")).IsEqualTo("SESSION_DELETED");
        }

        [Test]
        public async Task A_full_jam_refuses_one_more()
        {
            await using var relay = await RelayHost.StartAsync(maxMemberCount: 2);
            var (_, _, joinToken) = await JamWithGuestAsync(relay);
            var late = await relay.HelloAsync("late");

            await late.SendAsync(new { type = "join", joinToken });
            var full = await late.ExpectAsync("error");
            await Assert.That(full.Str("error")).IsEqualTo("SESSION_FULL");
            await Assert.That(full.Str("values", "maxMemberCount")).IsEqualTo("2");

            await late.SendAsync(new { type = "join", joinToken = "nope" });
            await Assert.That((await late.ExpectAsync("error")).Str("error")).IsEqualTo("SESSION_NOT_FOUND");
        }

        [Test]
        public async Task Playback_gets_a_server_sample_time_unless_the_host_sent_a_close_one()
        {
            await using var relay = await RelayHost.StartAsync();
            var (host, guest, _) = await JamWithGuestAsync(relay);

            await host.SendAsync(Playback("track:one"));
            await Assert.That((await guest.ExpectAsync("playback")).At("state", "sampledAt").GetInt64()).IsEqualTo(relay.NowMs);
            await host.ExpectAsync("ok");

            await host.SendAsync(Playback("track:one", sampledAt: relay.NowMs - 1_500));
            await Assert.That((await guest.ExpectAsync("playback")).At("state", "sampledAt").GetInt64()).IsEqualTo(relay.NowMs - 1_500);
            await host.ExpectAsync("ok");

            await host.SendAsync(Playback("track:one", sampledAt: relay.NowMs - 3_600_000));
            await Assert.That((await guest.ExpectAsync("playback")).At("state", "sampledAt").GetInt64()).IsEqualTo(relay.NowMs);
        }
    }
}
