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
    private int weakenCount;
    [ClientRpc] public void IncrementWeaken() { weakenCount++; Debug.Log("increment weaken"); }

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
            WallScript.Instance.WallEnabled(false);
            UI.UpdateWallText(wallCount);
            wallCount--;
        }

        // start turn, do this once then start shooting
        if (logic.player.isTurn && puckManager.AllPucksAreSlowedClient())
        {
            ServerLogicScript.Instance.CleanupDeadPucksServerRpc();
            activeBar = bar.ChangeBar("angle", logic.player.goingFirst);
            line.isActive = true;
            bar.SetWeakenBarCover(weakenCount);
            arrow.SetActive(true);
            GameHUDManager.Instance.ChangeTurnText("Your Turn");
            serverLogic.CreatePuckServerRpc();
            logic.player.isTurn = false;
            logic.player.isShooting = true;
            shotTimer = powerupsAreEnabled ? 30 : 18;
            if (powerupsAreEnabled) { powerupManager.ShuffleDeck(); }
            powerupsMenu.SetActive(powerupsAreEnabled);
        }

        if (logic.player.isShooting && Input.GetMouseButtonDown(0) && powerupsMenu.activeInHierarchy == false)
        {
            // make sure click is on the bottom half of the screen
            if (logic.ClickNotOnPuck())
            {
                switch (activeBar)
                {
                    case "angle":
                        angle = line.GetValue();
                        activeBar = bar.ChangeBar("power", logic.player.goingFirst);
                        SoundManagerScript.Instance.PlayClickSFXAlterPitch(1, 1f);
                        break;
                    case "power":
                        power = line.GetValue();
                        activeBar = bar.ChangeBar("spin");
                        SoundManagerScript.Instance.PlayClickSFXAlterPitch(1, 1.05f);
                        break;
                    case "spin":
                        spin = line.GetValue();
                        serverLogic.ShootServerRpc(angle, power, spin);
                        activeBar = bar.ChangeBar("none");
                        GameHUDManager.Instance.ChangeTurnText("Opponent's Turn");
                        UI.shotClockText.text = ""; // clear shot clock
                        line.isActive = false;
                        weakenCount = 0;
                        arrow.SetActive(false);
                        logic.player.isShooting = false;
                        SoundManagerScript.Instance.PlayClickSFXAlterPitch(1, 1.1f);
                        break;
                }
            }
        }

        // update shot clock text under 10 secs
        if (logic.player.isShooting && shotTimer <= 10.5 && shotTimer > 0)
        {
            UI.shotClockText.text = Mathf.RoundToInt(shotTimer).ToString();
        }
        // otherwise, clear the shot clock
        else if (UI.shotClockText.text != string.Empty)
        {
            UI.shotClockText.text = string.Empty;
        }
        // force shot after shot clock
        else if (logic.player.isShooting && shotTimer < 0)
        {
            angle = UnityEngine.Random.Range(20.0f, 80.0f);
            power = UnityEngine.Random.Range(30.0f, 60.0f);
            power = UnityEngine.Random.Range(45.0f, 55.0f);
            serverLogic.ShootServerRpc(angle, power, spin);
            activeBar = bar.ChangeBar("none");
            GameHUDManager.Instance.ChangeTurnText("Opponent's Turn");
            UI.shotClockText.text = ""; // clear shot clock
            line.isActive = false;
            weakenCount = 0;
            arrow.SetActive(false);
            logic.player.isShooting = false;
            powerupsMenu.SetActive(false);
        }
        // if we have no pucks left, don't show any bar
        if ((logic.player.isTurn || logic.player.isShooting) && isRunning && logic.player.puckCount <= 0)
        {
            activeBar = bar.ChangeBar("none");
            line.isActive = false;
            arrow.SetActive(false);
            GameHUDManager.Instance.ChangeTurnText(String.Empty);
            logic.player.isTurn = false;
            logic.player.isShooting = false;
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
    public void UpdatePuckCountClientRpc(bool isPlayer, int count, bool wasShot, ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;

        if (isPlayer)
        {
            logic.player.puckCount = count;
        }
        else
        {
            logic.opponent.puckCount = count;
        }
        GameHUDManager.Instance.ChangePuckCountText(isPlayer, count.ToString(), true);
        if (!wasShot) { return; } // methods of changing puck count without shoot return here (resurrect, pay1puck)
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
        LaserScript.Instance.DisableLaser();
        puckManager.ResetAlphaOnAllPucks();
    }

    // Server tells the client to switch to game scene and start the game.
    // Inputs the two puck skins used by the competitors.
    [ClientRpc]
    public void RestartGameOnlineClientRpc(int puckSpriteID_0, int puckSpriteID_1, bool powerupsEnabled = false)
    {
        if (!IsClient) return;

        UI.ChangeUI(UI.gameHud);

        UI.SetReButtons(false);
        if (powerupsAreEnabled) { powerupManager.LoadDeck(); }

        isRunning = true;
        wallCount = 3;
        WallScript.Instance.WallEnabled(true);
        UI.UpdateWallText(wallCount);
        receivedGameResult = false;
        powerupsAreEnabled = powerupsEnabled;
        logic.player.puckCount = 5;
        logic.player.scoreBonus = 0;
        logic.opponent.puckCount = 5;
        logic.opponent.scoreBonus = 0;
        logic.player.clientID = NetworkManager.Singleton.LocalClientId;
        logic.activeCompetitor = logic.opponent;
        PowerupAnimationManager.Instance.ClearPowerupPopupEffectAnimationQueue();

        Debug.Log("Client: Restarting game");
        Debug.Log($"player : {logic.player.puckSpriteID}     0 : {puckSpriteID_0}   1 : {puckSpriteID_1}");

        // Set player and opp skin. if both players use same skin, swap opp to alt skin
        if (puckSpriteID_0 == logic.player.puckSpriteID)
        {
            var swapAlt = puckSpriteID_0 == puckSpriteID_1 ? -1 : 1;
            UI.SetOpponentPuckIcon(puckSkinManager.ColorIDtoPuckSprite(puckSpriteID_1 * swapAlt), Math.Abs(puckSpriteID_1) == 40);
            logic.player.puckSpriteID = puckSpriteID_0;
        }
        else
        {
            var swapAlt = puckSpriteID_0 == puckSpriteID_1 ? -1 : 1;
            UI.SetOpponentPuckIcon(puckSkinManager.ColorIDtoPuckSprite(puckSpriteID_0 * swapAlt), Math.Abs(puckSpriteID_0) == 40);
            logic.player.puckSpriteID = puckSpriteID_1;
        }

        puckHalo.SetActive(false);
        bar.ToggleDim(false);
        weakenCount = 0;
        bar.SetWeakenBarCover(weakenCount);
        UI.onlineRematchButton.SetActive(false);
        GameHUDManager.Instance.ChangeTurnText("Opponent's Turn");
        line.GetComponent<LineScript>().FullSpeed();
    }

    // Server tells the client they can begin their turn
    [ClientRpc]
    public void StartTurnClientRpc(bool isPlayersTurn, bool isStartingPlayerParam, ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;

        if (isPlayersTurn)
        {
            Debug.Log("Client: Starting turn");
            logic.player.isTurn = true;
        }

        logic.activeCompetitor = isPlayersTurn ? logic.player : logic.opponent;

        logic.player.goingFirst = isStartingPlayerParam;

        LogicScript.Instance.UpdateScores();
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
        UI.DisableReadyButton();
    }

    public void StopGame()
    {
        isRunning = false;
        activeBar = bar.ChangeBar("none");
        line.isActive = false;
        if (logic.player != null)
        {
            logic.player.isTurn = false;
            logic.player.isShooting = false;
        }
        if (GameHUDManager.Instance != null)
        {
            GameHUDManager.Instance.ChangeTurnText(String.Empty);
        }
    }

    public void ShowRematchButton()
    {
        UI.onlineRematchButton.SetActive(true);
    }

    public bool IsStartingPlayer()
    {
        return logic.player.goingFirst;
    }

    [ClientRpc]
    public void AddPlayerACKClientRPC(ClientRpcParams clientRpcParams = default)
    {
        UI.DisableReadyButton();
    }

    public void ShotTimerBoost()
    {
        shotTimer += 10;
    }
}
