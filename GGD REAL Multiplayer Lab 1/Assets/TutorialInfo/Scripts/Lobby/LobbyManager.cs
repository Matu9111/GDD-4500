using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

namespace GDD4500.LAB01
{
    public class LobbyManager : MonoBehaviour
    {

        private List<PlayerInputHandler> _existingPlayers;

        [SerializeField] private string _GameplaySceneName = "Game";
        [SerializeField] private Collider _startTrigger;
        [SerializeField] private TextMeshPro _playerCountText;

        bool _matchStarted = false;

        private void Start()
        {
            _existingPlayers = PlayerManager.Instance.GetPlayers();

            PlayerManager.Instance.OnPlayerJoined += OnPlayerJoined;
        }

        private void OnDestroy()
        {
            PlayerManager.Instance.OnPlayerJoined -= OnPlayerJoined;
        }

        private void OnPlayerJoined(PlayerInputContext ctx)
        {
            _existingPlayers.Add(ctx.Handler);

            Debug.Log($"Player {ctx.Index + 1} joined lobby");
        }

        private void Update()
        {
            if (_matchStarted) return;
            
            // Count the number of players in the start trigger
            int playersInTrigger = 0;
            foreach (var player in _existingPlayers)
            {
                if (_startTrigger.bounds.Contains(player.transform.position))
                {
                    playersInTrigger++;
                }
            }

            // If all players are in the start trigger, start the match
            if (_existingPlayers.Count > 0 && playersInTrigger == _existingPlayers.Count)
            {
                StartCoroutine(StartMatch());
            }

            UpdatePlayerCountUI(playersInTrigger);
        }

        private void UpdatePlayerCountUI(int playersInTrigger)
        {
            // Update the player count text
            _playerCountText.text = $"Players in Start: {playersInTrigger}/{_existingPlayers.Count}";
        }

        private IEnumerator StartMatch()
        {
            // Call the StartMatch function from PlayerManager
            PlayerManager.Instance.StartMatch();
            print("Match started");
            _matchStarted = true;

            yield return new WaitForSeconds(1f);

            SceneManager.LoadScene(_GameplaySceneName);
        }
    }
}
