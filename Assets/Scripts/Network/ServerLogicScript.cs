/* This script runs the logic which is executed on the server during online play.
 * Functions marked [ServerRpc] are functions which are called by the client and executed on the server.
 *
 * Note: This script is filled with try catch statements so that we can send an error code to the client. 
 * This code will show me exactly which function caused the error without digging in server logs.
 */

using System.Collections;
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

    private int activeCompetitorIndex;
    private int startingPlayerIndex;

    private float shotTimer;
    bool gameIsRunning;

    private int randomlySelectedStartingPlayerIndex = -1;

    private bool player1powerupsenabled = false;
    private bool player2powerupsenabled = false;

    private bool sentGameResult = false;

    // variables for triplePowerup
    private int triplePowerup = 0;
    private int triplePowerupUserCompetitorIndex = -1;
    private float lastShotAngle;
    private float lastShotPower;
    private float lastShotSpin;

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
        shotTimer = 42;
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

        // do triple powerup
        if (triplePowerup > 0)
        {
            var allPucks = GameObject.FindGameObjectsWithTag("puck");
            foreach (var puck in allPucks)
            {
                // If any pucks are not safe, return
                if (!puck.GetComponent<PuckScript>().IsSafe())
                {
                    return;
                }
            }
            CreatePuck(triplePowerupUserCompetitorIndex == activeCompetitorIndex ? 0 : 1);
            Shoot(lastShotAngle + Random.Range(-10.0f, 10.0f), lastShotPower + Random.Range(-10.0f, 10.0f), 50, triplePowerupUserCompetitorIndex == activeCompetitorIndex ? 0 : 1);
            triplePowerup--;
        }

        // If both players have 0 pucks (aka game is over)
        if (competitorList.Count > 1 && competitorList.All(n => n.puckCount <= 0) && !sentGameResult)
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
            gameIsRunning = false;
            StartCoroutine(TryToEndGame());
            // this is here so this block doesn't get called repeatedly
            sentGameResult = true;
        }
    }

    private IEnumerator TryToEndGame()
    {
        Debug.Log("Ending game...");
        yield return new WaitForSeconds(1f);
        clientLogic.GameResultClientRpc();
        clientLogic.ShowRematchButton();
    }

    public Competitor GetActiveCompetitor()
    {
        return competitorList[activeCompetitorIndex];
    }

    // When 1 competitor is ready, setup their variables and update the ready text
    // When both are ready, start the game
    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(int puckID, bool powerupsEnabled, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer) return;
        try
        {
            var clientId = serverRpcParams.Receive.SenderClientId;

            // make sure we don't add the same client twice
            if (clients.Contains(clientId))
            {
                Debug.Log("Client already added.");
                return;
            }

            clients.Add(clientId);

            Competitor newCompetitor = new(puckID);
            newCompetitor.clientID = clientId;
            newCompetitor.puckCount = 5;
            newCompetitor.score = 0;
            sentGameResult = false;

            competitorList.Add(newCompetitor);

            clientRpcParamsList.Add(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            });

            if (powerupsEnabled && player1powerupsenabled) { player2powerupsenabled = true; }
            if (powerupsEnabled) { player1powerupsenabled = true; }

            // acknowledge the client has been added
            clientLogic.AddPlayerACKClientRPC(clientRpcParamsList[clients.Count - 1]);

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
            competitorList[startingPlayerIndex].goingFirst = true;

            Debug.Log(
                $"Selected starting client. \n" +
                $"Client Index #{activeCompetitorIndex} \n" +
                $"Client ID : {clients[activeCompetitorIndex]}\n");

            // tell both players to restart the game and who is going first
            clientLogic.RestartGameOnlineClientRpc(competitorList[0].puckSpriteID, competitorList[1].puckSpriteID, player1powerupsenabled && player2powerupsenabled);
            clientLogic.StartTurnClientRpc(true, true, clientRpcParamsList[activeCompetitorIndex]);
            clientLogic.StartTurnClientRpc(false, true, clientRpcParamsList[activeCompetitorIndex ^ 1]);
            // reset the shot timer and set the game as running for server logic
            shotTimer = (player1powerupsenabled && player2powerupsenabled) ? 42 : 30;
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

    // isNonActiveCompetitorBit is just for triple right now
    private void CreatePuck(int isNonActiveCompetitorBit = 0)
    {
        try
        {
            // if both players have 0 pucks, end game
            if (competitorList.All(n => n.puckCount <= 0) && triplePowerup <= 0)
            {
                gameIsRunning = false;
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
            // isNonActiveCompetitorBit is just for triple powerup at the moment
            Competitor competitor = competitorList[activeCompetitorIndex ^ isNonActiveCompetitorBit];
            int puckSpriteID = competitor.puckSpriteID;

            // determine the pucks starting position (left or right side) then spawn it
            float xpos = (startingPlayerIndex == (activeCompetitorIndex ^ isNonActiveCompetitorBit) ? -3.6f : 3.6f);
            GameObject puckObject = Instantiate(puck, new Vector3(xpos, -10.0f, 0.0f), Quaternion.identity);
            puckObject.GetComponent<NetworkObject>().Spawn();
            competitor.activePuckObject = puckObject;

            PuckScript puckScript = puckObject.GetComponent<PuckScript>();

            // tell the active competitor this new puck is theirs, tell non-active competitors it's not theirs
            for (int i = 0; i < competitorList.Count; i++)
            {
                puckScript.InitPuckClientRpc(i == (activeCompetitorIndex ^ isNonActiveCompetitorBit), puckSpriteID, clientRpcParamsList[i]);
            }
            competitor.activePuckScript = puckScript;

            Debug.Log(
                $"Puck has been spawned. \n" +
                $"Owned by Client Index #{activeCompetitorIndex ^ isNonActiveCompetitorBit} \n" +
                $"Client ID : {clients[activeCompetitorIndex ^ isNonActiveCompetitorBit]} \n" +
                $"Puck Skin ID: {competitor.puckSpriteID} \n");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            clientLogic.SetErrorMessageClientRpc(4);
        }
        var (hosterScore, joinerScore) = PuckManager.Instance.UpdateScores();
        //Debug.Log("[SERVER] Scores: " + hosterScore + " : " + joinerScore);
    }

    // Client tells the sever to shoot, given the shot parameters
    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc(float angleParameter, float powerParameter, float spinParameter)
    {
        if (!IsServer) return;
        Shoot(angleParameter, powerParameter, spinParameter);
    }

    // Shoot the puck. isNonActiveCompetitorBit is just for triple right now
    public void Shoot(float angleParameter, float powerParameter, float spinParameter, int isNonActiveCompetitorBit = 0)
    {
        try
        {
            competitorList[activeCompetitorIndex ^ isNonActiveCompetitorBit].activePuckScript.Shoot(angleParameter, powerParameter, spinParameter);

            if (isNonActiveCompetitorBit == 0) // if this is shot for turn (shooter is the active competitor), not shot from triple powerup
            {
                // track last shot parameters (just for triple powerup for now)
                lastShotAngle = angleParameter;
                lastShotPower = powerParameter;
                lastShotSpin = spinParameter;

                // Update puck count
                competitorList[activeCompetitorIndex].puckCount--;
                // tell active their puck count decreased
                clientLogic.UpdatePuckCountClientRpc(true, competitorList[activeCompetitorIndex].puckCount, true, clientRpcParamsList[activeCompetitorIndex]);
                // tell non-active their opponent's count decreased, and swap competitors
                clientLogic.UpdatePuckCountClientRpc(false, competitorList[activeCompetitorIndex].puckCount, true, clientRpcParamsList[SwapCompetitors()]);

                CleanupDeadPucks();

                // if a player would start their turn with 0 pucks, instead start the turn of the other player (who has pucks remaining)
                if (competitorList[activeCompetitorIndex].puckCount <= 0 && competitorList[activeCompetitorIndex ^ 1].puckCount > 0)
                {
                    SwapCompetitors();
                }

                // tell both players the turn is swapping
                clientLogic.StartTurnClientRpc(true, activeCompetitorIndex == startingPlayerIndex, clientRpcParamsList[activeCompetitorIndex]);
                clientLogic.StartTurnClientRpc(false, activeCompetitorIndex == startingPlayerIndex, clientRpcParamsList[activeCompetitorIndex ^ 1]);
                // reset shot clock
                shotTimer = (player1powerupsenabled && player2powerupsenabled) ? 42 : 30;
            }
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

    [ServerRpc(RequireOwnership = false)]
    public void CleanupDeadPucksServerRpc()
    {
        CleanupDeadPucks();
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
        activeCompetitorIndex = 0;
        startingPlayerIndex = 0;
        gameIsRunning = false;
        player1powerupsenabled = false;
        player2powerupsenabled = false;
    }

    public void Rematch()
    {
        sentGameResult = false;
        // destroy all network objects with the tag puck
        foreach (var obj in GameObject.FindGameObjectsWithTag("puck"))
        {
            Destroy(obj);
        }
        // reset puck count and score
        for (int i = 0; i < competitorList.Count; i++)
        {
            competitorList[i].puckCount = 5;
            competitorList[i].score = 0;
        }
        SelectRandomStartingPlayer();
    }

    // Block Powerup
    [ServerRpc(RequireOwnership = false)]
    public void BlockServerRpc()
    {
        if (!IsServer) return;
        try
        {
            Competitor competitor = competitorList[activeCompetitorIndex];
            int puckSpriteID = competitor.puckSpriteID;

            float xpos = (startingPlayerIndex == activeCompetitorIndex ? (Random.Range(2f, 4f)) : (Random.Range(-2f, -4f)));
            GameObject puckObject = Instantiate(puck, new Vector3(xpos, Random.Range(2f, 4f), 0.0f), Quaternion.identity);
            puckObject.GetComponent<NetworkObject>().Spawn();

            PuckScript puckScript = puckObject.GetComponent<PuckScript>();

            // tell the active competitor this new puck is theirs, tell non-active competitors it's not theirs
            for (int i = 0; i < competitorList.Count; i++)
            {
                puckScript.InitPuckClientRpc(i == activeCompetitorIndex, puckSpriteID, clientRpcParamsList[i]);
            }
            puckScript.InitBlockPuckClientRpc();

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

    // Hydra Powerup
    [ServerRpc(RequireOwnership = true)] // this is true so it only fires once
    public void PuckSpawnHelperServerRpc(bool playersPuck, float x, float y, int spawnCount, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        // don't allow pucks below the safe line to spawn anything
        if (y < 0) return;

        // Get the ClientId of the client that sent this ServerRPC
        ulong clientId = rpcParams.Receive.SenderClientId;
        int competitorIndex = clients.IndexOf(clientId);
        Vector3 pos = Vector3.zero;

        try
        {
            if (!playersPuck) { competitorIndex ^= 1; }
            Competitor competitor = competitorList[competitorIndex];
            //Competitor competitor = playersPuck ? competitorList[competitorIndex] : competitorList[(competitorIndex == 0 ? 1 : 0)];
            //competitorIndex = competitorList.IndexOf(competitor);
            int puckSpriteID = competitor.puckSpriteID;

            // do X times (X is spwanCount)
            for (int i = 0; i < spawnCount; i++)
            {
                float randRange = 2.0f;
                // generate coordinates for potenial spawn, then see if it's too close to another puck
                bool tooClose = true;
                while (tooClose)
                {
                    pos = new Vector3(x + Random.Range(-randRange, randRange), System.Math.Max(0, y + Random.Range(-randRange, randRange)), 0);

                    tooClose = false;
                    var pucks = GameObject.FindGameObjectsWithTag("puck");
                    foreach (var puck in pucks)
                    {
                        if (Vector2.Distance(puck.transform.position, pos) < 2)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    // expand possible range until we find a valid postion
                    randRange += 0.1f;
                }
                GameObject puckObject = Instantiate(puck, pos, Quaternion.identity);
                puckObject.GetComponent<NetworkObject>().Spawn();

                PuckScript puckScript = puckObject.GetComponent<PuckScript>();

                // tell the active competitor this new puck is theirs, tell non-active competitors it's not theirs
                for (int j = 0; j < competitorList.Count; j++)
                {
                    puckScript.InitPuckClientRpc(j == competitorIndex, puckSpriteID, clientRpcParamsList[j]);
                }

                if (spawnCount == 2)
                {
                    puckScript.EnableHydraClientRpc();
                }

                Debug.Log(
                    $"Puck has been spawned. \n" +
                    $"Owned by Client Index #{competitorIndex} \n" +
                    $"Client ID : {clients[competitorIndex]} \n" +
                    $"Puck Skin ID: {competitor.puckSpriteID} \n");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            clientLogic.SetErrorMessageClientRpc(4);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AdjustPuckCountServerRpc(bool playersPuck, int value, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        int competitorIndex = clients.IndexOf(clientId);
        if (!playersPuck) { competitorIndex ^= 1; }

        competitorList[competitorIndex].puckCount += value;
        clientLogic.UpdatePuckCountClientRpc(true, competitorList[competitorIndex].puckCount, false, clientRpcParamsList[competitorIndex]);
        clientLogic.UpdatePuckCountClientRpc(false, competitorList[competitorIndex].puckCount, false, clientRpcParamsList[competitorIndex ^ 1]);
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncrementTriplePowerupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        int competitorIndex = clients.IndexOf(clientId);
        triplePowerupUserCompetitorIndex = competitorIndex;
        triplePowerup += 2;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShotTimerBoostServerRpc()
    {
        shotTimer += 10;
    }
}