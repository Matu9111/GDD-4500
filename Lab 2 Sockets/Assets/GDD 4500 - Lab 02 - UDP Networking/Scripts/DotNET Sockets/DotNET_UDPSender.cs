// Written by ChatGPT
// Sends one UDP message to the given host:port via Inspector
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DotNET_UDPSender : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string destinationHost = "127.0.0.1";
    [SerializeField] private int destinationPort = 7778;

    private static readonly Encoding Utf8 = new UTF8Encoding(false);

    public async Task SendMessageAsync(string message)
    {
        await SendOnceAsync(message);
        Debug.Log($"[UDP Sender] Sent {message} to {destinationHost}:{destinationPort}");
    }

    private async Task SendOnceAsync(string text)
    {
        using var udpSender = new UdpClient(); // no bind → OS picks ephemeral local port
        byte[] payload = Utf8.GetBytes(text);

        try
        {
            int bytesSent = await udpSender.SendAsync(payload, payload.Length, destinationHost, destinationPort);
            Debug.Log($"[UDP Sender] Sent {bytesSent} bytes to {destinationHost}:{destinationPort}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[UDP Sender] Error: {ex.Message}");
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(DotNET_UDPSender))]
    public class UdpSenderEditor : Editor
    {
        private string messageText = "Ping via UDP";

        public override void OnInspectorGUI()
        {
            DotNET_UDPSender udpSender = (DotNET_UDPSender)target;

            // Draw the default inspector
            DrawDefaultInspector();

            GUILayout.Space(20);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter play mode to send messages.", MessageType.Info);
                return;
            }

            // Add a text field for message input
            messageText = EditorGUILayout.TextField("Message Text", messageText, GUILayout.Height(40));

            GUILayout.Space(10);

            // Add a button to send the message
            if (GUILayout.Button("Send Message", GUILayout.Height(20)))
            {
                // Send the message asynchronously
                udpSender.SendMessageAsync(messageText).ConfigureAwait(false);
            }
        }
    }
    #endif

}
