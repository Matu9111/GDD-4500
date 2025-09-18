using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace GDD4500.LAB02
{
    public class Server : MonoBehaviour
    {
        [SerializeField] Text consoleUI;
        [SerializeField] Player playerPrefab;
        [SerializeField] ParticleSystem _ExplosionParticles;

        List<Player> players = new List<Player>();

        [Header("Listen Settings")]
        [SerializeField] private int listenPort = 7778;

        private UdpClient _udpListener;
        private CancellationTokenSource _cts;

        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        private static string _listeningIP = "";

        private static int _listeningPort = 0;

        #region Server Networking

        private void Start()
        {
            _udpListener = new UdpClient(listenPort);
            _listeningIP = _udpListener.Client.LocalEndPoint.ToString();
            _listeningPort = listenPort;

            _cts = new CancellationTokenSource();

            _ = ReceiveLoopAsync(_cts.Token);
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                UdpReceiveResult result = await _udpListener.ReceiveAsync();

                byte[] receivedBytes = result.Buffer;
                string receievedText = Utf8.GetString(receivedBytes);

                SpawnClient(receievedText);
            }
        }

        private void OnDestroy()
        {
            try
            {
                _cts?.Cancel();
            }
            catch 
            {
                
            }

            _udpListener?.Close();
            _udpListener?.Dispose();
            _cts?.Dispose();
        }

        #endregion

        public void SpawnClient(string name)
        {
            foreach (Player existingPlayer in players)
            {
                if (existingPlayer.name == name)
                {
                    existingPlayer.AddImpulse();
                    return;
                }
            }

            Player player = Instantiate(playerPrefab, transform);
            player.Initialize(name, this);

            players.Add(player);

            consoleUI.text = $"Spawned client: {name}";
        }

        public void OnPlayerDestroyed(Player player)
        {
            players.Remove(player);
            var explosionParticles = Instantiate(_ExplosionParticles, player.transform.position, Quaternion.identity);
            explosionParticles.Play();
            Destroy(explosionParticles.gameObject, explosionParticles.main.duration);
        }
    }
}
