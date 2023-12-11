using Unity.Netcode;
using UnityEngine;

public class ClientLogicScript : NetworkBehaviour
{
    private UIManagerScript UI;
    private LogicScript logic;
    private ServerLogicScript serverLogic;

    private bool isTurn;
    private bool isShooting;

    private bool isRunning;

    private int puckCount;

    // bar and line
    private BarScript bar;
    private LineScript line;
    public string activeBar = "none";
    public float angle;
    public float power;
    public float spin;
    public GameObject puckHalo;

    private void OnEnable()
    {
        UI = GameObject.FindGameObjectWithTag("ui").GetComponent<UIManagerScript>();
        logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();
        serverLogic = GameObject.FindGameObjectWithTag("logic").GetComponent<ServerLogicScript>();
        bar = GameObject.FindGameObjectWithTag("bar").GetComponent<BarScript>();
        line = GameObject.FindGameObjectWithTag("line").GetComponent<LineScript>();
    }

    // Update is called once per frame
    void Update()
    {
        // start turn, do this once then start shooting
        if (isTurn && puckCount >= 0)
        {
            activeBar = bar.ChangeBar("angle");
            line.isActive = true;
            UI.TurnText = "Your Turn";
            serverLogic.CreatePuckServerRpc();
            isTurn = false;
            isShooting = true;
        }

        if (isShooting && Input.GetMouseButtonDown(0))
        {
            switch (activeBar)
            {
                case "angle":
                    angle = line.value;
                    activeBar = bar.ChangeBar("power");
                    break;
                case "power":
                    power = line.value;
                    activeBar = bar.ChangeBar("spin");
                    break;
                case "spin":
                    spin = line.value;
                    serverLogic.ShootServerRpc(angle, power, spin);
                    activeBar = bar.ChangeBar("none");
                    UI.TurnText = "Opponent's Turn";
                    line.isActive = false;
                    isShooting = false;
                    break;
            }
        }

        if ((isTurn || isShooting) && isRunning && puckCount <= 0)
        {
            activeBar = bar.ChangeBar("none");
            line.isActive = false;
            UI.TurnText = "";
            isTurn = false;
            isShooting = false;
        }
    }

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
    }

    [ClientRpc]
    public void UpdateReadyWaitingCompetitorsClientRpc()
    {
        if (!IsClient) return;

        UI.waitingText.text = "1/2 Players Ready";
    }

    [ClientRpc]
    public void GameOverConfirmationClientRpc()
    {
        if (!IsClient) return;

        Debug.Log("Game Over");
        isRunning = false;

        UI.TurnText = "Game Over";
    }

    [ClientRpc]
    public void RestartGameOnlineClientRpc(int puckSpriteID_0, int puckSpriteID_1)
    {
        if (!IsClient) return;

        puckCount = 5;
        isRunning = true;

        Debug.Log("Client: Restarting game");
        Debug.Log($"player : {logic.player.puckSpriteID}     0 : {puckSpriteID_0}   1 : {puckSpriteID_1}");

        // Set player and opp skin. if both players use same skin, swap opp to alt skin
        if (puckSpriteID_0 == logic.player.puckSpriteID)
        {
            var swapAlt = puckSpriteID_0 == puckSpriteID_1 ? -1 : 1;
            UI.SetOpponentPuckIcon(logic.ColorIDtoPuckSprite(puckSpriteID_1 * swapAlt));
        }
        else
        {
            var swapAlt = puckSpriteID_0 == puckSpriteID_1 ? -1 : 1;
            UI.SetOpponentPuckIcon(logic.ColorIDtoPuckSprite(puckSpriteID_0 * swapAlt));
        }

        puckHalo.SetActive(false);
        UI.ChangeUI(UI.gameHud);
        UI.TurnText = "Opponent's Turn";
    }

    [ClientRpc]
    public void StartTurnClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;

        Debug.Log("Client: Starting turn");
        isTurn = true;
    }
}
