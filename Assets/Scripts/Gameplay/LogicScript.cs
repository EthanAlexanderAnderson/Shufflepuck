/* Logic Script is kinda like the "main" function for playing vs CPU, 
 * it controls most things happening in game and directs other scripts.
 */

using System;
using UnityEngine;
using UnityEngine.UI;
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

    // Hard CPU shot paths
    GameObject[] CPUPaths;

    // powerups
    public bool powerupsAreEnabled = false;
    [SerializeField] private GameObject powerupsMenu; // set in editor
    private bool powerupHasBeenUsedThisTurn = false;

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

    // temp variables
    private float tempTime = 0;
    public float CPUShotAngle = 0;
    public float CPUShotPower = 0;
    public float CPUShotSpin = 50;
    public Competitor player;
    public Competitor opponent;
    public Competitor activeCompetitor;
    public Competitor nonActiveCompetitor;
    public bool playedAGame = false;
    public bool tutorialActive;

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
#if (UNITY_EDITOR)
        PlayerPrefs.SetInt("easyWin", 0);
        PlayerPrefs.SetInt("easyHighscore", 14);
        PlayerPrefs.SetInt("mediumHighscore", 12);
        PlayerPrefs.SetInt("hardHighscore", 10);
#endif

        // connect scripts
        bar = BarScript.Instance;
        line = LineScript.Instance;
        UI = UIManagerScript.Instance;
        puckManager = PuckManager.Instance;
        puckSkinManager = PuckSkinManager.Instance;
        soundManager = SoundManagerScript.Instance;

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
        // check if tutorial should be active
        tutorialActive = PlayerPrefs.GetInt("tutorialCompleted") == 0 && PlayerPrefs.GetInt("easyWin") == 0 && PlayerPrefs.GetInt("easyHighscore") == 0;
        // initialize CPU paths
        CPUPaths = GameObject.FindGameObjectsWithTag("cpu_path");
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
            // clear pucks in non-safe zones
            puckManager.CleanupDeadPucks();

            // update wall status
            if (wallCount == 0 && puckManager.AllPucksAreSlowedMore())
            {
                wall.SetActive(false);
                UI.UpdateWallText(wallCount);
                wallCount--;
            }

            // start Players turn, do this then start shooting
            if (player.isTurn && !player.isShooting && ((player.goingFirst && opponent.puckCount == 5) || opponent.activePuckScript.IsSlowed()))
            {
                StartingPlayersTurnHelper();
            }

            // now player may shoot
            if ((Input.GetMouseButtonDown(0)) && gameIsRunning && (player.isShooting || isLocal) && powerupsMenu.activeInHierarchy == false)
            {
                // make sure click is on the bottom half of the screen
                if (Input.mousePosition.y < Screen.height / 2)
                {
                    PlayerShootingHelper();
                }
            }

            // start CPU's turn, do this then start shooting
            if (opponent.isTurn && (player.activePuckScript == null || (player.activePuckScript.IsSlowed() && (puckManager.AllPucksAreSlowed() && difficulty < 2 || puckManager.AllPucksAreSlowedMore()))))
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
        else if (gameIsRunning && puckManager.AllPucksAreStopped())
        {
            gameIsRunning = false;
            UpdateScores();
            UI.ChangeUI(UI.gameResultScreen);
            UI.UpdateGameResult(player.score, opponent.score, difficulty, isLocal);
            isLocal = false;
            arrow.SetActive(false);
        }
    }

    // main gameplay helpers
    private void StartingPlayersTurnHelper()
    {
        if (tutorialActive && UI.iPage == 6)
        {
            UI.AdvanceTutorial();
        }

        if (difficulty >= 2 && !isLocal)
        {
            powerupsMenu.SetActive(powerupsAreEnabled);
            if (powerupsAreEnabled) { ShufflePowerups(); }
            powerupHasBeenUsedThisTurn = false;
        }
        puckHalo.SetActive(difficulty == 0);
        activeBar = bar.ChangeBar("angle", activeCompetitor.isPlayer);
        bar.ToggleDim(false);
        line.isActive = true;
        arrow.SetActive(true);
        UI.TurnText = isLocal ? "Player 1's Turn" : "Your Turn";
        if (player.puckCount == 1)
        {
            UI.TurnText = "LAST PUCK";
        }
        puckManager.CreatePuck(player);
        player.isTurn = false;
        player.isShooting = true;
        if (wallCount > 0)
        {
            wallCount--;
            if (wallCount > 0) { UI.UpdateWallText(wallCount); }
        }
    }

    private void PlayerShootingHelper()
    {
        soundManager.PlayClickSFX();
        // change state on tap depending on current state
        switch (activeBar)
        {
            case "angle":
                angle = line.GetValue();
                activeBar = bar.ChangeBar("power", activeCompetitor.isPlayer);
                break;
            case "power":
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
                spin = line.GetValue();
                Shoot(angle, power, spin);
                break;
        }
    }

    private void StartingOpponentsTurnHelper()
    {
        powerupHasBeenUsedThisTurn = false;
        activeBar = bar.ChangeBar("angle", activeCompetitor.isPlayer);
        if (!isLocal)
        {
            bar.ToggleDim(true);
        }
        line.isActive = true;
        arrow.SetActive(true);
        UI.TurnText = isLocal ? "Player 2's Turn" : "CPU's Turn";
        if (opponent.puckCount == 1 && isLocal)
        {
            UI.TurnText = "LAST PUCK";
        }
        puckManager.CreatePuck(opponent);
        opponent.isTurn = false;
        opponent.isShooting = true;

        if (wallCount > 0)
        {
            wallCount--;
            if (wallCount > 0) { UI.UpdateWallText(wallCount); }
        }
        // first turn on med-hard diff, CPU Shoots perfect
        if (difficulty == 1 && opponent.puckCount == 5)
        {
            CPUShotAngle = Random.Range(33.0f, 38.0f);
            CPUShotPower = Random.Range(65.0f, 78.0f);
        }
        // easy-med regular shots only
        else if (difficulty < 2)
        {
            CPUShotAngle = Random.Range(11.0f + (difficulty * 5.0f), 63.0f - (difficulty * 5.0f));
            CPUShotPower = Random.Range(30.0f + (difficulty * 5.0f), 90.0f - (difficulty * 5.0f));
        }
        // hard uses paths, and random if no path is found
        else
        {
            (CPUShotAngle, CPUShotPower, CPUShotSpin) = FindOpenPath();
        }

        // use powerup (this must be after CPU find path, to not double-use powerups after a phase shot path has been selected)
        if (difficulty >= 2 && !isLocal && powerupsAreEnabled && !powerupHasBeenUsedThisTurn)
        {
            if (opponent.puckCount > 3) // first two shots use block
            {
                BlockPowerup();
            }
            else // last three, use plus one or bolt
            {
                // if the ratio of player pucks to opponent pucks is greater than 2, use bolt
                var allPucks = GameObject.FindGameObjectsWithTag("puck");
                float playerPucks = 0;
                float opponentPucks = 0.001f; // so we don't divide by zero
                foreach (var puck in allPucks)
                {
                    var puckScript = puck.GetComponent<PuckScript>();
                    if (puckScript.IsPlayersPuck() && puckScript.ComputeValue() > 0)
                    {
                        playerPucks++;
                    }
                    else if (!puckScript.IsPlayersPuck() && puckScript.ComputeValue() > 0)
                    {
                        opponentPucks++;
                    }
                }
                if (playerPucks / opponentPucks > 2)
                {
                    BoltPowerup();
                }
                // otherwise, use plus one
                else
                {
                    PlusOnePowerup();
                }
            }
        }
    }

    private void OpponentShootingHelper()
    {
        // timestamp the beginning of CPU's turn for delays
        if (tempTime == 0)
        {
            tempTime = timer;
        }

        // after 1.5 seconds elapsed, CPU selects angle
        if (Mathf.Abs(line.GetValue() - CPUShotAngle) < (timer - (tempTime + 1.5)) && activeBar == "angle")
        {
            activeBar = bar.ChangeBar("power", activeCompetitor.isPlayer);
        }
        // after 3 seconds elapsed, CPU selects power
        if (Mathf.Abs(line.GetValue() - CPUShotPower) < (timer - (tempTime + 3)) && activeBar == "power")
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
        if (Mathf.Abs(line.GetValue() - CPUShotSpin) < (timer - (tempTime + 4.5)) && activeBar == "spin")
        {
            Shoot(CPUShotAngle, CPUShotPower, CPUShotSpin);
            tempTime = 0;
        }
    }

    private void Shoot(float angle, float power, float spin = 50.0f)
    {
        if (forcefieldScript.IsPlayers() != activeCompetitor.isPlayer)
        {
            forcefieldScript.DisableForcefield();
        }
        Debug.Log("Shooting: " + angle + " | " + power + " | " + spin);
        activeBar = bar.ChangeBar("none");
        line.isActive = false;
        arrow.SetActive(false);
        activeCompetitor.ShootActivePuck(angle, power, spin);
        UI.PostShotUpdate(player.puckCount, opponent.puckCount);
        UI.UpdateShotDebugText(angle, power, spin);
        nonActiveCompetitor.isTurn = true;
        SwapCompetitors();
    }

    private void SwapCompetitors()
    {
        var temp = activeCompetitor;
        activeCompetitor = nonActiveCompetitor;
        nonActiveCompetitor = temp;
    }

    public void RestartGame(int diff)
    {
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
            wall.SetActive(false);
        }
        else
        {
            wallCount = 3;
            wall.SetActive(true);
        }
        UI.UpdateWallText(wallCount);

        UI.SetReButtons(true);
        gameIsRunning = true;
        puckHalo.SetActive(difficulty == 0);
        line.isActive = false;
        bar.ChangeBar("none");
        UI.ChangeUI(UI.gameHud);
        Debug.Log("Starting match with difficulty: " + difficulty);
    }

    public bool LeftsTurn()
    {
        return (((player.isTurn || player.isShooting) && player.goingFirst) || ((opponent.isTurn || opponent.isShooting) && !player.goingFirst));
    }

    // update the scores
    public void UpdateScores()
    {
        (player.score, opponent.score) = puckManager.UpdateScores();
        UI.UpdateScores(player.score, opponent.score);
    }

    // back button in game
    public void ReturnToMenu()
    {
        gameIsRunning = false;
        player.puckCount = 0;
        opponent.puckCount = 0;
        UI.ChangeUI(UI.titleScreen);
        line.isActive = false;
        bar.ChangeBar("none");
        arrow.SetActive(false);
    }

    // this is the CPU AI for hard mode
    private (float, float, float) FindOpenPath()
    {
        CPUPathScript best = null;
        int highestValue = 0;
        // check all paths to see which are unblocked
        foreach (var path in CPUPaths)
        {
            var pathi = path.GetComponent<CPUPathScript>();
            pathi.DisablePathVisualization();
            var pathiShotValue = pathi.CalculateValue();
            if (pathiShotValue > highestValue)
            {
                // handle phase powerup shots
                if (!powerupsAreEnabled && pathi.DoesPathRequirePhasePowerup())
                {
                    continue;
                }

                best = pathi;
                highestValue = pathiShotValue;
            }
        }
        // chose the highest-value unblocked path
        if (highestValue > 0)
        {
            Debug.Log("Found path with highest value " + highestValue);
            best.EnablePathVisualization();

            // handle phase powerup shots
            if (best.DoesPathRequirePhasePowerup())
            {
                PhasePowerup();
            }

            return best.GetPath();
        }
        // otherwise, Shoot random
        else
        {
            Debug.Log("No path :(");
            return (Random.Range(35.0f, 65.0f), Random.Range(40.0f, 70.0f), Random.Range(45.0f, 55.0f));
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // power up stuff, eventually should be put into it's own script
    [SerializeField] private Button powerupButton1;
    [SerializeField] private Button powerupButton2;
    [SerializeField] private Button powerupButton3;

    [SerializeField] private Sprite plusOneImage;
    [SerializeField] private Sprite foresightImage;
    [SerializeField] private Sprite blockImage;
    [SerializeField] private Sprite boltImage;
    [SerializeField] private Sprite forceFieldImage;
    [SerializeField] private Sprite phaseImage;

    public void SetPowerups(bool value)
    {
        powerupsAreEnabled = value;
    }

    private void ShufflePowerups()
    {
        Button[] powerupButtons = { powerupButton1, powerupButton2, powerupButton3 };
        Sprite[] powerupSprites = { plusOneImage, foresightImage, blockImage, boltImage, forceFieldImage, phaseImage };

        Action[] methodArray = new Action[]
        {
            PlusOnePowerup,
            ForesightPowerup,
            BlockPowerup,
            BoltPowerup,
            ForceFieldPowerup,
            PhasePowerup
        };
        // generate 3 unique random powerups
        int[] randomPowerups = new int[3];
        for (int i = 0; i < 3; i++)
        {
            int randomPowerup = Random.Range(0, methodArray.Length);
            while (Array.Exists(randomPowerups, element => element == randomPowerup))
            {
                randomPowerup = Random.Range(0, methodArray.Length);
            }
            randomPowerups[i] = randomPowerup;
            powerupButtons[i].image.sprite = powerupSprites[randomPowerup];
            powerupButtons[i].onClick.RemoveAllListeners();
            powerupButtons[i].onClick.AddListener(() => methodArray[randomPowerup]());
            // add disable powerupmenu object function as listener
            powerupButtons[i].onClick.AddListener(() => powerupsMenu.SetActive(false));
        }
    }

    public void PlusOnePowerup() // give active puck +1 value
    {
        activeCompetitor.activePuckScript.SetPuckBonusValue(1);
        activeCompetitor.activePuckScript.SetPowerupText("plus one");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        powerupHasBeenUsedThisTurn = true;
    }

    public void ForesightPowerup() // enable the shot predicted location halo
    {
        puckHalo.SetActive(true);
        activeCompetitor.activePuckScript.SetPowerupText("foresight");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        powerupHasBeenUsedThisTurn = true;
    }

    public void BlockPowerup() // create a valueless blocking puck
    {
        int swap = activeCompetitor.isPlayer ? 1 : -1;
        GameObject blockPuckObject = Instantiate(puckPrefab, new Vector3(Random.Range(2f * swap, 4f * swap), Random.Range(2f, 4f), -1.0f), Quaternion.identity);
        PuckScript blockPuckScript = blockPuckObject.GetComponent<PuckScript>();
        blockPuckScript.InitPuck(activeCompetitor.isPlayer, activeCompetitor.puckSpriteID);
        blockPuckScript.SetPuckBaseValue(0);
        blockPuckScript.SetPowerupText("valueless");
        blockPuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.SetPowerupText("block");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        powerupHasBeenUsedThisTurn = true;
    }

    private PuckScript pucki;
    public void BoltPowerup() // destroy a random puck with value greater than or equal to 1
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        int randomPuckIndex = Random.Range(0, allPucks.Length);
        pucki = allPucks[randomPuckIndex].GetComponent<PuckScript>();
        var iterationLimit = 0;
        while (pucki.ComputeValue() == 0 && iterationLimit < 1000)
        {
            randomPuckIndex = Random.Range(0, allPucks.Length);
            pucki = allPucks[randomPuckIndex].GetComponent<PuckScript>();
            iterationLimit++;
        }
        if (pucki != null && pucki.ComputeValue() > 0)
        {
            pucki.DestroyPuck();
        }
        activeCompetitor.activePuckScript.SetPowerupText("bolt");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        powerupHasBeenUsedThisTurn = true;
    }

    [SerializeField] private ForcefieldScript forcefieldScript;
    public void ForceFieldPowerup()
    {
        activeCompetitor.activePuckScript.SetPowerupText("force field");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        forcefieldScript.EnableForcefield(activeCompetitor.isPlayer);
        powerupHasBeenUsedThisTurn = true;
    }

    public void PhasePowerup()
    {
        activeCompetitor.activePuckScript.SetPowerupText("phase");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.SetPhase(true);
        powerupHasBeenUsedThisTurn = true;
    }

    [SerializeField] private AudioSource puckDestroySFX;
    public void playDestroyPuckSFX(float SFXvolume)
    {
        puckDestroySFX.volume = SFXvolume;
        puckDestroySFX.Play();
    }
}
