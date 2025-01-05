/* This script runs the logic which is executed on the client during online play.
*  Functions marked [ClientRpc] are functions which are called by the server and executed on the client.
*  Functions with "ClientRpcParams clientRpcParams = default" as a parameter are only executed by 1 of the 2 competitors.
*/

using System;
using Unity.Netcode;
using UnityEngine;

public class ClientLogicScript : NetworkBehaviour
{
    // self
    public static ClientLogicScript Instance;

    // dependancies
    private UIManagerScript UI;
    private LogicScript logic;
    private ServerLogicScript serverLogic;
    private PuckSkinManager puckSkinManager;
    private PuckManager puckManager;
    private PowerupManager powerupManager;

    // client competitor variable
    public Competitor client;

    public bool isRunning { get; private set; }
    private float shotTimer;

    private bool receivedGameResult = false;

    private bool powerupsAreEnabled;
    [SerializeField] private GameObject powerupsMenu;

    // bar and line
    private BarScript bar;
    private LineScript line;
    public GameObject arrow;
    public string activeBar = "none";
    public float angle;
    public float power;
    public float spin;
    public GameObject puckHalo;

    // wall
    private int wallCount = 3;
    [SerializeField] private GameObject wall; // set in editor

    // game state / events
    public delegate void PlayerShot();
    public static event PlayerShot OnPlayerShot;
    public delegate void OpponentShot();
    public static event OpponentShot OnOpponentShot;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void Start()
    {
        UI = UIManagerScript.Instance;
        logic = LogicScript.Instance;
        serverLogic = ServerLogicScript.Instance;
        bar = BarScript.Instance;
        line = LineScript.Instance;
        puckSkinManager = PuckSkinManager.Instance;
        puckManager = PuckManager.Instance;
        powerupManager = PowerupManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRunning) { return; }

        // update wall status
        if (wallCount == 0)
        {
            wall.SetActive(false);
            UI.UpdateWallText(wallCount);
            wallCount--;
        }

        // start turn, do this once then start shooting
        if (client.isTurn && puckManager.AllPucksAreSlowedClient())
        {
            activeBar = bar.ChangeBar("angle", client.goingFirst);
            line.isActive = true;
            arrow.SetActive(true);
            UI.TurnText = "Your Turn";
            serverLogic.CreatePuckServerRpc();
            client.isTurn = false;
            client.isShooting = true;
            shotTimer = powerupsAreEnabled ? 30 : 18;
            powerupsMenu.SetActive(powerupsAreEnabled);
            if (powerupsAreEnabled) { powerupManager.ShufflePowerups(); }
        }

        if (client.isShooting && Input.GetMouseButtonDown(0) && powerupsMenu.activeInHierarchy == false)
        {
            // make sure click is on the bottom half of the screen
            if (logic.ClickNotOnPuck())
            {
                switch (activeBar)
                {
                    case "angle":
                        angle = line.GetValue();
                        activeBar = bar.ChangeBar("power", client.goingFirst);
                        break;
                    case "power":
                        power = line.GetValue();
                        activeBar = bar.ChangeBar("spin");
                        break;
                    case "spin":
                        spin = line.GetValue();
                        serverLogic.ShootServerRpc(angle, power, spin);
                        activeBar = bar.ChangeBar("none");
                        UI.TurnText = "Opponent's Turn";
                        line.isActive = false;
                        arrow.SetActive(false);
                        client.isShooting = false;
                        break;
                }
            }
        }

        // update shot clock text under 10 secs
        if (client.isShooting && shotTimer <= 10.5 && shotTimer > 0)
        {
            UI.shotClockText.text = Mathf.RoundToInt(shotTimer).ToString();
        }
        // clear shot clock when not turn
        else if (!client.isShooting && !client.isTurn)
        {
            UI.shotClockText.text = "";
        }
        // force shot after shot clock
        else if (client.isShooting && shotTimer < 0)
        {
            angle = UnityEngine.Random.Range(20.0f, 80.0f);
            power = UnityEngine.Random.Range(30.0f, 60.0f);
            power = UnityEngine.Random.Range(45.0f, 55.0f);
            serverLogic.ShootServerRpc(angle, power, spin);
            activeBar = bar.ChangeBar("none");
            UI.TurnText = "Opponent's Turn";
            line.isActive = false;
            arrow.SetActive(false);
            client.isShooting = false;
            powerupsMenu.SetActive(false);
        }
        // if we have no pucks left, don't show any bar
        if ((client.isTurn || client.isShooting) && isRunning && client.puckCount <= 0)
        {
            activeBar = bar.ChangeBar("none");
            line.isActive = false;
            arrow.SetActive(false);
            UI.TurnText = "";
            client.isTurn = false;
            client.isShooting = false;
        }
        shotTimer -= Time.deltaTime;
    }

    private void DecrementWallCount()
    {
        if (wallCount > 0)
        {
            wallCount--;
            if (wallCount > 0) { UI.UpdateWallText(wallCount); }
        }
    }

    // Server tells the client to update in-game UI showing puck counts for each player
    [ClientRpc]
    public void UpdatePuckCountClientRpc(bool isPlayer, int count, ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;

        if (isPlayer)
        {
            client.puckCount = count;
            UI.playerPuckCountText.text = count.ToString();
        }
        else
        {
            UI.opponentPuckCountText.text = count.ToString();
        }
        DecrementWallCount();
        PowerupManager.Instance.DisableForceFieldIfNecessary();
        if (isPlayer)
        {
            OnPlayerShot?.Invoke();
        }
        else
        {
            OnOpponentShot?.Invoke();
        }
        puckHalo.SetActive(false);
    }

    // Server tells the client to update online waiting screen to show that a competitor has clicked the ready button
    [ClientRpc]
    public void UpdateReadyWaitingCompetitorsClientRpc()
    {
        if (!IsClient) return;

        UI.waitingText.text = "1/2 Players Ready";
    }

    // Server tells the client the game is over, and to display "game over" to the player
    [ClientRpc]
    public void GameOverConfirmationClientRpc()
    {
        if (!IsClient) return;

        Debug.Log("Game Over");
        isRunning = false;
        UI.TurnText = "";
        FogScript.Instance.DisableFog();
    }

    // Server updates us with match result, for now we fetch score local
    [ClientRpc]
    public void GameResultClientRpc()
    {
        if (!IsClient) return;
        if (receivedGameResult) return;

        UI.UpdateGameResult(-1, -1, -1, false, true);
        UI.ChangeUI(UI.gameResultScreen);
        arrow.SetActive(false);
        receivedGameResult = true;
        FogScript.Instance.DisableFog();
    }

    // Server tells the client to switch to game scene and start the game.
    // Inputs the two puck skins used by the competitors.
    [ClientRpc]
    public void RestartGameOnlineClientRpc(int puckSpriteID_0, int puckSpriteID_1, bool powerupsEnabled = false)
    {
        if (!IsClient) return;

        UI.SetReButtons(false);
        if (powerupsAreEnabled) { powerupManager.LoadDeck(); }

        isRunning = true;
        wallCount = 3;
        wall.SetActive(true);
        UI.UpdateWallText(wallCount);
        receivedGameResult = false;
        powerupsAreEnabled = powerupsEnabled;
        client = new();
        client.puckCount = 5;
        client.clientID = NetworkManager.Singleton.LocalClientId;
        logic.activeCompetitor = client;

        Debug.Log("Client: Restarting game");
        Debug.Log($"player : {logic.player.puckSpriteID}     0 : {puckSpriteID_0}   1 : {puckSpriteID_1}");

        // Set player and opp skin. if both players use same skin, swap opp to alt skin
        if (puckSpriteID_0 == logic.player.puckSpriteID)
        {
            var swapAlt = puckSpriteID_0 == puckSpriteID_1 ? -1 : 1;
            UI.SetOpponentPuckIcon(puckSkinManager.ColorIDtoPuckSprite(puckSpriteID_1 * swapAlt), Math.Abs(puckSpriteID_1) == 40);
            client.puckSpriteID = puckSpriteID_0;
        }
        else
        {
            var swapAlt = puckSpriteID_0 == puckSpriteID_1 ? -1 : 1;
            UI.SetOpponentPuckIcon(puckSkinManager.ColorIDtoPuckSprite(puckSpriteID_0 * swapAlt), Math.Abs(puckSpriteID_0) == 40);
            client.puckSpriteID = puckSpriteID_1;
        }

        puckHalo.SetActive(false);
        bar.ToggleDim(false);
        UI.onlineRematchButton.SetActive(false);
        UI.ChangeUI(UI.gameHud);
        UI.TurnText = "Opponent's Turn";
    }

    // Server tells the client they can begin their turn
    [ClientRpc]
    public void StartTurnClientRpc(bool isStartingPlayerParam, ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;

        Debug.Log("Client: Starting turn");
        client.isTurn = true;
        client.goingFirst = isStartingPlayerParam;
    }

    // Server tells the client to set the error message.
    // This happens when an error happens on the server.
    // The codes are just used for debugging, instead of digging in the server logs.
    [ClientRpc]
    public void SetErrorMessageClientRpc(int code)
    {
        if (!IsClient) return;
        UI.SetErrorMessage("Server Error: " + code + " - Contact developer.");
    }

    [ClientRpc]
    public void AlertDisconnectClientRpc()
    {
        if (!IsClient) return;
        UI.SetErrorMessage("Your opponent has disconnected.");
    }

    public void StopGame()
    {
        isRunning = false;
        UI.TurnText = "";
        activeBar = bar.ChangeBar("none");
        line.isActive = false;
        if (client != null)
        {
            client.isTurn = false;
            client.isShooting = false;
        }
    }

    public void ShowRematchButton()
    {
        UI.onlineRematchButton.SetActive(true);
    }

    public bool IsStartingPlayer()
    {
        return client.goingFirst;
    }
}
