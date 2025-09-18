// Written by ChatGPT
// Listens for UDP datagrams on a local port and logs the sender + message.
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DotNET_UDPReceiver : MonoBehaviour
{
    [Header("Listen Settings")]
    [SerializeField] private int listenPort = 7778;

    private UdpClient _udpListener;
    private CancellationTokenSource _cts;

    private static readonly Encoding Utf8 = new UTF8Encoding(false);

    private static string _receivedMessage = "";
    public string ReceivedMessage => _receivedMessage;

    private static string _listeningIP = "";
    public string ListeningIP => _listeningIP;

    private static int _listeningPort = 0;
    public int ListeningPort => _listeningPort;

    private void Start()
    {
        _cts = new CancellationTokenSource();

        // Bind the UDP socket to the specified local port.
        _udpListener = new UdpClient(listenPort);
        _listeningIP = _udpListener.Client.LocalEndPoint.ToString();
        _listeningPort = listenPort;

        Debug.Log($"[UDP Receiver] Listening on {_listeningIP}:{_listeningPort}");

        // Start a background loop so we don't block Unity's main thread.
        _ = ReceiveLoopAsync(_cts.Token);
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                // Wait for the next datagram. This yields without blocking the main thread.
                UdpReceiveResult result = await _udpListener.ReceiveAsync();

                IPEndPoint senderEndPoint = result.RemoteEndPoint;
                byte[] receivedBytes = result.Buffer;
                string receivedText = Utf8.GetString(receivedBytes);

                _receivedMessage = receivedText;

                Debug.Log($"[UDP Receiver] From {senderEndPoint} → \"{receivedText}\" (len={receivedBytes.Length})");
            }
        }
        catch (ObjectDisposedException)
        {
            // Expected when we dispose while awaiting ReceiveAsync during shutdown.
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[UDP Receiver] Error: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        // Stop the loop and release the socket cleanly.
        try { _cts?.Cancel(); } catch { /* ignore */ }
        _udpListener?.Close();
        _udpListener?.Dispose();
        _cts?.Dispose();
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(DotNET_UDPReceiver))]
    public class UdpSenderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DotNET_UDPReceiver udpSender = (DotNET_UDPReceiver)target;

            // Draw the default inspector
            DrawDefaultInspector();

            GUILayout.Space(20);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter play mode to receive messages.", MessageType.Info);
                return;
            }

            if (string.IsNullOrEmpty(udpSender.ReceivedMessage))
            {
                EditorGUILayout.HelpBox("No message received yet.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Message received.", MessageType.Info);
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Listening on IP:", udpSender.ListeningIP);
            EditorGUILayout.LabelField("Listening on Port:", udpSender.ListeningPort.ToString());
        }   
    }
    #endif
}
