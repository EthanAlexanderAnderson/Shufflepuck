/* Logic Script is kinda like the "main" function for playing vs CPU, 
 * it controls most things happening in game and directs other scripts.
 */

using UnityEngine;

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

    // powerups
    public bool powerupsAreEnabled = false;
    [SerializeField] private GameObject powerupsMenu; // set in editor

    // game state
    public bool gameIsRunning { get; private set; }
    private float timer = 0;
    private int difficulty; // 0 easy 1 medium 2 hard

    //public bool isLocal;
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
                PlayerShootingHelper();
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
        GameObject[] CPUPaths = GameObject.FindGameObjectsWithTag("cpu_path");
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
                best = pathi;
                highestValue = pathiShotValue;
            }
        }
        // chose the highest-value unblocked path
        if (highestValue > 0)
        {
            Debug.Log("Found path with highest value " + highestValue);
            best.EnablePathVisualization();
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

    public void SetPowerups(bool value)
    {
        powerupsAreEnabled = value;
    }

    public void PlusOnePowerup()
    {
        player.activePuckScript.SetPuckBonusValue(1);
    }

    public void ForesightPowerup()
    {
        puckHalo.SetActive(true);
    }

    public void BlockPowerup()
    {
        GameObject blockPuckObject = Instantiate(puckPrefab, new Vector3(Random.Range(2f, 4f), Random.Range(2f, 4f), 0.0f), Quaternion.identity);
        PuckScript blockPuckScript = blockPuckObject.GetComponent<PuckScript>();
        blockPuckScript.InitPuck(true, player.puckSpriteID);
    }
}
