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

public class MatchmakerClient : MonoBehaviour
{ 
    private string ticketId;
    //[SerializeField] private GameObject gameHUD;
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

    public void StartClient()
    {
        CreateATicket();
    }

    private async void CreateATicket()
    {
        await UnityServices.InitializeAsync();
        var options = new CreateTicketOptions("Public");
        var players = new List<Player>
        {
            new Player(PlayerID())
        };

        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        ticketId = ticketResponse.Id;
        Debug.Log($"Ticket ID: {ticketId}");
        PollTicketStatus();
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
