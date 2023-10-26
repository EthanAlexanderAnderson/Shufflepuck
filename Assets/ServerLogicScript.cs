using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ServerLogicScript : NetworkBehaviour
{
    
    public GameObject puck;
    //public Sprite puckSprite;

    //private UIManagerScript UI;
    //private LogicScript logic;
    private ClientLogicScript clientLogic;

    private List<ulong> clients = new(); // List of Client IDs
    private List<Competitor> competitorList = new(); // use this for puck ownership, same indexes as clients
    private List<ClientRpcParams> clientRpcParamsList = new(); // use this for targeted rpc, same indexes as clients

    private List<int> competitorPuckCountList = new();
    private List<int> competitorScoreList = new();

    private GameObject activePuck;

    private int activeCompetitorIndex;
    //public Competitor nonActiveCompetitor; // Let's not use this unless necessary

    private void OnEnable()
    {
        //logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();
        clientLogic = GameObject.FindGameObjectWithTag("logic").GetComponent<ClientLogicScript>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(int puckID, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer) return;
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
            $"Puck ID: {puckID} \n" );

        clientLogic.UpdateReadyWaitingCompetitorsClientRpc();

        if (clients.Count == 2)
        {
            SelectRandomStartingPlayer();
        }
    }

    public void SelectRandomStartingPlayer()
    {
        if (!IsServer) return;
        activeCompetitorIndex = Random.Range(0, 2);

        Debug.Log(
            $"Selected starting client. \n" +
            $"Client Index #{activeCompetitorIndex} \n" +
            $"Client ID : {clients[activeCompetitorIndex]}\n");

        clientLogic.RestartGameOnlineClientRpc(competitorList[0].puckSpriteID, competitorList[1].puckSpriteID);
        clientLogic.StartTurnClientRpc(clientRpcParamsList[activeCompetitorIndex]);
        //nonActiveCompetitor = randomInt == 1 ? competitorList[0] : competitorList[1];
    }

    [ServerRpc(RequireOwnership = false)]
    public void CreatePuckServerRpc()
    {
        if (!IsServer) return;
        CreatePuck();
    }

    private void CreatePuck()
    {

        if (competitorPuckCountList.All(n => n <= 0))
        {
            clientLogic.GameOverConfirmationClientRpc();
            return;
        }

        Competitor competitor = competitorList[activeCompetitorIndex];
        int puckSpriteID = competitor.puckSpriteID;

        GameObject puckObject = Instantiate(puck, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
        puckObject.GetComponent<NetworkObject>().Spawn();
        competitor.activePuckObject = puckObject;

        PuckScript puckScript = puckObject.GetComponent<PuckScript>();

        //competitorPuckCountList[activeCompetitorIndex]--;

        // tell the active competitor this new puck is theirs, tell non-active competitors it's not theirs
        for (int i = 0; i < competitorList.Count; i++)
        {
            puckScript.InitPuckClientRpc( i == activeCompetitorIndex, puckSpriteID, clientRpcParamsList[i] );
        }
        //puckScript.InitPuckClientRpc(true, puckSpriteID);
        competitor.activePuckScript = puckScript;

        Debug.Log(
            $"Puck has been spawned. \n" +
            $"Owned by Client Index #{activeCompetitorIndex} \n" +
            $"Client ID : {clients[activeCompetitorIndex]} \n" +
            $"Puck Skin ID: {competitor.puckSpriteID} \n" );
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc(float angleParameter, float powerParameter, float spinParameter)
    {
        if (!IsServer) return;
        
        competitorList[activeCompetitorIndex].activePuckScript.Shoot(angleParameter, powerParameter, spinParameter);

        // Update puck count
        competitorPuckCountList[activeCompetitorIndex]--;
        // tell active their puck count decreased
        clientLogic.UpdatePuckCountClientRpc(true, competitorPuckCountList[activeCompetitorIndex], clientRpcParamsList[activeCompetitorIndex]);
        // tell non-active their opponent's count decreased
        clientLogic.UpdatePuckCountClientRpc(false, competitorPuckCountList[activeCompetitorIndex], clientRpcParamsList[SwapCompetitors()]);
        // Also no idea if this works ^^^


        // NO IDEA IF THIS SCALES PAST 2 PLAYERS (update it doesn't work regardless)
        // for each player
        /*
        for (int i = 0; i < clientRpcParamsList.Count; i++)
        {
            // send each puck count
            for (int j = 0; j < competitorList.Count; j++)
            {
                // send to the active player with true and non-active with false
                clientLogic.UpdatePuckCountClientRpc((j == activeCompetitorIndex && i == activeCompetitorIndex), competitorPuckCountList[j], clientRpcParamsList[i]);
            }
            SwapCompetitors();
        }
        */
        clientLogic.StartTurnClientRpc(clientRpcParamsList[activeCompetitorIndex]);
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













































































    /*

    // DEBUGGING FUNCTIONS
    [ServerRpc(RequireOwnership = false)]
    public void DebugShootServerRpc(float angleParameter, float powerParameter, float spinParameter)
    {
        if (!IsServer) return;
        DebugShoot(angleParameter, powerParameter, spinParameter);
    }

    private void DebugShoot(float angleParameter, float powerParameter, float spinParameter)
    {
        var objects = GameObject.FindGameObjectsWithTag("puck");
        foreach (var obj in objects)
        {
            obj.GetComponent<PuckScript>().Shoot(angleParameter, powerParameter, spinParameter);
        }

        var puckObject = Instantiate(puck, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
        var puckScript = puckObject.GetComponent<PuckScript>();

        puckObject.GetComponent<NetworkObject>().Spawn();
        Debug.Log("Server: puck has been spawned");
        clientLogic.SpawnedPuckConfirmationClientRpc();

        puckScript.InitPuck(true, puckSprite);
        puckScript.Shoot(angleParameter, powerParameter, spinParameter);
    }*/
}

