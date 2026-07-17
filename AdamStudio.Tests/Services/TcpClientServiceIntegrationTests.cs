using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AdamStudio.Services;
using AdamStudio.Services.Interfaces;
using AdamStudio.Services.TcpClientDependency;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace AdamStudio.Tests.Services
{
    /// <summary>
    /// Integration tests for TcpClientService against a real loopback TCP server.
    ///
    /// These tests start a System.Net.Sockets.TcpListener on 127.0.0.1 with a
    /// random port (port 0 = OS-assigned), then instantiate TcpClientService
    /// pointing at that endpoint. The tests verify the event-driven contract:
    /// connect, send, receive, disconnect.
    ///
    /// Why integration, not unit: TcpClientService inherits from
    /// NetCoreServer.TcpClient which wraps real socket I/O. Mocking the base
    /// class would test nothing useful — we need to verify the event wiring
    /// works end-to-end with real TCP.
    ///
    /// Tier 2 roadmap item: "Write integration test for server communication
    /// — the highest-risk area."
    /// </summary>
    public class TcpClientServiceIntegrationTests : IDisposable
    {
        private readonly TcpListener mServer;
        private readonly int mPort;
        private readonly IServiceProvider mServiceProvider;
        private readonly TcpClientService mClient;

        public TcpClientServiceIntegrationTests()
        {
            // Start a loopback TCP server on a random port.
            mServer = new TcpListener(IPAddress.Loopback, 0);
            mServer.Start();
            mPort = ((IPEndPoint)mServer.LocalEndpoint).Port;

            // Build a mock IServiceProvider that returns IServiceSettings
            // configured for our loopback endpoint.
            TcpCllientSettings tcpSettings = new(
                IPAddress.Loopback.ToString(),
                mPort,
                new TcpClientOption
                {
                    ReconnectCount = 0,       // no reconnects in tests
                    ReconnectTimeout = 1      // 1 second (won't be used)
                });

            IServiceSettings serviceSettings = Substitute.For<IServiceSettings>();
            serviceSettings.TcpCllientSettings.Returns(tcpSettings);

            mServiceProvider = Substitute.For<IServiceProvider>();
            mServiceProvider.GetService<IServiceSettings>().Returns(serviceSettings);

            mClient = new TcpClientService(mServiceProvider);
        }

        [Fact]
        public async Task ConnectAsync_ToLoopbackServer_RaisesConnectedEvent()
        {
            // Arrange
            bool connectedEventRaised = false;
            mClient.RaiseTcpCientConnectedEvent += (sender, e) => connectedEventRaised = true;

            // Act
            _ = mClient.ConnectAsync();
            // Accept the connection on the server side so the client can complete
            // the TCP handshake.
            using TcpClient serverSide = await mServer.AcceptTcpClientAsync();

            // Wait briefly for the client's OnConnected callback to fire.
            await Task.Delay(200);

            // Assert
            Assert.True(connectedEventRaised, "Connected event was not raised");
            Assert.True(mClient.IsConnected);
        }

        [Fact]
        public async Task ServerSendsData_ClientRaisesReceivedEvent()
        {
            // Arrange
            _ = mClient.ConnectAsync();
            using TcpClient serverSide = await mServer.AcceptTcpClientAsync();
            await Task.Delay(200); // wait for client OnConnected

            byte[] receivedBuffer = Array.Empty<byte>();
            long receivedSize = 0;
            mClient.RaiseTcpClientReceivedEvent += (sender, e) =>
            {
                receivedBuffer = e.Buffer;
                receivedSize = e.Size;
            };

            // Act — server sends a known byte sequence to the client.
            byte[] payload = Encoding.UTF8.GetBytes("PING");
            NetworkStream serverStream = serverSide.GetStream();
            await serverStream.WriteAsync(payload);
            await serverStream.FlushAsync();

            // Wait briefly for the client's OnReceived callback to fire.
            await Task.Delay(300);

            // Assert
            Assert.True(receivedSize > 0, "Received event was not raised");
            // The buffer may be larger than the payload (NetCoreServer reuses a
            // receive buffer); verify the first bytes match.
            byte[] actual = new byte[Math.Min(payload.Length, receivedSize)];
            Array.Copy(receivedBuffer, 0, actual, 0, actual.Length);
            Assert.Equal(payload, actual);
        }

        [Fact]
        public async Task ServerDisconnects_ClientRaisesDisconnectedEvent()
        {
            // Arrange — connect first
            _ = mClient.ConnectAsync();
            using TcpClient serverSide = await mServer.AcceptTcpClientAsync();
            await Task.Delay(200);

            bool disconnectedEventRaised = false;
            mClient.RaiseTcpClientDisconnectedEvent += (sender, e) => disconnectedEventRaised = true;

            // Act — server closes the connection
            serverSide.Close();

            // Wait for the client to detect the disconnect.
            // With ReconnectCount=0, OnDisconnected should fire the event
            // immediately (no reconnect attempts).
            await Task.Delay(500);

            // Assert
            Assert.True(disconnectedEventRaised, "Disconnected event was not raised");
            Assert.False(mClient.IsConnected);
        }

        [Fact]
        public async Task Connect_ToNonExistentServer_DoesNotConnect_AndSignalsFailure()
        {
            // Arrange — use a port that is almost certainly not listening.
            // Port 1 is reserved but typically not bound on a dev machine.
            TcpCllientSettings badSettings = new(
                IPAddress.Loopback.ToString(),
                1, // port 1 — nothing should be listening here
                new TcpClientOption { ReconnectCount = 0, ReconnectTimeout = 1 });

            IServiceSettings badServiceSettings = Substitute.For<IServiceSettings>();
            badServiceSettings.TcpCllientSettings.Returns(badSettings);

            IServiceProvider badProvider = Substitute.For<IServiceProvider>();
            badProvider.GetService<IServiceSettings>().Returns(badServiceSettings);

            bool errorEventRaised = false;
            bool disconnectedEventRaised = false;
            SocketError? capturedError = null;

            using TcpClientService badClient = new(badProvider);
            badClient.RaiseTcpClientErrorEvent += (sender, e) =>
            {
                errorEventRaised = true;
                capturedError = e.Error;
            };
            badClient.RaiseTcpClientDisconnectedEvent += (sender, e) => disconnectedEventRaised = true;

            // Act
            _ = badClient.ConnectAsync();
            // NetCoreServer may signal failure via either OnError or OnDisconnected,
            // depending on the socket error type and timing. Wait up to 3 seconds
            // for either signal.
            await Task.Delay(3000);

            // Assert — the client must not report IsConnected, and must have
            // signaled failure via at least one of the two events.
            Assert.False(badClient.IsConnected,
                "Client should not be connected to a non-existent server");

            Assert.True(errorEventRaised || disconnectedEventRaised,
                "Neither Error nor Disconnected event was raised for unreachable server. " +
                $"errorEvent={errorEventRaised}, disconnectedEvent={disconnectedEventRaised}");

            // If the error event did fire, the error code must not be Success.
            if (errorEventRaised)
            {
                Assert.NotNull(capturedError);
                Assert.NotEqual(SocketError.Success, capturedError!.Value);
            }
        }

        [Fact]
        public async Task DisconnectAndStop_RaisesDisconnectedEvent()
        {
            // Arrange — connect first
            _ = mClient.ConnectAsync();
            using TcpClient serverSide = await mServer.AcceptTcpClientAsync();
            await Task.Delay(200);

            bool disconnectedEventRaised = false;
            mClient.RaiseTcpClientDisconnectedEvent += (sender, e) => disconnectedEventRaised = true;

            // Act
            mClient.DisconnectAndStop();

            // Assert
            Assert.True(disconnectedEventRaised, "Disconnected event was not raised on DisconnectAndStop");
            Assert.False(mClient.IsConnected);
        }

        [Fact]
        public async Task ReconnectCount_Property_ReturnsConfiguredValue()
        {
            // The ReconnectCount property should return the value from
            // TcpClientOption (0 in our test configuration).
            Assert.Equal(0, mClient.ReconnectCount);
        }

        [Fact]
        public async Task ReconnectTimeout_Property_ReturnsConfiguredValue()
        {
            Assert.Equal(1, mClient.ReconnectTimeout);
        }

        public void Dispose()
        {
            try
            {
                mClient.DisconnectAndStop();
                mClient.Dispose();
            }
            catch
            {
                // Best-effort cleanup — ignore errors during teardown.
            }

            mServer.Stop();
        }
    }
}
