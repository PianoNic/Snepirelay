using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Snepirelay.API.Extensions;
using Snepirelay.Application.Command.Maintenance;

namespace Snepirelay.Tests.Support
{
    public sealed class RelayHost : IAsyncDisposable
    {
        private readonly WebApplication _app;

        private RelayHost(WebApplication app, FakeTimeProvider time)
        {
            _app = app;
            Time = time;
        }

        public FakeTimeProvider Time { get; }
        public TestServer Server => _app.GetTestServer();
        public HttpClient Http => Server.CreateClient();
        public long NowMs => Time.GetUtcNow().ToUnixTimeMilliseconds();

        public static async Task<RelayHost> StartAsync(int maxMemberCount = 32)
        {
            var time = new FakeTimeProvider(new DateTimeOffset(2026, 9, 15, 12, 0, 0, TimeSpan.Zero));
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
            builder.WebHost.UseTestServer();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Relay:MaxMemberCount"] = $"{maxMemberCount}",
                ["Relay:SweepInterval"] = "1.00:00:00",
            });
            builder.Services.AddSingleton<TimeProvider>(time);
            builder.Services.AddSnepirelay(builder.Configuration);

            var app = builder.Build();
            app.MapSnepirelay();
            await app.StartAsync();
            return new RelayHost(app, time);
        }

        public async Task<RelayClient> ConnectAsync()
        {
            var socket = await Server.CreateWebSocketClient().ConnectAsync(new Uri(Server.BaseAddress, SnepirelayExtensions.RelayPath), CancellationToken.None);
            return new RelayClient(socket);
        }

        public async Task<RelayClient> HelloAsync(string displayName)
        {
            var client = await ConnectAsync();
            await client.HelloAsync(displayName);
            return client;
        }

        public async Task SweepAsync() => await _app.Services.GetRequiredService<IMediator>().Send(new SweepCommand());

        public async ValueTask DisposeAsync()
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
