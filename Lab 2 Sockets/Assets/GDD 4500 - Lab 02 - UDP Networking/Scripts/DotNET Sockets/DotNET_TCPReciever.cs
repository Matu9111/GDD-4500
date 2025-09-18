// Written by ChatGPT
// Listens on a TCP port, accepts clients, and echoes each line back.
// Attach to a GameObject in one project/play session.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class DotNET_TCPReciever : MonoBehaviour
{
    [Header("Listen Settings")]
    public int listenPort = 7777;

    [Header("Runtime (read-only)")]
    [SerializeField] private string status = "Idle";
    [SerializeField] private string lastMessage = "";
    [SerializeField] private int connectedClients = 0;

    private TcpListener _listener;
    private CancellationTokenSource _cts;

    private static readonly Encoding Utf8 = new UTF8Encoding(false);

    private void Start()
    {
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, listenPort);
        _listener.Start();
        status = $"Listening on 0.0.0.0:{listenPort}";
        _ = AcceptLoopAsync(_cts.Token);
    }

    private async Task AcceptLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                Interlocked.Increment(ref connectedClients);
                _ = HandleClientAsync(client, token);
            }
        }
        catch (ObjectDisposedException) { /* stopping */ }
        catch (Exception ex) { status = $"Listener error: {ex.Message}"; }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        var remote = client.Client.RemoteEndPoint?.ToString() ?? "<unknown>";
        try
        {
            using (client)
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream, Utf8, false, 1024, true))
            using (var writer = new StreamWriter(stream, Utf8, 1024, true) { AutoFlush = true })
            {
                status = $"Client connected: {remote}";
                string line;
                while (!token.IsCancellationRequested && (line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    lastMessage = $"From {remote}: \"{line}\"";
                    await writer.WriteLineAsync($"echo: {line}").ConfigureAwait(false);
                }
            }
        }
        catch (IOException) { /* peer closed */ }
        catch (Exception ex) { status = $"Client error ({remote}): {ex.Message}"; }
        finally
        {
            Interlocked.Decrement(ref connectedClients);
            status = $"Listening on 0.0.0.0:{listenPort}  (clients: {connectedClients})";
        }
    }

    private void OnDestroy()
    {
        try { _cts?.Cancel(); } catch { }
        try { _listener?.Stop(); } catch { }
        _cts?.Dispose();
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(DotNET_TCPReciever))]
public class TcpReceiverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var rx = (DotNET_TCPReciever)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(GetPrivateField<string>(rx, "status") ?? "", MessageType.None);
        EditorGUILayout.LabelField("Last Message");
        EditorGUILayout.TextArea(GetPrivateField<string>(rx, "lastMessage") ?? "", GUILayout.MinHeight(40));
    }

    // Helper to read private serialized fields nicely
    private static T GetPrivateField<T>(object obj, string field)
    {
        var f = obj.GetType().GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return f != null ? (T)f.GetValue(obj) : default;
    }
}
#endif
