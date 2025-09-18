// Written by ChatGPT
// Connects to a TCP server and lets you send lines; shows last response.
// Attach to a GameObject in another project/play session (or the same).

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
public class DotNET_TCPSender : MonoBehaviour
{
    [Header("Destination")]
    public string host = "127.0.0.1";
    public int port = 7777;

    [Header("Runtime (read-only)")]
    [SerializeField] private string connectionStatus = "Disconnected";
    [SerializeField] private string lastResponse = "";

    private TcpClient _client;
    private StreamReader _reader;
    private StreamWriter _writer;
    private CancellationTokenSource _cts;

    private static readonly Encoding Utf8 = new UTF8Encoding(false);

    private async void Start()
    {
        _cts = new CancellationTokenSource();
        await EnsureConnectedAsync(_cts.Token);
    }

    private async Task EnsureConnectedAsync(CancellationToken token)
    {
        if (_client != null && _client.Connected) return;

        try
        {
            connectionStatus = $"Connecting to {host}:{port}...";
            _client = new TcpClient();
            _client.NoDelay = true; // reduce Nagle latency for tiny messages
            await _client.ConnectAsync(host, port);
            var stream = _client.GetStream();
            _reader = new StreamReader(stream, Utf8, false, 1024, true);
            _writer = new StreamWriter(stream, Utf8, 1024, true) { AutoFlush = true };
            connectionStatus = $"Connected: Local={_client.Client.LocalEndPoint} Remote={_client.Client.RemoteEndPoint}";
            _ = ReceiveLoopAsync(token);
        }
        catch (Exception ex)
        {
            connectionStatus = $"Connect failed: {ex.Message}";
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && _client?.Connected == true)
            {
                string line = await _reader.ReadLineAsync().ConfigureAwait(false);
                if (line == null) break; // server closed
                lastResponse = line;
            }
        }
        catch (IOException) { /* closed */ }
        catch (Exception ex) { connectionStatus = $"Receive error: {ex.Message}"; }
        finally
        {
            connectionStatus = "Disconnected";
        }
    }

    /// <summary>Sends one line (with newline framing). Called from the custom inspector.</summary>
    public async Task SendMessageAsync(string message)
    {
        if (_writer == null || _client?.Connected != true)
        {
            await EnsureConnectedAsync(_cts.Token);
        }

        if (_writer != null && _client?.Connected == true)
        {
            try { await _writer.WriteLineAsync(message); }
            catch (Exception ex) { connectionStatus = $"Send error: {ex.Message}"; }
        }
    }

    private void OnDestroy()
    {
        try { _cts?.Cancel(); } catch { }
        try { _client?.Close(); } catch { }
        _client?.Dispose();
        _cts?.Dispose();
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(DotNET_TCPSender))]
public class TcpSenderEditor : Editor
{
    private string messageText = "Hello via TCP";

    public override void OnInspectorGUI()
    {
        var sender = (DotNET_TCPSender)target;

        // Draw the default inspector
        DrawDefaultInspector();

        GUILayout.Space(20);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to send messages.", MessageType.Info);
            return;
        }

        // Connection status & last response
        EditorGUILayout.LabelField("Connection", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(GetPrivateField<string>(sender, "connectionStatus") ?? "", MessageType.None);
        EditorGUILayout.LabelField("Last Response");
        EditorGUILayout.TextArea(GetPrivateField<string>(sender, "lastResponse") ?? "", GUILayout.MinHeight(40));

        GUILayout.Space(10);

        // Message entry + send
        messageText = EditorGUILayout.TextField("Message Text", messageText, GUILayout.Height(20));
        if (GUILayout.Button("Send Message", GUILayout.Height(22)))
        {
            sender.SendMessageAsync(messageText).ConfigureAwait(false);
        }
    }

    // Helper to read private serialized fields nicely
    private static T GetPrivateField<T>(object obj, string field)
    {
        var f = obj.GetType().GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return f != null ? (T)f.GetValue(obj) : default;
    }
}
#endif
