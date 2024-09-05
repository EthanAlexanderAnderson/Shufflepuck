/* Logic Script is kinda like the "main" function for playing vs CPU, 
 * it controls most things happening in game and directs other scripts.
 */

using UnityEngine;

public class LogicScript : MonoBehaviour
{
    UIManagerScript UI;

    // prefabs
    public GameObject puckPrefab; // default puck prefab

    private PuckManager puckManager;
    private PuckSkinManager puckSkinManager;
    private SoundManagerScript soundManager;

    [SerializeField] GameObject puckHalo;

    // bar and line
    BarScript bar;
    LineScript line;
    public string activeBar = "none";
    float angle;
    float power;
    float spin;

    // wall
    int wallCount = 3;
    [SerializeField] private GameObject wall;

    // powerups
    public bool powerupsAreEnabled = false;
    public GameObject powerupsMenu;

    // game state
    bool gameIsRunning;
    float timer = 0;
    int difficulty; // 0 easy 1 medium 2 hard
    public bool goingFirst;

    //public bool isLocal;
    bool isLocal;
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

    // Start is called before the first frame update
    void Start()
    {
#if (UNITY_EDITOR)
        //PlayerPrefs.SetInt("easyHighscore", 8);
        //PlayerPrefs.SetInt("mediumHighscore", 6);
        //PlayerPrefs.SetInt("hardHighscore", 4);
#endif

        // connect scripts
        bar = GameObject.FindGameObjectWithTag("bar").GetComponent<BarScript>();
        line = GameObject.FindGameObjectWithTag("line").GetComponent<LineScript>();
        UI = GameObject.FindGameObjectWithTag("ui").GetComponent<UIManagerScript>();
        puckManager = PuckManager.Instance;
        puckSkinManager = PuckSkinManager.Instance;
        soundManager = SoundManagerScript.Instance;

        // create player objects
        player = new Competitor { isPlayer = true };
        opponent = new Competitor { isPlayer = false };
        var targetFPS = PlayerPrefs.GetInt("FPS", 90);
        Application.targetFrameRate = targetFPS;
        //Debug.Log("Target FPS: " + Application.targetFrameRate);

        // load play prefs data
        UI.UpdateProfileText();
        UI.UpdateLocks();
        // load puck id, but not dev skin
        PuckSkinManager.Instance.SelectPlayerPuckSprite(PlayerPrefs.GetInt("puck") == 0 && PlayerPrefs.GetInt("hardHighscore") <= 5 ? 1 : PlayerPrefs.GetInt("puck"));
        activeCompetitor = opponent;
        nonActiveCompetitor = player;
    }

    // Update is called once per frame
    void Update()
    {
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
            if (player.isTurn && !player.isShooting && ((goingFirst && opponent.puckCount == 5) || opponent.activePuckScript.IsSlowed()))
            {
                if (difficulty >= 2 && !isLocal)
                {
                    powerupsMenu.SetActive(powerupsAreEnabled);
                }
                puckHalo.SetActive(difficulty == 0);
                activeBar = bar.ChangeBar("angle", LeftsTurn());
                bar.toggleDim(false);
                line.isActive = true;
                UI.TurnText = isLocal ? "Player 1's Turn" : "Your Turn";
                if (player.puckCount == 1) {
                    UI.TurnText = "LAST PUCK";
                }
                CreatePuck(true);
                player.isTurn = false;
                player.isShooting = true;
                //goingFirst = false;
                if (wallCount > 0)
                {
                    wallCount--;
                    if (wallCount > 0) { UI.UpdateWallText(wallCount); }
                }
            }

            // now player may shoot
            if ((Input.GetMouseButtonDown(0)) && gameIsRunning && (player.isShooting || isLocal) && powerupsMenu.activeInHierarchy == false)
            {
                soundManager.PlayClickSFX();
                // change state on tap depending on current state
                switch (activeBar)
                {
                    case "angle":
                        angle = line.value;
                        activeBar = bar.ChangeBar("power", LeftsTurn());
                        break;
                    case "power":
                        power = line.value;
                        // if non-hard diff, end turn
                        if (difficulty < 2)
                        {
                            Shoot(angle, power);
                            UI.UpdateShotDebugText(angle, power);
                        }
                        // on hard diff, show spin bar
                        else
                        {
                            activeBar = bar.ChangeBar("spin", LeftsTurn());
                        }
                        break;
                    // if hard, select spin
                    case "spin":
                        spin = line.value;
                        Shoot(angle, power, spin);
                        UI.UpdateShotDebugText(angle, power, spin);
                        break;
                }
            }

            // start CPU's turn, do this then start shooting
            if (opponent.isTurn && (player.activePuckScript == null || (player.activePuckScript.IsSlowed() && (puckManager.AllPucksAreSlowed() && difficulty < 2 || puckManager.AllPucksAreSlowedMore()))))
            {
                activeBar = bar.ChangeBar("angle", LeftsTurn());
                if (!isLocal)
                {
                    bar.toggleDim(true);
                }
                line.isActive = true;
                UI.TurnText = isLocal ? "Player 2's Turn":"CPU's Turn";
                if (opponent.puckCount == 1 && isLocal) {
                    UI.TurnText = "LAST PUCK";
                }
                CreatePuck(false);
                opponent.isTurn = false;
                opponent.isShooting = true;

                if (wallCount > 0) {
                    wallCount--;
                    if (wallCount > 0) { UI.UpdateWallText(wallCount); }
                }
                // first turn on med-hard diff, CPU Shoots perfect
                if (difficulty == 1 && opponent.puckCount == 5)
                {
                    CPUShotAngle = goingFirst ? Random.Range(33.0f, 38.0f) : Random.Range(62.0f, 67.0f);
                    CPUShotPower = Random.Range(65.0f, 78.0f);
                }
                // easy-med regular shots only
                else if (difficulty < 2)
                {
                    CPUShotAngle = goingFirst ? Random.Range(11.0f + (difficulty * 5.0f), 63.0f - (difficulty * 5.0f)) : Random.Range(37.0f + (difficulty * 5.0f), 89.0f - (difficulty * 5.0f));
                    CPUShotPower = Random.Range(30.0f + (difficulty * 5.0f), 90.0f - (difficulty * 5.0f));
                }
                // hard uses paths, and random if no path is found
                else
                {
                    (CPUShotAngle, CPUShotPower, CPUShotSpin) = FindOpenPath();
                }
            }
            // now opponent may shoot
            if (opponent.isShooting && !isLocal)
            {
                // timestamp the beginning of CPU's turn for delays
                if (tempTime == 0)
                {
                    tempTime = timer;
                }

                // after 1.5 seconds elapsed, CPU selects angle
                if (Mathf.Abs(line.value - CPUShotAngle) < (timer - (tempTime + 1.5)) && activeBar == "angle")
                {
                    activeBar = bar.ChangeBar("power", LeftsTurn());
                }
                // after 3 seconds elapsed, CPU selects power
                if (Mathf.Abs(line.value - CPUShotPower) < (timer - (tempTime + 3)) && activeBar == "power")
                {
                    if (difficulty < 2)
                    {
                        Shoot(CPUShotAngle, CPUShotPower);
                        tempTime = 0;
                    }
                    else
                    {
                        activeBar = bar.ChangeBar("spin", LeftsTurn());
                    }
                }
                // after 4.5 seconds elapsed, CPU selects spin (for hard mode only)
                if (Mathf.Abs(line.value - CPUShotSpin) < (timer - (tempTime + 4.5)) && activeBar == "spin")
                {
                    Shoot(CPUShotAngle, CPUShotPower, CPUShotSpin);
                    tempTime = 0;
                }
            }

            // lastly, increment timer while game is running
            timer += Time.deltaTime;
        }
        // ran out of pucks (game over)
        else if (gameIsRunning && (player.puckCount <= 0 || opponent.puckCount <= 0) && puckManager.AllPucksAreStopped())
        {
            gameIsRunning = false;
            UpdateScores();
            UI.ChangeUI(UI.gameResultScreen);
            UI.UpdateGameResult(player.score, opponent.score, difficulty, isLocal);
            isLocal = false;
        }
    }

    private void Shoot(float angle, float power, float spin = 50.0f)
    {
        Debug.Log("Shooting: " + angle + " | " + power + " | " + spin);
        activeBar = bar.ChangeBar("none");
        line.isActive = false;
        activeCompetitor.activePuckScript.Shoot(angle, power, spin);
        activeCompetitor.isShooting = false;
        activeCompetitor.puckCount--;
        UI.PostShotUpdate(player.puckCount, opponent.puckCount);
        nonActiveCompetitor.isTurn = true;
        SwapCompetitors();
        Debug.Log("Passing turn to: " + (activeCompetitor.isPlayer == true ? "player" : "opponent"));
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
        UI.titleScreenBackground.SetActive(false);
        // reset game variables
        puckSkinManager.RandomizeCPUPuckSprite();
        // if diff is -1, that means this function is called from the rematch button, so don't change the difficulty
        if (diff >= 0)
        {
            difficulty = diff;
        }
        player.score = 0;
        opponent.score = 0;
        player.puckCount = 5;
        opponent.puckCount = 5;

        if (difficulty < 2) // for easy & medium
        {
            player.isTurn = false;
            player.isShooting = false;
            opponent.isTurn = true;
            opponent.isShooting = false;
            activeCompetitor = opponent;
            nonActiveCompetitor = player;
            goingFirst = false;
        }
        else // for hard
        {
            player.isTurn = true;
            player.isShooting = false;
            opponent.isTurn = false;
            opponent.isShooting = false;
            activeCompetitor = player;
            nonActiveCompetitor = opponent;
            goingFirst = true;
        }

        UI.SetReButtons(true);
        gameIsRunning = true;
        puckHalo.SetActive(difficulty == 0);
        line.isActive = false;
        bar.ChangeBar("none");
        UI.ChangeUI(UI.gameHud);
        wall.SetActive(true);
        wallCount = 3;
        Debug.Log("Starting match with difficulty: " + difficulty);
    }

    // create a puck. bool parameter of if its the player's puck or not so we can set the sprite
    // TODO : move to puckmanager script
    public void CreatePuck(bool isPlayersPuck)
    {
        if (isPlayersPuck)
        {
            player.activePuckObject = Instantiate(puckPrefab, new Vector3((goingFirst ? -3.6f : 3.6f), -10.0f, 0.0f), Quaternion.identity);
            player.activePuckScript = player.activePuckObject.GetComponent<PuckScript>();
            player.activePuckScript.InitPuck(true, player.puckSprite);
        }
        else
        {
            opponent.activePuckObject = Instantiate(puckPrefab, new Vector3((goingFirst ? 3.6f : -3.6f), -10.0f, 0.0f), Quaternion.identity);
            opponent.activePuckScript = opponent.activePuckObject.GetComponent<PuckScript>();
            opponent.activePuckScript.InitPuck(false, opponent.puckSprite);
        }
    }

    public bool LeftsTurn()
    {
        return (((player.isTurn || player.isShooting) && goingFirst) || ((opponent.isTurn || opponent.isShooting) && !goingFirst));
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
        UI.titleScreenBackground.SetActive(true);
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
            return (Random.Range(35.0f, 65.0f), Random.Range(40.0f, 70.0f), Random.Range(40.0f, 50.0f));
        }
    }

    // controller for SFX mute
    public float GetSFX()
    {
        return soundManager.GetSFXVolume();
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
        blockPuckScript.InitPuck(true, player.puckSprite);
    }
}
