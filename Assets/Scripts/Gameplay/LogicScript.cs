/* Logic Script is kinda like the "main" function for playing vs CPU, 
 * it controls most things happening in game and directs other scripts.
 */

using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class LogicScript : MonoBehaviour
{
    // self
    public static LogicScript Instance;

    // dependancies
    private UIManagerScript UI;
    private PuckManager puckManager;
    private PuckSkinManager puckSkinManager;
    private SoundManagerScript soundManager;
    private PowerupManager powerupManager;
    [SerializeField] private GameObject puckHalo; // set in editor

    // prefabs
    public GameObject puckPrefab; // default puck prefab

    // bar and line
    private BarScript bar;
    private LineScript line;
    public GameObject arrow;
    public string activeBar = "none";
    private float angle;
    private float power;
    private float spin;

    // wall
    private int wallCount = 3;
    [SerializeField] private GameObject wall; // set in editor
    public bool WallIsActive() { return wallCount > 0; }

    // powerups
    [SerializeField] private GameObject powerupsMenu; // set in editor
    public int triplePowerup;

    // game state
    public bool gameIsRunning { get; private set; }
    private float timer = 0;
    private int difficulty; // 0 easy 1 medium 2 hard

    private bool isLocal;
    public bool IsLocal
    {
        get => isLocal;
        set
        {
            isLocal = value;
        }
    }

    public delegate void PlayerShot();
    public static event PlayerShot OnPlayerShot;
    public delegate void OpponentShot();
    public static event PlayerShot OnOpponentShot;

    // temp variables
    private float tempTime = 0;
    private float CPUShotAngle = -1;
    private float CPUShotPower = -1;
    private float CPUShotSpin = -1;
    public Competitor player;
    public Competitor opponent;
    public Competitor activeCompetitor;
    public Competitor nonActiveCompetitor;
    public bool playedAGame = false;
    public bool tutorialActive = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    // Start is called before the first frame update
    void Start()
    {
        PowerupCardData.EncodeDecodeTest();

#if (UNITY_EDITOR)
        PlayerPrefs.SetInt("easyWin", 0);
        PlayerPrefs.SetInt("easyHighscore", 14);
        PlayerPrefs.SetInt("mediumHighscore", 12);
        PlayerPrefs.SetInt("hardHighscore", 10);
        // TODO: REMOVE
        PlayerPrefs.SetInt("CraftingCredits", 999);
        PlayerPrefs.SetInt("StandardPacks", 999);
        PlayerPrefs.SetInt("PlusPacks", 999);
#endif

        // connect scripts
        bar = BarScript.Instance;
        line = LineScript.Instance;
        UI = UIManagerScript.Instance;
        puckManager = PuckManager.Instance;
        puckSkinManager = PuckSkinManager.Instance;
        soundManager = SoundManagerScript.Instance;
        powerupManager = PowerupManager.Instance;

        // create player objects
        player = new Competitor { isPlayer = true };
        opponent = new Competitor { isPlayer = false };
        Application.targetFrameRate = PlayerPrefs.GetInt("FPS", 90);

        // load play prefs data
        UI.UpdateProfileText();
        // load puck id, but not dev skin
        PuckSkinManager.Instance.SelectPlayerPuckSprite(PlayerPrefs.GetInt("puck") == 0 && PlayerPrefs.GetInt("hardHighscore") <= 5 ? 1 : PlayerPrefs.GetInt("puck"));
        activeCompetitor = opponent;
        nonActiveCompetitor = player;
        // make sure we're on title screen
        UI.ChangeUI(UI.titleScreen);
    }

    // Update is called once per frame
    void Update()
    {
        if (tutorialActive && UI.iPage < 6)
        {
            return;
        }

        // only do this stuff when game is running (not in menu etc.)
        if (gameIsRunning && (player.puckCount > 0 || opponent.puckCount > 0))
        {
            // update wall status
            if (wallCount == 0 && puckManager.AllPucksAreSlowedMore())
            {
                WallScript.Instance.WallEnabled(false);
                UI.UpdateWallText(wallCount);
                wallCount--;
            }

            // do triple powerup
            if (triplePowerup > 0 && activeCompetitor.activePuckScript != null && activeCompetitor.activePuckScript.IsSafe())
            {
                activeCompetitor.activePuckScript.RemovePowerupText("triple");
                puckManager.CreatePuck(activeCompetitor);
                activeCompetitor.ShootActivePuck(triplePower + Random.Range(-10.0f, 10.0f), tripleAngle + Random.Range(-10.0f, 10.0f), 50, false);
                triplePowerup--;
            }

            // start Players turn, do this then start shooting
            if (player.isTurn && !player.isShooting && ((player.goingFirst && opponent.puckCount == 5) || puckManager.AllPucksAreSlowed()))
            {
                StartingPlayersTurnHelper();
            }

            // now player may shoot
            if ((Input.GetMouseButtonDown(0)) && gameIsRunning && (player.isShooting || isLocal) && powerupsMenu.activeInHierarchy == false)
            {
                // make sure click is not on a puck
                if (ClickNotOnPuck())
                {
                    PlayerShootingHelper();
                }
            }

            // start CPU's turn, do this then start shooting
            if (opponent.isTurn && (player.activePuckScript == null || puckManager.AllPucksAreSlowed() && difficulty < 2 || puckManager.AllPucksAreSlowedMore()))
            {
                StartingOpponentsTurnHelper();
            }
            // now opponent may shoot
            if (opponent.isShooting && !isLocal)
            {
                OpponentShootingHelper();
            }

            // lastly, increment timer while game is running
            timer += Time.deltaTime;
        }
        // ran out of pucks (game over)
        else if (gameIsRunning && puckManager.AllPucksAreStopped() && puckManager.AllPucksAreNotNearingScoreZoneEdge())
        {
            gameIsRunning = false;
            UpdateScores();
            UI.ChangeUI(UI.gameResultScreen);
            UI.UpdateGameResult(player.GetScore(), opponent.GetScore(), difficulty, isLocal);
            isLocal = false;
            arrow.SetActive(false);
            FogScript.Instance.DisableFog();
        }
    }

    // main gameplay helpers
    private void StartingPlayersTurnHelper()
    {
        if (tutorialActive && UI.iPage == 6)
        {
            UI.AdvanceTutorial();
        }

        // if we start our turn with no pucks remaining, skip our turn
        if (player.puckCount <= 0 && opponent.puckCount > 0)
        {
            player.isTurn = false;
            opponent.isTurn = true;
            return;
        }

        activeCompetitor = player;
        nonActiveCompetitor = opponent;

        puckHalo.SetActive(difficulty == 0);
        activeBar = bar.ChangeBar("angle", activeCompetitor.isPlayer);
        bar.ToggleDim(false);
        line.isActive = true;
        arrow.SetActive(true);
        GameHUDManager.Instance.ChangeTurnText(isLocal ? "Player 1's Turn" : "Your Turn");
        if (player.puckCount == 1)
        {
            GameHUDManager.Instance.ChangeTurnText("LAST PUCK");
        }
        puckManager.CreatePuck(player);
        player.isTurn = false;
        player.isShooting = true;
        if (wallCount > 0)
        {
            wallCount--;
            if (wallCount > 0) { UI.UpdateWallText(wallCount); }
        }

        if (!isLocal)
        {
            powerupManager.ShuffleDeck();
            powerupsMenu.SetActive(true);
        }
    }

    private void PlayerShootingHelper()
    {
        // change state on tap depending on current state
        switch (activeBar)
        {
            case "angle":
                soundManager.PlayClickSFXAlterPitch(1, 1f);
                angle = line.GetValue();
                activeBar = bar.ChangeBar("power", activeCompetitor.isPlayer);
                break;
            case "power":
                soundManager.PlayClickSFXAlterPitch(1, 1.05f);
                power = line.GetValue();
                // if non-hard diff, end turn
                if (difficulty < 2)
                {
                    Shoot(angle, power);
                }
                // on hard diff, show spin bar
                else
                {
                    activeBar = bar.ChangeBar("spin");
                }
                break;
            // if hard, select spin
            case "spin":
                soundManager.PlayClickSFXAlterPitch(1, 1.1f);
                spin = line.GetValue();
                Shoot(angle, power, spin);
                break;
        }
    }

    private void StartingOpponentsTurnHelper()
    {
        // if opponents starts their turn with no pucks remaining, skip their turn
        if (opponent.puckCount <= 0 && player.puckCount > 0)
        {
            opponent.isTurn = false;
            player.isTurn = true;
            return;
        }

        activeCompetitor = opponent;
        nonActiveCompetitor = player;

        activeBar = bar.ChangeBar("angle", activeCompetitor.isPlayer);
        if (!isLocal)
        {
            bar.ToggleDim(true);
        }
        line.isActive = true;
        arrow.SetActive(true);
        puckHalo.SetActive(difficulty == 0);
        GameHUDManager.Instance.ChangeTurnText(isLocal ? "Player 2's Turn" : "CPU's Turn");
        if (opponent.puckCount == 1 && isLocal)
        {
            GameHUDManager.Instance.ChangeTurnText("LAST PUCK");
        }
        puckManager.CreatePuck(opponent);
        opponent.isTurn = false;
        opponent.isShooting = true;

        if (wallCount > 0)
        {
            wallCount--;
            if (wallCount > 0) { UI.UpdateWallText(wallCount); }
        }

        CPUShotAngle = -1f;
        CPUShotPower = -1f;
        CPUShotAngle = -1f;

        CPUBehaviorScript.TurnReset();

        readyToStartOpponentTurn = true;
    }

    bool readyToStartOpponentTurn = false;
    public float powerupWaitTime = 0;
    private void OpponentShootingHelper()
    {
        // timestamp the beginning of CPU's turn for delays
        if (tempTime == 0)
        {
            tempTime = timer;
        }

        if (CPUShotAngle < 0) (CPUShotAngle, CPUShotPower, CPUShotSpin) = CPUBehaviorScript.Think(timer - tempTime);

        // after 1.5 seconds elapsed, CPU selects angle
        if (CPUShotAngle >= 0 && Mathf.Abs(line.GetValue() - CPUShotAngle) < (timer - (tempTime + 1.5 + powerupWaitTime))/2 && activeBar == "angle")
        {
            activeBar = bar.ChangeBar("power", activeCompetitor.isPlayer);
        }
        // after 3 seconds elapsed, CPU selects power
        if (CPUShotPower >= 0 && Mathf.Abs(line.GetValue() - CPUShotPower) < (timer - (tempTime + 3 + powerupWaitTime))/2 && activeBar == "power")
        {
            if (difficulty < 2)
            {
                Shoot(CPUShotAngle, CPUShotPower);
                tempTime = 0;
            }
            else
            {
                activeBar = bar.ChangeBar("spin");
            }
        }
        // after 4.5 seconds elapsed, CPU selects spin (for hard mode only)
        if (CPUShotSpin >= 0 && Mathf.Abs(line.GetValue() - CPUShotSpin) < (timer - (tempTime + 4.5 + powerupWaitTime))/2 && activeBar == "spin")
        {
            Shoot(CPUShotAngle, CPUShotPower, CPUShotSpin);
            tempTime = 0;
        }
    }

    private float triplePower, tripleAngle, tripleSpin;
    private void Shoot(float angle, float power, float spin = 50.0f)
    {
        puckManager.CleanupDeadPucks();
        powerupManager.DisableForceFieldIfNecessary();
        Debug.Log("Shooting: " + angle + " | " + power + " | " + spin);
        activeBar = bar.ChangeBar("none");
        line.isActive = false;
        arrow.SetActive(false);
        GameHUDManager.Instance.ChangeTurnText(string.Empty);
        activeCompetitor.ShootActivePuck(angle, power, spin);
        (triplePower, tripleAngle, tripleSpin) = (angle, power, spin);
        UI.PostShotUpdate(player.puckCount, opponent.puckCount);
        UI.UpdateShotDebugText(angle, power, spin);
        if (activeCompetitor.isPlayer)
        {
            OnPlayerShot?.Invoke();
            player.isTurn = false;
            opponent.isTurn = true;
        }
        else
        {
            OnOpponentShot?.Invoke();
            player.isTurn = true;
            opponent.isTurn = false;
        }
    }

    public void RestartGame(int diff)
    {
        puckManager.ClearAllPucks();
        if (tutorialActive)
        {
            // check if tutorial should be active
            tutorialActive = PlayerPrefs.GetInt("tutorialCompleted") == 0 && PlayerPrefs.GetInt("easyWin") == 0 && PlayerPrefs.GetInt("easyHighscore") == 0;
        }
        playedAGame = true;
        // organize scene
        puckManager.ClearAllPucks();
        // reset game variables
        puckSkinManager.RandomizeCPUPuckSprite();
        // if diff is -1, that means this function is called from the rematch button, so don't change the difficulty
        if (diff >= 0)
        {
            difficulty = diff;
        }
        player.ResetProperties();
        opponent.ResetProperties();
        UI.PostShotUpdate(player.puckCount, opponent.puckCount);
        UpdateScores();
        PowerupAnimationManager.Instance.ClearPowerupPopupEffectAnimationQueue();
        triplePowerup = 0;
        LaserScript.Instance.DisableLaser();

        // load player & CPU decks
        powerupManager.LoadDeck();
        CPUBehaviorScript.FullReset(difficulty);

        if (difficulty < 2) // for easy & medium
        {
            player.isTurn = false;
            opponent.isTurn = true;
            activeCompetitor = opponent;
            nonActiveCompetitor = player;
            player.goingFirst = false;
            opponent.goingFirst = true;
        }
        else // for hard
        {
            player.isTurn = true;
            opponent.isTurn = false;
            activeCompetitor = player;
            nonActiveCompetitor = opponent;
            player.goingFirst = true;
            opponent.goingFirst = false;
        }

        if (difficulty == 0) {
            wallCount = 0;
            WallScript.Instance.WallEnabled(false);
        }
        else
        {
            wallCount = 3;
            WallScript.Instance.WallEnabled(true);
        }
        UI.UpdateWallText(wallCount);

        UI.SetReButtons(true);
        gameIsRunning = true;
        puckHalo.SetActive(difficulty == 0);
        line.isActive = false;
        line.GetComponent<LineScript>().FullSpeed();
        bar.ChangeBar("none");
        UI.ChangeUI(UI.gameHud);
        powerupManager.DisableForceFieldIfNecessary();
        FogScript.Instance.DisableFog();
        powerupsMenu.SetActive(false);
    }

    public bool LeftsTurn()
    {
        return (((player.isTurn || player.isShooting) && player.goingFirst) || ((opponent.isTurn || opponent.isShooting) && !player.goingFirst));
    }

    // update the scores
    public void UpdateScores()
    {
        (player.score, opponent.score) = puckManager.UpdateScores();
        UI.UpdateScores(player.GetScore(), opponent.GetScore());
        UI.UpdateScoreBonuses(player.scoreBonus, opponent.scoreBonus);
    }

    // back button in game
    public void ReturnToMenu()
    {
        gameIsRunning = false;
        isLocal = false;
        player.puckCount = 0;
        opponent.puckCount = 0;
        UI.ChangeUI(UI.titleScreen);
        line.isActive = false;
        bar.ChangeBar("none");
        arrow.SetActive(false);
        powerupsMenu.SetActive(false);
        FogScript.Instance.DisableFog();
        WallScript.Instance.WallEnabled(false);
        LaserScript.Instance.DisableLaser();
        puckManager.ClearAllPucks();
    }

    public void IncrementPuckCount(bool playersPuck, int value = 1)
    {
        if (playersPuck)
        {
            player.puckCount += value;
        }
        else
        {
            opponent.puckCount += value;
        }
        UI.PostShotUpdate(player.puckCount, opponent.puckCount);
    }

    public void ModifyScoreBonus(bool isplayer, int value)
    {
        if (isplayer)
        {
            player.scoreBonus += value;
        }
        else
        {
            opponent.scoreBonus += value;
        }
        UI.UpdateScores(player.GetScore(), opponent.GetScore());
        UI.UpdateScoreBonuses(player.scoreBonus, opponent.scoreBonus);
    }

    // O(n) + O(m) where n is gameobjects in the scene and m is pucks
    public bool ClickNotOnPuck()
    {
        Vector3 mousePosition = Input.mousePosition; // Get mouse position in screen space
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition); // Convert to world space

        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            worldPosition.z = puck.transform.position.z; // Ensure the z-coordinate is set as the same as the puck to compare
            // Calculate the distance between the mouse click position and the puck position
            float distance = Vector3.Distance(worldPosition, puck.transform.position);

            // If the distance is less than or equal to 1 unit, the click is on or near a puck
            if (distance <= 1f)
            {
                return false; // Click is too close to a puck
            }
        }
        return true; // Click is not near any puck
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
