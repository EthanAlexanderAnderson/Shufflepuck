/* This script runs the logic which is executed on the server during online play.
 * Functions marked [ServerRpc] are functions which are called by the client and executed on the server.
 *
 * Note: This script is filled with try catch statements so that we can send an error code to the client. 
 * This code will show me exactly which function caused the error without digging in server logs.
 */

using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ServerLogicScript : NetworkBehaviour
{
    // self
    public static ServerLogicScript Instance;

    // dependancies
    private ClientLogicScript clientLogic;
    public GameObject puck; //prefab

    private List<ulong> clients = new(); // List of Client IDs
    private List<Competitor> competitorList = new(); // use this for puck ownership, same indexes as clients
    private List<ClientRpcParams> clientRpcParamsList = new(); // use this for targeted rpc, same indexes as clients

    private List<int> competitorPuckCountList = new();
    private List<int> competitorScoreList = new();

    private int activeCompetitorIndex;
    private int startingPlayerIndex;

    private float shotTimer;
    bool gameIsRunning;

    private int randomlySelectedStartingPlayerIndex = -1;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void Start()
    {
        clientLogic = ClientLogicScript.Instance;
        shotTimer = 30;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (gameIsRunning) { shotTimer -= Time.deltaTime; } // decrement timer
        if (shotTimer < 0)
        {
            Debug.Log("Shot Timer Exceeded");
            shotTimer = 60;
            clientLogic.AlertDisconnectClientRpc();
        }

        // If both players have 0 pucks (aka game is over)
        if (competitorPuckCountList.Count > 1 && competitorPuckCountList.All(n => n <= 0))
        {
            var allPucks = GameObject.FindGameObjectsWithTag("puck");
            foreach (var puck in allPucks)
            {
                // If any pucks are not stopped, return
                if (!puck.GetComponent<PuckScript>().IsStopped())
                {
                    return;
                }
            }
            clientLogic.GameResultClientRpc();
            // host only, show rematch button
            clientLogic.ShowRematchButton();
        }
    }

    // When 1 competitor is ready, setup their variables and update the ready text
    // When both are ready, start the game
    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(int puckID, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer) return;
        try
        {
            var clientId = serverRpcParams.Receive.SenderClientId;

            clients.Add(clientId);

            Competitor newCompetitor = new(puckID);
            newCompetitor.clientID = clientId;

            competitorList.Add(newCompetitor);

            clientRpcParamsList.Add(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            });

            competitorPuckCountList.Add(5);
            competitorScoreList.Add(0);

            Debug.Log(
                $"Client added to client list. \n" +
                $"Client Index: {clients.Count - 1} \n" +
                $"Client ID: {clientId} \n" +
                $"Puck ID: {puckID} \n");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            clientLogic.SetErrorMessageClientRpc(0);
        }
        try
        {
            clientLogic.UpdateReadyWaitingCompetitorsClientRpc();

            if (clients.Count == 2)
            {
                SelectRandomStartingPlayer();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            clientLogic.SetErrorMessageClientRpc(1);
        }
    }

    // Pick a random player to begin
    public void SelectRandomStartingPlayer()
    {
        if (!IsServer) return;

        try
        {
            // if first match, make random
            if (randomlySelectedStartingPlayerIndex <= -1)
            {
                randomlySelectedStartingPlayerIndex = Random.Range(0, 2);
            }
            // if rematch, change 1 to 0, or 0 to 1
            else
            {
                randomlySelectedStartingPlayerIndex ^= 1;
            }

            activeCompetitorIndex = randomlySelectedStartingPlayerIndex;
            startingPlayerIndex = activeCompetitorIndex;

            Debug.Log(
                $"Selected starting client. \n" +
                $"Client Index #{activeCompetitorIndex} \n" +
                $"Client ID : {clients[activeCompetitorIndex]}\n");

            clientLogic.RestartGameOnlineClientRpc(competitorList[0].puckSpriteID, competitorList[1].puckSpriteID);
            clientLogic.StartTurnClientRpc(true, clientRpcParamsList[activeCompetitorIndex]);
            shotTimer = 21;
            gameIsRunning = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            clientLogic.SetErrorMessageClientRpc(2);
        }
    }

    // client tells the server to create a puck
    [ServerRpc(RequireOwnership = false)]
    public void CreatePuckServerRpc()
    {
        if (!IsServer) return;
        CreatePuck();
    }

    private void CreatePuck()
    {
        try
        {
            // if both players have 0 pucks, end game
            if (competitorPuckCountList.All(n => n <= 0))
            {
                clientLogic.GameOverConfirmationClientRpc();
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            clientLogic.SetErrorMessageClientRpc(3);
        }
        try
        {
            Competitor competitor = competitorList[activeCompetitorIndex];
            int puckSpriteID = competitor.puckSpriteID;

            float xpos = (startingPlayerIndex == activeCompetitorIndex ? -3.6f : 3.6f);
            GameObject puckObject = Instantiate(puck, new Vector3(xpos, -10.0f, 0.0f), Quaternion.identity);
            puckObject.GetComponent<NetworkObject>().Spawn();
            competitor.activePuckObject = puckObject;

            PuckScript puckScript = puckObject.GetComponent<PuckScript>();

            // tell the active competitor this new puck is theirs, tell non-active competitors it's not theirs
            for (int i = 0; i < competitorList.Count; i++)
            {
                puckScript.InitPuckClientRpc(i == activeCompetitorIndex, puckSpriteID, clientRpcParamsList[i]);
            }
            competitor.activePuckScript = puckScript;

            Debug.Log(
                $"Puck has been spawned. \n" +
                $"Owned by Client Index #{activeCompetitorIndex} \n" +
                $"Client ID : {clients[activeCompetitorIndex]} \n" +
                $"Puck Skin ID: {competitor.puckSpriteID} \n");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            clientLogic.SetErrorMessageClientRpc(4);
        }
    }

    // Client tells the sever to shoot, given the shot parameters
    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc(float angleParameter, float powerParameter, float spinParameter)
    {
        if (!IsServer) return;

        try
        {
            competitorList[activeCompetitorIndex].activePuckScript.Shoot(angleParameter, powerParameter, spinParameter);

            // Update puck count
            competitorPuckCountList[activeCompetitorIndex]--;
            // tell active their puck count decreased
            clientLogic.UpdatePuckCountClientRpc(true, competitorPuckCountList[activeCompetitorIndex], clientRpcParamsList[activeCompetitorIndex]);
            // tell non-active their opponent's count decreased, and swap competitors
            clientLogic.UpdatePuckCountClientRpc(false, competitorPuckCountList[activeCompetitorIndex], clientRpcParamsList[SwapCompetitors()]);

            CleanupDeadPucks();

            clientLogic.StartTurnClientRpc(activeCompetitorIndex == startingPlayerIndex, clientRpcParamsList[activeCompetitorIndex]);
            shotTimer = 21;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            clientLogic.SetErrorMessageClientRpc(5);
        }
    }

    private int SwapCompetitors()
    {
        activeCompetitorIndex += 1;
        if (activeCompetitorIndex >= competitorList.Count)
        {
            activeCompetitorIndex = 0;
        }
        return activeCompetitorIndex;
    }

    // Server cleans up out of bounds pucks
    private void CleanupDeadPucks()
    {
        try
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("puck"))
            {
                var pucki = obj.GetComponent<PuckScript>();
                if (!pucki.IsSafe() && pucki.IsStopped())
                {
                    Destroy(obj);
                }
            }

        }
        catch (System.Exception e)
        {
            Debug.Log("CleanupDeadPucks Failed.");
            Debug.LogError(e);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AlertDisconnectServerRpc()
    {
        if (!IsServer) return;

        clientLogic.AlertDisconnectClientRpc();
    }

    public void ResetSeverVariables()
    {
        clients.Clear();
        competitorList.Clear();
        clientRpcParamsList.Clear();
        competitorPuckCountList.Clear();
        competitorScoreList.Clear();
        activeCompetitorIndex = 0;
        startingPlayerIndex = 0;
        gameIsRunning = false;
    }

    public void Rematch()
    {
        // destroy all network objects with the tag puck
        foreach (var obj in GameObject.FindGameObjectsWithTag("puck"))
        {
            Destroy(obj);
        }
        // reset puck count and score
        for (int i = 0; i < competitorList.Count; i++)
        {
            competitorPuckCountList[i] = 5;
            competitorScoreList[i] = 0;
        }
        SelectRandomStartingPlayer();
    }
}