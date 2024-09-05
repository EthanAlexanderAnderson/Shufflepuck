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
        UI = GameObject.FindGameObjectWithTag("ui").GetComponent<UIManagerScript>();
        logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();
        serverLogic = GameObject.FindGameObjectWithTag("logic").GetComponent<ServerLogicScript>();
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
            Debug.Log(e);
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
            // lobby settings
            string lobbyName = "MyLobby";
            int maxPlayers = 2;
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = true;
            // create lobby
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            // setup lobby event detector
            LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            ILobbyEvents lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
            // change UI
            hostLobby = lobby;
            isHost = true;
            Debug.Log("Created Lobby. Code:  " + lobby.LobbyCode);
            UI.lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;
            GUIUtility.systemCopyBuffer = lobby.LobbyCode;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            UI.SetErrorMessage("Failed to create Lobby. Please try again.");
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
        if (lobbyCodeInputField.text.Length != 6)
        {
            UI.SetErrorMessage("Invalid lobby code.");
            return;
        }
        try
        {
            // idk why this block is needed but it is
            UpdatePlayerOptions options = new UpdatePlayerOptions();

            options.Data = new Dictionary<string, PlayerDataObject>()
            {
                {
                    "existing data key", new PlayerDataObject(
                        visibility: PlayerDataObject.VisibilityOptions.Private,
                        value: "updated data value")
                },
                {
                    "new data key", new PlayerDataObject(
                        visibility: PlayerDataObject.VisibilityOptions.Public,
                        value: "new data value")
                }
            };
            // idk block end
            hostLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCodeInputField.text);
            isHost = false;
            Debug.Log("Joined Lobby with code: " + lobbyCodeInputField.text);
            UI.lobbyCodeText.text = "Lobby Code: " + lobbyCodeInputField.text;
            Debug.Log(PlayerID());
            CreateATicket(hostLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            UI.SetErrorMessage("Lobby not found.");
            UI.FailedToFindMatch();
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
            CreateATicket(hostLobby.Id);
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
            Debug.Log(e);
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
            Debug.Log(e);
            UI.SetErrorMessage("Server Error: -1 - Contact developer.");
        }
    }

    // Stop Client
    public void StopClient()
    {
        serverLogic.AlertDisconnectServerRpc();
        NetworkManager.Singleton.Shutdown();
    }
}
