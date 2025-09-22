//This script was generated with ChatGPT 5 by Alex Johnstone

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace GDD4500.LAB01
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

        public Action<PlayerInputContext> OnPlayerJoined;

        [Header("Asset & Maps")]
        [SerializeField] private InputActionAsset sharedAsset;    // your .inputactions (has Lobby + Gameplay)
        [SerializeField] private string lobbyMapName = "Lobby";
        [SerializeField] private string gameplayMapName = "Gameplay";
        [Space]
        [SerializeField] private string joinActionName = "Join";

        [Header("Spawning")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private int maxPlayers = 4;

        [Header("Schemes")]
        [SerializeField] private string gamepadSchemeName = "Gamepad";
        [SerializeField] private string[] keyboardSchemeNames = { "Keyboard Left", "Keyboard Right" };

        [Header("Activation")]
        [SerializeField] private bool enableGameplayOnJoin = false; // set true if you want immediate gameplay control



        private readonly List<PlayerInputContext> _players = new();
        private readonly HashSet<string> _claimedKBMSchemes = new();
        private InputAction _joinAction;

        private void Awake()
        {
            // Singleton boilerplate
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;

            // Ensure this object is not destroyed when the scene is unloaded
            DontDestroyOnLoad(this.gameObject);
        }

        void OnEnable()
        {
            var lobby = sharedAsset.FindActionMap(lobbyMapName, true);
            _joinAction = lobby.FindAction(joinActionName, true);

            // NOTE: This action listens globally (not tied to any InputUser).
            _joinAction.performed += OnJoinPerformed;
            _joinAction.Enable();
        }

        void OnDisable()
        {
            if (_joinAction != null)
            {
                _joinAction.performed -= OnJoinPerformed;
                _joinAction.Disable();
            }
        }

        private void OnJoinPerformed(InputAction.CallbackContext ctx)
        {
            if (_players.Count >= maxPlayers) return;

            var control = ctx.control;
            if (control == null) return;

            var device = control.device;

            // Determine which binding (and thus which group) triggered this.
            string bindingGroup = ResolveBindingGroup(_joinAction, control);
            string schemeToUse = ChooseScheme(bindingGroup, device);
            if (schemeToUse == null)
            {
                Debug.Log($"[Lobby] No valid scheme available for {device} (bindingGroup={bindingGroup}).");
                return;
            }

            // Prevent reusing a gamepad already paired; allow keyboard reuse for split keyboard.
            if (device is Gamepad && IsDevicePaired(device))
            {
                Debug.Log($"[Lobby] {device.displayName} is already taken.");
                return;
            }




            // If it’s a KBM scheme that’s already in use, try the sibling; otherwise fail.
            if (IsKBMScheme(schemeToUse) && _claimedKBMSchemes.Contains(schemeToUse))
            {

                // var alt = GetAlternateKBMScheme();
                // if (alt != null) schemeToUse = alt;
                // else
                // {
                    Debug.Log("[Lobby] Keyboard scheme taken.");
                    return;
                //}
            }

            // Build the device list to pair.
            var toPair = new List<InputDevice>();
            if (device is Gamepad gp)
            {
                toPair.Add(gp);
            }
            else if (device is Keyboard || device is Mouse)
            {
                // For split keyboard, we pair the Keyboard (optionally Mouse) to each user.
                if (Keyboard.current != null) toPair.Add(Keyboard.current);
            }
            else
            {
                // Unsupported device type for join
                Debug.Log($"[Lobby] Ignoring device type {device?.GetType().Name} for join.");
                return;
            }

            CreatePlayer(toPair, schemeToUse);
        }

        private void CreatePlayer(List<InputDevice> devices, string scheme)
        {
            // Make a per-player clone of the whole asset
            var perPlayer = Instantiate(sharedAsset);

            // Create user and pair devices
            var user = InputUser.PerformPairingWithDevice(devices[0]);
            for (int i = 1; i < devices.Count; i++)
                InputUser.PerformPairingWithDevice(devices[i], user);

            // Associate actions with the user (so scheme activation applies a binding mask)
            user.AssociateActionsWithUser(perPlayer);

            // Activate the selected scheme (filters bindings to that group)
            user.ActivateControlScheme(scheme);

            // Optional: explicitly enforce mask too 
            perPlayer.bindingMask = InputBinding.MaskByGroup(scheme);

            // Enable gameplay now or later (e.g., when the match starts)
            if (enableGameplayOnJoin)
            {
                var gm = perPlayer.FindActionMap(gameplayMapName, true);
                gm.Enable();
            }

            // Spawn player and hand over context
            var go = Instantiate(playerPrefab);
            var handler = go.GetComponent<PlayerInputHandler>();
            var ctx = new PlayerInputContext
            {
                Index = _players.Count,
                SchemeName = scheme,
                User = user,
                Actions = perPlayer,
                Handler = handler

            };
            _players.Add(ctx);

            if (IsKBMScheme(scheme))
            {

                _claimedKBMSchemes.Add(scheme);
            }

            if (handler != null) handler.Initialize(ctx);

            OnPlayerJoined?.Invoke(ctx);

            Debug.Log($"[Lobby] Player {ctx.Index + 1} joined with scheme '{scheme}' and devices: {string.Join(", ", devices.Select(d => d.displayName))}");
        }

        // Call this when you want to start the match:
        public void StartMatch()
        {
            // Stop listening for new joins
            if (_joinAction != null) _joinAction.Disable();

            foreach (var p in _players)
            {
                var map = p.Actions.FindActionMap(gameplayMapName, true);
                map.Enable();
            }
            Debug.Log("[Lobby] Match started.");
        }

        public void StartLobby()
        {
            // Stop listening for new joins
            if (_joinAction != null) _joinAction.Enable();
            
            foreach (var p in _players)
            {
                var map = p.Actions.FindActionMap(lobbyMapName, true);
                map.Disable();
            }
            Debug.Log("[Lobby] Lobby started.");
        }

        private bool IsDevicePaired(InputDevice device)
        {
            foreach (var p in _players)
                if (p.User.pairedDevices.Contains(device)) return true;
            return false;
        }

        private static bool IsKBMScheme(string scheme)
            => scheme != null && scheme.StartsWith("Keyboard");


        private string ChooseScheme(string bindingGroup, InputDevice device)
        {
            // If binding specified a group, prefer it (and ensure it exists in the asset)
            if (!string.IsNullOrEmpty(bindingGroup) &&
                sharedAsset.controlSchemes.Any(cs => cs.name == bindingGroup))
            {
                return bindingGroup;
            }

            // Otherwise infer from device kind
            if (device is Gamepad)
                return gamepadSchemeName;

            if (device is Keyboard || device is Mouse)
            {
                // Pick first available keyboard scheme
                foreach (var s in keyboardSchemeNames)
                    if (!_claimedKBMSchemes.Contains(s)) return s;
            }

            return null;
        }

        private static string ResolveBindingGroup(InputAction action, InputControl triggeringControl)
        {
            // Find which binding path matches the control, then read its groups (semicolon-separated)
            var bindings = action.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                var b = bindings[i];
                if (b.isComposite || b.isPartOfComposite) continue;

                var path = b.path; // use raw path; good enough for join
                if (string.IsNullOrEmpty(path)) continue;

                if (InputControlPath.Matches(path, triggeringControl))
                {
                    // groups format: "KBM_P1;AnotherGroup"
                    var groups = b.groups;
                    if (string.IsNullOrEmpty(groups)) return null;

                    var semi = groups.IndexOf(';');
                    return semi >= 0 ? groups.Substring(0, semi) : groups;
                }
            }
            return null;
        }

        public List<PlayerInputHandler> GetPlayers()
        {
            return _players.Select(p => p.Handler).ToList();
        }
    }
}

