using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Eisenholz.AssetCatalog.Editor.Accelerator
{
    /// <summary>Lightweight reachability probe for the Accelerator's agent port (a TCP connect).</summary>
    public static class AcceleratorHealthCheck
    {
        public readonly struct ProbeResult
        {
            public readonly bool Ok;
            public readonly string Message;

            public ProbeResult(bool ok, string message)
            {
                Ok = ok;
                Message = message;
            }
        }

        public static async Task<ProbeResult> ProbeAsync(string host, int port, int timeoutMs = 3000)
        {
            if (string.IsNullOrEmpty(host))
                return new ProbeResult(false, "Host is empty — set it first.");
            if (port <= 0 || port > 65535)
                return new ProbeResult(false, $"Invalid port: {port}.");

            var client = new TcpClient();
            try
            {
                var connectTask = client.ConnectAsync(host, port);
                var completed = await Task.WhenAny(connectTask, Task.Delay(timeoutMs));

                if (completed != connectTask)
                {
                    // Don't leave the connect attempt's exception unobserved when we dispose below.
                    _ = connectTask.ContinueWith(t => { _ = t.Exception; }, TaskScheduler.Default);
                    return new ProbeResult(false, $"Timed out after {timeoutMs} ms connecting to {host}:{port}.");
                }

                await connectTask; // surface any connection error
                return client.Connected
                    ? new ProbeResult(true, $"Reachable at {host}:{port}.")
                    : new ProbeResult(false, "Could not connect.");
            }
            catch (Exception e)
            {
                return new ProbeResult(false, $"{host}:{port} — {e.Message}");
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}
