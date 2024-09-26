/* This file controls the client-side networking.
 * Including: 
 * creating matchmaking tickets,
 * creating lobbies,
 * joining lobbies,
 * change client UI,
 * and more
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using StatusOptions = Unity.Services.Matchmaker.Models.MultiplayAssignment.StatusOptions;
using UnityEngine;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;

public class MatchmakerClient : MonoBehaviour
{ 
    private string ticketId;
    private UIManagerScript UI;
    private ServerLogicScript serverLogic;
    private LogicScript logic;

    // Getter
    private string PlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    // SIGN IN --- This happens instantly upon opening the game
    private void OnEnable()
    {
        ServerStartUp.ClientInstance += SignIn;
    }

    private void Start()
    {
        logic = LogicScript.Instance;
        UI = UIManagerScript.Instance;
        serverLogic = ServerLogicScript.Instance;
    }

    private void OnDisable()
    {
        ServerStartUp.ClientInstance -= SignIn;
    }

    // Called OnEnable() to sign in client
    private async void SignIn()
    {
        await ClientSignIn();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task ClientSignIn(string serviceProfileName = null)
    {
        await UnityServices.InitializeAsync();
        Debug.Log($"Signed In Anon as {serviceProfileName}({PlayerID()})");
    }

    // START ONLINE PLAY (This is NOT the same as starting a network client, that happens after matching)
    // StartClient() is called by Online -> Public button
    public void StartClient()
    {
        CreateATicket();
    }

    // Tell Unity Matchmaker we are looking for a game
    //private bool ticketActive;
    private async void CreateATicket( string lobbyID = "PUBLIC" )
    {
        await UnityServices.InitializeAsync();
        var options = new CreateTicketOptions("Public");
        var players = new List<Unity.Services.Matchmaker.Models.Player>
        {
            new Unity.Services.Matchmaker.Models.Player(PlayerID(), new MatchmakingPlayerData { LobbyID = lobbyID })
        };

        try
        {
            var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
            ticketId = ticketResponse.Id;
            Debug.Log($"Ticket ID: {ticketId}");
            PollTicketStatus();
        }
        catch (MatchmakerServiceException e)
        {
            Debug.LogError(e);
            UI.SetErrorMessage("Failed to connect to server. Please try again.");
            UI.FailedToFindMatch();
        }
    }

    // Waiting for response from Unity Matchmaker
    private async void PollTicketStatus()
    {
        MultiplayAssignment multiplayAssignment = null;
        bool gotAssignement = false;
        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1f));
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(ticketId);
            if (ticketStatus == null) continue;
            if (ticketStatus.Type == typeof(MultiplayAssignment))
            {
                multiplayAssignment = ticketStatus.Value as MultiplayAssignment;
            }
            switch (multiplayAssignment.Status)
            {
                case StatusOptions.Found:
                    gotAssignement = true;
                    TicketAssigned(multiplayAssignment);
                    Debug.Log($"Got Assignment! {multiplayAssignment.Message}");
                    break;
                case StatusOptions.InProgress:
                    break;
                case StatusOptions.Failed:
                    gotAssignement = true;
                    Debug.Log($"Failed to get ticket status. Error: {multiplayAssignment.Message}");
                    UI.SetErrorMessage("Connection error. Please try again.");
                    await MatchmakerService.Instance.DeleteTicketAsync(ticketId);
                    break;
                case StatusOptions.Timeout:
                    gotAssignement = true;
                    Debug.Log($"Failed to get ticket status. Timed out");
                    if (UI.waitingGif.activeInHierarchy)
                    {
                        UI.SetErrorMessage("Connection timed out. Please try again.");
                        UI.FailedToFindMatch();
                    }
                    await MatchmakerService.Instance.DeleteTicketAsync(ticketId);
                    break;
            }
        } while (!gotAssignement);
    }

    // When Unity Matchmaker assigns a game, start client and show ready button
    private void TicketAssigned(MultiplayAssignment assignment)
    {
        Debug.Log($"Ticket assigned: {assignment.Ip}:{assignment.Port}");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        NetworkManager.Singleton.StartClient();
        UI.EnableReadyButton();
    }

    // this will be for host / join private lobby
    private Lobby hostLobby;
    bool isHost;
    private float heartbeatTimer;

    // Called by Online -> Host button
    // creates lobby with Unity Lobby service
    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 2;
            CreateLobbyOptions options = new CreateLobbyOptions { IsPrivate = true };

            // Create the Lobby
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            // Subscribe to lobby events
            LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            ILobbyEvents lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);

            // Allocate Relay server
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Pass Relay join code to lobby data
            Dictionary<string, DataObject> data = new Dictionary<string, DataObject> {
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
            };

            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions { Data = data });

            // Set up UnityTransport to use Relay
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            //if (NetworkManager.Singleton.IsClient) { NetworkManager.Singleton.Shutdown(); }
            NetworkManager.Singleton.StartHost(); // Start as Host
            hostLobby = lobby;
            isHost = true;

            Debug.Log("Created Lobby with Relay. Lobby Code: " + lobby.LobbyCode);
            UI.lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;
            GUIUtility.systemCopyBuffer = lobby.LobbyCode;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            UI.SetErrorMessage("Failed to create Lobby with Relay.");
        }
    }

    public class MatchmakingPlayerData
    {
        public string LobbyID;
    }

    public TMP_InputField lobbyCodeInputField;

    // only show the Join button when 6 characters are entered, helps misinput
    public void ChangeJoinButtonVisibiity()
    {
        joinButtion.SetActive(lobbyCodeInputField.text.Length == 6);
    }

    // Called by Online -> Join -> Join button (after code is entered)
    public async void JoinLobby()
    {
        try
        {
            string lobbyCode = lobbyCodeInputField.text;
            if (lobbyCode.Length != 6)
            {
                UI.SetErrorMessage("Invalid lobby code.");
                return;
            }

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);

            // Retrieve Relay join code from lobby data
            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            // Set up UnityTransport to use Relay
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            //if (NetworkManager.Singleton.IsHost) { NetworkManager.Singleton.Shutdown(); }
            NetworkManager.Singleton.StartClient(); // Start as Client
            hostLobby = lobby;
            isHost = false;

            Debug.Log("Joined Lobby with Relay. Lobby Code: " + lobbyCode);
            UI.lobbyCodeText.text = "Lobby Code: " + lobbyCode;
            UI.EnableReadyButton();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            UI.SetErrorMessage("Failed to join Lobby with Relay.");
        }
    }

    // For the host: when a competitor joins your lobby, tell Matchmaker to queue for a server
    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.PlayerJoined.Changed)
        {
            Debug.Log("Player Joined");
            Debug.Log("HOST ID: " + PlayerID());
            Debug.Log("JOINER ID: " + changes.PlayerJoined.Value[0].Player.Id);
            // CreateATicket(hostLobby.Id); for multiplay
            // for relay
            UI.EnableReadyButton();
        }
    }

    [SerializeField] private GameObject joinButtion;
    private void Update()
    {
        Heartbeat();
    }

    // Keep lobby open while waiting for joiner
    public async void Heartbeat()
    {
        if (hostLobby != null && isHost)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = 20;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                Debug.Log("heartbeating...");
            }
        }
    }

    // Called by the back button, shutdowns down the lobby and stops the heartbeat
    public async void ShutdownLobby()
    {
        try 
        {
            if (hostLobby != null)
            {
                await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }

        hostLobby = null;
        isHost = false;
    }

    // Called by the ready button
    public void AddPlayer()
    {
        try
        {
            serverLogic.AddPlayerServerRpc(logic.player.puckSpriteID);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            UI.SetErrorMessage("Server Error: -1 - Contact developer.");
        }
    }

    // Stop Client
    public void StopClient()
    {
        serverLogic.AlertDisconnectServerRpc();
        //NetworkManager.Singleton.Shutdown();
    }
}
