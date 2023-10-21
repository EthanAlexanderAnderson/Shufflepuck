using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerLogicScript : NetworkBehaviour
{

    public GameObject puck;
    public Sprite puckSprite;

    private UIManagerScript UI;
    private LogicScript logic;

    private List<ulong> clients = new();
    private List<ClientRpcParams> clientRpcParamsList = new();

    private void OnEnable()
    {
        UI = GameObject.FindGameObjectWithTag("ui").GetComponent<UIManagerScript>();
        logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {

        if (!IsServer) return;
        var clientId = serverRpcParams.Receive.SenderClientId;

        clients.Add(clientId);
        clientRpcParamsList.Add(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });

        Debug.Log($"Client added to client list with ID: {clientId}");

        if (clients.Count == 2)
        {
            SelectRandomStartingPlayer();
        }
    }

    public void SelectRandomStartingPlayer()
    {
        if (!IsServer) return;
        int randomInt = Random.Range(0, 2);
        Debug.Log($"Selected starting client with ID: {clients[randomInt]}");
        RestartGameOnlineClientRpc();
        StartTurnClientRpc(clientRpcParamsList[randomInt]);
    }


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
        SpawnedPuckConfirmationClientRpc();

        puckScript.InitPuck(true, puckSprite);
        puckScript.Shoot(angleParameter, powerParameter, spinParameter);
    }

    [ClientRpc]
    public void SpawnedPuckConfirmationClientRpc()
    {
        if (!IsClient) return;
        Debug.Log("Client: Confirmed puck has been spawned on Server");
    }

    [ClientRpc]
    public void RestartGameOnlineClientRpc()
    {
        if (!IsClient) return;
        Debug.Log("Restarting game");
        logic.RestartGameOnline();
    }

    [ClientRpc]
    public void StartTurnClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;
        Debug.Log("Starting turn");
        logic.StartTurn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void CreatePuckServerRpc()
    {
        var puckObject = Instantiate(puck, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
        puckObject.GetComponent<NetworkObject>().Spawn();
    }
}
