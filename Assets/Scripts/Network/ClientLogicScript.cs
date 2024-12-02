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

    private bool isTurn;
    private bool isShooting;
    public bool isStartingPlayer { get; private set; }

    public bool isRunning { get; private set; }
    private float shotTimer;

    private int puckCount;

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
        if (isTurn && puckCount > 0 && puckManager.AllPucksAreSlowed())
        {
            activeBar = bar.ChangeBar("angle", isStartingPlayer);
            line.isActive = true;
            arrow.SetActive(true);
            UI.TurnText = "Your Turn";
            serverLogic.CreatePuckServerRpc();
            isTurn = false;
            isShooting = true;
            shotTimer = 18;
        }

        if (isShooting && Input.GetMouseButtonDown(0))
        {
            switch (activeBar)
            {
                case "angle":
                    angle = line.GetValue();
                    activeBar = bar.ChangeBar("power", isStartingPlayer);
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
                    isShooting = false;
                    break;
            }
        }

        // update shot clock text under 10 secs
        if (isShooting && shotTimer <= 10.5 && shotTimer > 0)
        {
            UI.shotClockText.text = Mathf.RoundToInt(shotTimer).ToString();
        }
        // clear shot clock when not turn
        else if (!isShooting && !isTurn)
        {
            UI.shotClockText.text = "";
        }
        // force shot after shot clock
        else if (isShooting && shotTimer < 0)
        {
            angle = UnityEngine.Random.Range(20.0f, 80.0f);
            power = UnityEngine.Random.Range(30.0f, 60.0f);
            power = UnityEngine.Random.Range(45.0f, 55.0f);
            serverLogic.ShootServerRpc(angle, power, spin);
            activeBar = bar.ChangeBar("none");
            UI.TurnText = "Opponent's Turn";
            line.isActive = false;
            arrow.SetActive(false);
            isShooting = false;
        }

        if ((isTurn || isShooting) && isRunning && puckCount <= 0)
        {
            activeBar = bar.ChangeBar("none");
            line.isActive = false;
            arrow.SetActive(false);
            UI.TurnText = "";
            isTurn = false;
            isShooting = false;
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
            puckCount = count;
            UI.playerPuckCountText.text = count.ToString();
        }
        else
        {
            UI.opponentPuckCountText.text = count.ToString();
        }
        DecrementWallCount();
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
    }

    // Server updates us with match result, for now we fetch score local
    [ClientRpc]
    public void GameResultClientRpc()
    {
        if (!IsClient) return;

        UI.UpdateGameResult(-1, -1, -1, false, true);
        UI.ChangeUI(UI.gameResultScreen);
        arrow.SetActive(false);
    }

    // Server tells the client to switch to game scene and start the game.
    // Inputs the two puck skins used by the competitors.
    [ClientRpc]
    public void RestartGameOnlineClientRpc(int puckSpriteID_0, int puckSpriteID_1)
    {
        if (!IsClient) return;

        UI.SetReButtons(false);

        puckCount = 5;
        isRunning = true;
        wallCount = 3;
        wall.SetActive(true);
        UI.UpdateWallText(wallCount);

        Debug.Log("Client: Restarting game");
        Debug.Log($"player : {logic.player.puckSpriteID}     0 : {puckSpriteID_0}   1 : {puckSpriteID_1}");

        // Set player and opp skin. if both players use same skin, swap opp to alt skin
        if (puckSpriteID_0 == logic.player.puckSpriteID)
        {
            var swapAlt = puckSpriteID_0 == puckSpriteID_1 ? -1 : 1;
            UI.SetOpponentPuckIcon(puckSkinManager.ColorIDtoPuckSprite(puckSpriteID_1 * swapAlt), Math.Abs(puckSpriteID_1) == 40);
        }
        else
        {
            var swapAlt = puckSpriteID_0 == puckSpriteID_1 ? -1 : 1;
            UI.SetOpponentPuckIcon(puckSkinManager.ColorIDtoPuckSprite(puckSpriteID_0 * swapAlt), Math.Abs(puckSpriteID_0) == 40);
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
        isTurn = true;
        isStartingPlayer = isStartingPlayerParam;
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
        isTurn = false;
        isShooting = false;
    }

    public void ShowRematchButton()
    {
        UI.onlineRematchButton.SetActive(true);
    }
}
