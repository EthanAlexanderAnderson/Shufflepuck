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
                    break;
                case StatusOptions.Timeout:
                    gotAssignement = true;
                    Debug.Log($"Failed to get ticket status. Timed out");
                    UI.SetErrorMessage("Connection timed out. Please try again.");
                    UI.FailedToFindMatch();
                    break;
            }
        } while (!gotAssignement);
    }

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

    public void ChangeJoinButtonVisibiity()
    {
        joinButtion.SetActive(lobbyCodeInputField.text.Length == 6);
    }

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

    public void AddPlayer()
    {
        serverLogic.AddPlayerServerRpc(logic.player.puckSpriteID);
    }

    // Stop Client
    public void StopClient()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
