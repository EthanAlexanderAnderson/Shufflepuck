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
using Unity.Collections;

public class MatchmakerClient : MonoBehaviour
{
    private string ticketId;

    // Getter
    private string PlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    /*/ SIGN IN --- This happens instantly upon opening the game
    private void OnEnable()
    {
        ServerStartUp.ClientInstance += SignIn;
    }
    */

    /*
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
    */

    // START ONLINE PLAY (This is NOT the same as starting a network client, that happens after matching)
    // StartClient() is called by Online -> Public button
    public void StartClient()
    {
        CreateATicket();
    }

    // Tell Unity Matchmaker we are looking for a game
    //private bool ticketActive;
    private async void CreateATicket(string lobbyID = "PUBLIC")
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
            UIManagerScript.Instance.SetErrorMessage("Failed to connect to server. Please try again.");
            UIManagerScript.Instance.FailedToFindMatch();
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
                    UIManagerScript.Instance.SetErrorMessage("Connection error. Please try again.");
                    await MatchmakerService.Instance.DeleteTicketAsync(ticketId);
                    break;
                case StatusOptions.Timeout:
                    gotAssignement = true;
                    Debug.Log($"Failed to get ticket status. Timed out");
                    if (UIManagerScript.Instance.waitingGif.activeInHierarchy)
                    {
                        UIManagerScript.Instance.SetErrorMessage("Connection timed out. Please try again.");
                        UIManagerScript.Instance.FailedToFindMatch();
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
        UIManagerScript.Instance.EnableReadyButton();
    }

    // this will be for host / join private lobby
    private Lobby hostLobby;
    bool isHost;
    private float heartbeatTimer;

    // Called by Online -> Host button
    // creates lobby with Unity Lobby service
    public async void CreateLobby(bool isPrivate)
    {
        try
        {
            string lobbyName = isPrivate ? "PrivateLobby" : "PublicLobby";
            int maxPlayers = 2;
            CreateLobbyOptions options = new CreateLobbyOptions { IsPrivate = isPrivate };

            // Create the Lobby
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            // Subscribe to lobby events
            LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            ILobbyEvents lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);

            // Allocate Relay server
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Pass Relay join code to lobby data
            Dictionary<string, DataObject> data = new Dictionary<string, DataObject> {
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) },
                { "Elo", new DataObject(DataObject.VisibilityOptions.Public, PlayerPrefs.GetInt("Elo", 100).ToString(), DataObject.IndexOptions.N1) }
            };

            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions { Data = data });

            // Set up UnityTransport to use Relay
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            //if (NetworkManager.Singleton.IsClient) { NetworkManager.Singleton.Shutdown(); }
            NetworkManager.Singleton.StartHost(); // Start as Host
            hostLobby = lobby;
            isHost = true;

            if (isPrivate)
            {
                Debug.Log("Created Private Lobby. Code: " + lobby.LobbyCode);
                UIManagerScript.Instance.lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;
                GUIUtility.systemCopyBuffer = lobby.LobbyCode;
            }
            else
            {
                Debug.Log("Created Public Lobby.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            UIManagerScript.Instance.SetErrorMessage("Failed to create Lobby with Relay.");
        }
    }

    public class MatchmakingPlayerData
    {
        public string LobbyID;
    }

    public TMP_InputField lobbyCodeInputField;
    [SerializeField] private GameObject joinButtion;
    // only show the Join button when 6 characters are entered, helps misinput
    public void ChangeJoinButtonVisibiity()
    {
        joinButtion.SetActive(lobbyCodeInputField.text.Length == 6);
    }

    // Called by Online -> Join -> Join button (after code is entered)
    public void JoinPrivateLobbyWithCode()
    {
        JoinLobby(true, lobbyCode: lobbyCodeInputField.text);
    }

    private async void JoinLobby(bool isPrivate, string lobbyId = "", string lobbyCode = "")
    {
        try
        {
            if (isPrivate && lobbyCode.Length != 6)
            {
                Debug.LogError("Invalid lobby code.");
                UIManagerScript.Instance.SetErrorMessage("Invalid lobby code.");
                return;
            }
            else if (!isPrivate && lobbyId == "")
            {
                Debug.LogError("Invalid lobby ID.");
                UIManagerScript.Instance.SetErrorMessage("Invalid lobby ID.");
                return;
            }

            Lobby lobby = null;
            if (isPrivate && lobbyCode.Length == 6)
            {
                lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            }
            else if (!isPrivate && !string.IsNullOrEmpty(lobbyId))
            {
                lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            }
            else
            {
                Debug.LogError("Invalid lobby code or ID.");
                UIManagerScript.Instance.SetErrorMessage("Invalid lobby code or ID.");
                return;
            }

            // Retrieve Relay join code from lobby data
            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            Debug.Log("Relay Join Code: " + relayJoinCode);

            // Set up UnityTransport to use Relay
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost) { NetworkManager.Singleton.Shutdown(); }

            // wait until the client is shutdown
            int maxIterations = 0;
            while ((NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsHost) && maxIterations < 50)
            {
                await Task.Delay(100);
                maxIterations++;
            }

            if (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsHost)
            {
                Debug.LogError("Failed to shutdown previous NetworkManager before joining new lobby.");
                UIManagerScript.Instance.SetErrorMessage("Failed to join Lobby with Relay.");
                return;
            }

            NetworkManager.Singleton.StartClient(); // Start as Client

            hostLobby = lobby;
            isHost = false;

            if (isPrivate)
            {
                Debug.Log("Joined Private Lobby. Code: " + lobbyCode);
                UIManagerScript.Instance.lobbyCodeText.text = "Lobby Code: " + lobbyCode;
                UIManagerScript.Instance.EnableReadyButton();
            }
            else
            {
                Debug.Log("Joined Public Lobby. ID: " + lobbyId);
                UIManagerScript.Instance.EnableReadyButton();
            }
        }
        catch (LobbyServiceException)
        {
            Debug.LogError("LobbyServiceException: Failed to join Lobby (Invalid Join Code or Lobby is full)");
            UIManagerScript.Instance.SetErrorMessage("Failed to join Lobby (Invalid Join Code or Lobby is full).");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            UIManagerScript.Instance.SetErrorMessage("Failed to join Lobby with Relay.");
        }
    }

    public async void PublicMatchmaking()
    {
        int playerElo = PlayerPrefs.GetInt("Elo", 100);
        int[] searchRanges = { 100, 300, 9999 }; // progressively widen Elo range
        int rateLimitDelay = 750;

        for (int i = 0; i < searchRanges.Length; i++)
        {
            int minElo = playerElo - searchRanges[i];
            int maxElo = playerElo + searchRanges[i];

            // Query for public, open lobbies
            var query = new QueryLobbiesOptions
            {
                Count = 1,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"),
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.IsLocked,
                        op: QueryFilter.OpOptions.EQ,
                        value: "false"),

                    new QueryFilter(
                        field: QueryFilter.FieldOptions.N1,
                        op: QueryFilter.OpOptions.GE,
                        value: minElo.ToString()),
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.N1,
                        op: QueryFilter.OpOptions.LE,
                        value: maxElo.ToString())
                },
                Order = new List<QueryOrder>
                {
                new QueryOrder(
                    asc: true,
                    field: QueryOrder.FieldOptions.Created)
                }
            };

            try
            {
                // make query to get lobbies
                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(query);

                // Handle query response
                if (response.Results.Count > 0) // public lobby was found -> join it
                {
                    Lobby foundLobby = response.Results[0];
                    Debug.Log($"Found lobby within {searchRanges[i]} Elo range: {foundLobby.Data["Elo"].Value}");
                    JoinLobby(false, lobbyId: foundLobby.Id);
                    return;
                }

                // default query delay
                await Task.Delay(rateLimitDelay);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    Debug.LogWarning("Rate limited, waiting longer...");
                    // the search at the current elo failed, so we need to retry this iteration
                    i--;
                    // increase the delay between requests each time we get rate limited.
                    rateLimitDelay += 500;

                    // if the delay is longer than 10 seconds, clearly there is some issue, so cancel matchmaking
                    if (rateLimitDelay > 10000)
                    {
                        Debug.LogError("QueryLobbiesAsync Rate Limit Error");
                        UIManagerScript.Instance.SetErrorMessage("Matchmaking failed: rate limit error");
                        return;
                    }

                    await Task.Delay(rateLimitDelay);
                }
            }
        }

        CreateLobby(false); // public lobby wasn't found -> open one
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
            UIManagerScript.Instance.EnableReadyButton();
        }
    }

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
        if (hostLobby != null && isHost)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
                Debug.Log("Lobby successfully deleted.");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to delete lobby: {e.Message}");
            }
        }

        hostLobby = null;
        isHost = false;
    }


    // Called by the ready button
    public void AddPlayer()
    {
        try
        {
            ServerLogicScript.Instance.AddPlayerServerRpc(LogicScript.Instance.player.puckSpriteID, new FixedString32Bytes(PlayerAuthentication.Instance.GetUsername()), PlayerPrefs.GetInt("Elo", 100));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            UIManagerScript.Instance.SetErrorMessage("Server Error: -1 - Contact developer.");
        }
    }

    // Stop Client
    public async void StopClient()
    {
        try
        {
            ClientLogicScript.Instance.StopGame();
            ServerLogicScript.Instance.AlertDisconnectServerRpc();
            ServerLogicScript.Instance.ResetSeverVariables();
        }

        catch (Exception e)
        {
            Debug.LogError(e);
        }
        try
        {
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.Shutdown();

                await Task.Delay(1000); // Add delay to ensure full shutdown

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(default);

                Debug.Log("Client successfully shut down.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            UIManagerScript.Instance.SetErrorMessage("Server Error: -3 - Contact developer.");
        }
    }
}