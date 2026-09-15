using System.Net;
using System.Text.Json;
using Snepirelay.Tests.Support;

namespace Snepirelay.Tests
{
    public class ConnectionTests
    {
        [Test]
        public async Task A_dropped_guest_can_come_back_within_the_grace_and_is_removed_after_it()
        {
            await using var relay = await RelayHost.StartAsync();
            var host = await relay.HelloAsync("host");
            var guest = await relay.HelloAsync("guest");
            await host.SendAsync(new { type = "create" });
            var joinToken = (await host.ExpectAsync("session_update")).Str("session", "joinToken");
            await guest.SendAsync(new { type = "join", joinToken });
            await host.ExpectAsync("session_update");
            await guest.ExpectAsync("session_update");

            await guest.CloseAsync();
            var dropped = await host.ExpectAsync("session_update");
            await Assert.That(dropped.Str("reason")).IsEqualTo("USER_UPDATED");
            await Assert.That(ListeningOf(dropped, guest.MemberId)).IsFalse();

            relay.Time.Advance(TimeSpan.FromSeconds(30));
            await relay.SweepAsync();

            var back = await relay.ConnectAsync();
            var welcome = await back.HelloAsync("guest", sameInstallAs: guest);
            await Assert.That(welcome.Type()).IsEqualTo("welcome");
            await Assert.That(welcome.Str("memberId")).IsEqualTo(guest.MemberId);
            await Assert.That(welcome.Str("session", "joinToken")).IsEqualTo(joinToken);
            await Assert.That(ListeningOf(await host.ExpectAsync("session_update"), guest.MemberId)).IsTrue();

            await back.CloseAsync();
            await host.ExpectAsync("session_update");
            relay.Time.Advance(TimeSpan.FromSeconds(61));
            await relay.SweepAsync();

            var removed = await host.ExpectAsync("session_update");
            await Assert.That(removed.Str("reason")).IsEqualTo("USER_LEFT");
            await Assert.That(removed.At("session", "members").GetArrayLength()).IsEqualTo(1);
        }

        [Test]
        public async Task A_host_that_stays_away_past_the_grace_ends_the_jam()
        {
            await using var relay = await RelayHost.StartAsync();
            var host = await relay.HelloAsync("host");
            var guest = await relay.HelloAsync("guest");
            await host.SendAsync(new { type = "create" });
            var joinToken = (await host.ExpectAsync("session_update")).Str("session", "joinToken");
            await guest.SendAsync(new { type = "join", joinToken });
            await host.ExpectAsync("session_update");
            await guest.ExpectAsync("session_update");

            await host.CloseAsync();
            await guest.ExpectAsync("session_update");

            await guest.SendAsync(new { type = "command", command = new { kind = "pause" } });
            await Assert.That((await guest.ExpectAsync("error")).Str("error")).IsEqualTo("HOST_OFFLINE");

            relay.Time.Advance(TimeSpan.FromSeconds(61));
            await relay.SweepAsync();

            var ended = await guest.ExpectAsync("session_update");
            await Assert.That(ended.Str("reason")).IsEqualTo("SESSION_DELETED");
            await Assert.That(ended.At("session").ValueKind).IsEqualTo(JsonValueKind.Null);
        }

        [Test]
        public async Task The_secret_guards_an_install_and_a_second_connection_replaces_the_first()
        {
            await using var relay = await RelayHost.StartAsync();
            var first = await relay.HelloAsync("phone");

            var impostor = await relay.ConnectAsync();
            var refused = await impostor.HelloAsync("phone", sameInstallAs: first, secret: new string('x', 64));
            await Assert.That(refused.Str("error")).IsEqualTo("UNAUTHORIZED");
            await Assert.That(await impostor.ExpectClosedAsync()).IsEqualTo("unauthorized");

            var second = await relay.ConnectAsync();
            var welcome = await second.HelloAsync("phone", sameInstallAs: first);
            await Assert.That(welcome.Str("memberId")).IsEqualTo(first.MemberId);
            await Assert.That(await first.ExpectClosedAsync()).IsEqualTo("replaced");
        }

        [Test]
        public async Task Bad_input_gets_an_error_and_only_a_missing_hello_or_protocol_closes()
        {
            await using var relay = await RelayHost.StartAsync();

            var early = await relay.ConnectAsync();
            await early.SendAsync(new { type = "create", rid = "r" });
            var helloRequired = await early.ExpectAsync("error");
            await Assert.That(helloRequired.Str("error")).IsEqualTo("HELLO_REQUIRED");
            await Assert.That(helloRequired.Str("rid")).IsEqualTo("r");
            await Assert.That(await early.ExpectClosedAsync()).IsEqualTo("hello_required");

            var old = await relay.ConnectAsync();
            var unsupported = await old.HelloAsync("old", protocol: 2);
            await Assert.That(unsupported.Str("error")).IsEqualTo("UNSUPPORTED_PROTOCOL");
            await Assert.That(unsupported.Str("values", "supported")).IsEqualTo("1");

            var client = await relay.HelloAsync("client");
            await client.SendAsync(new { type = "dance" });
            await Assert.That((await client.ExpectAsync("error")).Str("error")).IsEqualTo("UNKNOWN_TYPE");

            await client.SendRawAsync("{not json");
            await Assert.That((await client.ExpectAsync("error")).Str("error")).IsEqualTo("INVALID_MESSAGE");

            await client.SendAsync(new { type = "join" });
            await Assert.That((await client.ExpectAsync("error")).Str("error")).IsEqualTo("INVALID_MESSAGE");

            await client.SendAsync(new { type = "ping", clientTime = 7, rid = "t" });
            var pong = await client.ExpectAsync("pong");
            await Assert.That(pong.At("clientTime").GetInt64()).IsEqualTo(7);
            await Assert.That(pong.At("serverTime").GetInt64()).IsEqualTo(relay.NowMs);
        }

        [Test]
        public async Task The_app_endpoint_and_health_answer()
        {
            await using var relay = await RelayHost.StartAsync();

            var app = await relay.Http.GetFromJsonElementAsync("api/app");
            await Assert.That(app.At("protocol").GetInt32()).IsEqualTo(1);

            var health = await relay.Http.GetAsync("health/live");
            await Assert.That(health.StatusCode).IsEqualTo(HttpStatusCode.OK);
        }

        private static bool ListeningOf(JsonElement update, string memberId) =>
            update.At("session", "members").EnumerateArray().Single(m => m.Str("id") == memberId).At("listening").GetBoolean();
    }
}
