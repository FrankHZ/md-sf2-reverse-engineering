using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace Sf2.Remake.GodotAdapter.Observation;

// The socket worker exchanges only strings with the game thread. It admits the
// next client after dispatch of every complete preceding request.
internal sealed class DebugPort : IDisposable
{
    internal sealed record Request(string Line)
    {
        internal TaskCompletionSource<string> Reply { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _stop = new();
    private readonly Channel<Request> _inbox = Channel.CreateBounded<Request>(1);
    private readonly Task _worker;

    internal DebugPort(int port)
    {
        _listener = new TcpListener(IPAddress.Loopback, port) { ExclusiveAddressUse = true };
        _listener.Start(1);
        _worker = Task.Run(Serve);
    }

    internal bool TryRead(out Request? request) => _inbox.Reader.TryRead(out request);
    internal const int MaxReplyBytes = 1024 * 1024;

    private async Task<string> Dispatch(string line)
    {
        var request = new Request(line);
        await _inbox.Writer.WriteAsync(request, _stop.Token);
        return await request.Reply.Task.WaitAsync(_stop.Token);
    }

    private async Task Serve()
    {
        try
        {
            while (!_stop.IsCancellationRequested)
            {
                using var client = await _listener.AcceptTcpClientAsync(_stop.Token);
                try { await Exchange(client); }
                catch (Exception error) when (error is IOException or SocketException or OperationCanceledException)
                { /* Disconnect, idle deadline or shutdown. */ }
            }
        }
        catch (Exception error) when (_stop.IsCancellationRequested &&
            error is OperationCanceledException or SocketException or ObjectDisposedException) { }
    }

    private async Task Exchange(TcpClient client)
    {
        client.NoDelay = true;
        using var stream = client.GetStream();
        byte[] buffer = new byte[256];
        byte[] line = new byte[64];
        int length = 0;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(_stop.Token);
        deadline.CancelAfter(TimeSpan.FromSeconds(60));
        while (true)
        {
            int count = await stream.ReadAsync(buffer, deadline.Token);
            if (count == 0) return;
            for (int i = 0; i < count; i++)
            {
                byte value = buffer[i];
                if (value != '\n' && length < line.Length && (value == '\r' || value is >= 32 and <= 126))
                {
                    line[length++] = value;
                    continue;
                }
                bool valid = value == '\n';
                string response = valid
                    ? await Dispatch(Encoding.ASCII.GetString(line, 0, length).TrimEnd('\r'))
                    : "{\"ok\":false,\"error\":\"invalid-frame\"}";
                if (Encoding.UTF8.GetByteCount(response) + 1 > MaxReplyBytes)
                    response = "{\"ok\":false,\"error\":\"observation-too-large\"}";
                await stream.WriteAsync(Encoding.UTF8.GetBytes(response + "\n"), deadline.Token);
                if (!valid) return;
                length = 0;
                deadline.CancelAfter(TimeSpan.FromSeconds(60));
            }
        }
    }

    public void Dispose()
    {
        _stop.Cancel();
        _listener.Stop();
        // Cancellation also breaks a pending main-thread reply. The worker
        // never waits for the scene during shutdown.
        _worker.GetAwaiter().GetResult();
        _stop.Dispose();
    }
}
