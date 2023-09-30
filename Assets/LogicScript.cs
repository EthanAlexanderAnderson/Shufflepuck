/* Logic Script is kinda like the "main" function, 
 * it controls most things happening in game and directs
 * other scripts.
 */

using UnityEngine;
using UnityEngine.UI;

public class LogicScript : MonoBehaviour
{
    private UIManagerScript UI;

    // pucks
    [HideInInspector] public GameObject puck; // default puck prefab

    [HideInInspector] private GameObject activePlayerPuckObject;
    [HideInInspector] private GameObject activeCPUPuckObject;

    [HideInInspector] private PuckScript activePlayerPuckScript;
    [HideInInspector] private PuckScript activeCPUPuckScript;

    // sprites
    [HideInInspector] public Sprite puckBlue;
    [HideInInspector] public Sprite puckGreen;
    [HideInInspector] public Sprite puckGrey;
    [HideInInspector] public Sprite puckOrange;
    [HideInInspector] public Sprite puckPink;
    [HideInInspector] public Sprite puckPurple;
    [HideInInspector] public Sprite puckRed;
    [HideInInspector] public Sprite puckYellow;

    [HideInInspector] public Sprite puckRainbow;
    [HideInInspector] public Sprite puckCanada;
    [HideInInspector] public Sprite puckDonut;
    [HideInInspector] public Sprite puckCaptain;

    public Sprite playerPuckSprite;
    public Sprite CPUPuckSprite;

    // table
    [HideInInspector] public GameObject table;

    public GameObject puckHalo;

    // bar and line
    private BarScript bar;
    private LineScript line;
    public string activeBar = "none";
    public float angle;
    public float power;
    public float spin;

    // scoring
    public int playerScore;
    public int opponentScore;

    public int playerPuckCount;
    public int opponentPuckCount;

    // game state
    public bool isPlayersTurn;
    public bool isPlayerShooting;
    public bool isCPUShooting;
    public bool gameIsRunning = false;
    public float timer = 0;
    public int difficulty; // 0 easy 1 medium 2 hard

    // temp variables
    private float tempTime = 0;
    public float CPUShotAngle = 0;
    public float CPUShotPower = 0;
    public float CPUShotSpin = 50;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        // connect scripts
        bar = GameObject.FindGameObjectWithTag("bar").GetComponent<BarScript>();
        line = GameObject.FindGameObjectWithTag("line").GetComponent<LineScript>();
        UI = GameObject.FindGameObjectWithTag("ui").GetComponent<UIManagerScript>();
        // load play prefs data
        UI.UpdateProfileText();
        UI.UpdateLocks();
        SelectPlayerPuckSprite(PlayerPrefs.GetInt("puck"));
    }

    // Update is called once per frame
    void Update()
    {
        // only do this stuff when game is running (not in menu etc.)
        if (gameIsRunning && (playerPuckCount > 0 || opponentPuckCount > 0))
        {
            // clear pucks in non-safe zones
            CleanupDeadPucks();

            // if input
            if ((Input.GetMouseButtonDown(0)) && gameIsRunning && isPlayerShooting)
            {
                // change state on tap depending on current state
                switch (activeBar)
                {
                    case "angle":
                        angle = line.value;
                        activeBar = bar.ChangeBar("power");
                        break;
                    case "power":
                        power = line.value;
                        // if non-hard diff, end turn
                        if (difficulty < 2)
                        {
                            activeBar = bar.ChangeBar("none");
                            activePlayerPuckScript.Shoot(angle, power);
                            isPlayersTurn = false;
                            isPlayerShooting = false;
                            playerPuckCount--;
                            UI.PostShotUpdate(playerPuckCount, opponentPuckCount);
                            isCPUShooting = true;
                        }
                        // on hard diff, show spin bar
                        else
                        {
                            activeBar = bar.ChangeBar("spin");
                        }
                        break;
                    // if hard, select spin
                    case "spin":
                        spin = line.value;
                        activeBar = bar.ChangeBar("none");
                        activePlayerPuckScript.Shoot(angle, power, spin);
                        isPlayersTurn = false;
                        isPlayerShooting = false;
                        playerPuckCount--;
                        UI.PostShotUpdate(playerPuckCount, opponentPuckCount);
                        isCPUShooting = true;
                        break;
                }
            }

            // start Players turn
            if (isPlayersTurn && activeCPUPuckScript.IsSlowed())
            {
                UI.TurnText = "Your Turn";
                CreatePuck(true);
                activeBar = bar.ChangeBar("angle");
                isPlayersTurn = false;
                isPlayerShooting = true;
            }

            // start CPU's turn
            if (!isPlayersTurn && (activePlayerPuckScript == null || (activePlayerPuckScript.IsSlowed() && AllPucksAreSlowed())))
            {
                // timestamp the beginning of CPU's turn for delays
                if (tempTime == 0)
                {
                    tempTime = timer;
                }

                // do this stuff immediately, once
                if (isCPUShooting)
                {
                    activeBar = bar.ChangeBar("angle");
                    UI.TurnText = "Your Turn";
                    CreatePuck(false);
                    isCPUShooting = false;
                    // first turn on med-hard diff, CPU Shoots perfect
                    if (difficulty == 1 && opponentPuckCount == 5)
                    {
                        CPUShotAngle = Random.Range(48.0f, 52.0f);
                        CPUShotPower = Random.Range(65.0f, 78.0f);
                    }
                    // easy-med regular shots only
                    else if (difficulty < 2)
                    {
                        CPUShotAngle = Random.Range(20.0f + (difficulty * 5.0f), 80.0f - (difficulty * 5.0f));
                        CPUShotPower = Random.Range(30.0f + (difficulty * 5.0f), 90.0f - (difficulty * 5.0f));
                    }
                    // hard uses paths, and random if no path is found
                    else
                    {
                        Debug.Log("searching for path...");
                        (CPUShotAngle, CPUShotPower, CPUShotSpin) = FindOpenPath();
                    }

                }
                // after 1.5 seconds elapsed, CPU selects angle
                if (tempTime + 1.5 < timer && Mathf.Abs(line.value - CPUShotAngle) < (1.0f + difficulty) && activeBar == "angle")
                {
                    activeBar = bar.ChangeBar("power");
                }
                // after 3 seconds elapsed, CPU selects power
                if (tempTime + 3 < timer && Mathf.Abs(line.value - CPUShotPower) < (1.0f + difficulty) && activeBar == "power")
                {
                    if (difficulty < 2)
                    {
                        activeBar = bar.ChangeBar("none");
                        activeCPUPuckScript.Shoot(CPUShotAngle, CPUShotPower);
                        isPlayersTurn = true;
                        tempTime = 0;
                        opponentPuckCount--;
                        UI.PostShotUpdate(playerPuckCount, opponentPuckCount);
                    }
                    else
                    {
                        activeBar = bar.ChangeBar("spin");
                    }
                }
                // after 4.5 seconds elapsed, CPU selects spin (for hard mode only)
                if (tempTime + 4.5 < timer && Mathf.Abs(line.value - CPUShotSpin) < (1.0f + difficulty) && activeBar == "spin")
                {
                    activeBar = bar.ChangeBar("none");
                    activeCPUPuckScript.Shoot(CPUShotAngle, CPUShotPower, CPUShotSpin);
                    isPlayersTurn = true;
                    tempTime = 0;
                    opponentPuckCount--;
                    UI.PostShotUpdate(playerPuckCount, opponentPuckCount);
                }
            }

            // lastly, increment timer while game is running
            timer += Time.deltaTime;
        }
        // ran out of pucks (game over)
        else if (gameIsRunning && (playerPuckCount <= 0 || opponentPuckCount <= 0) && AllPucksAreStopped())
        {
            gameIsRunning = false;
            UpdateScores();
            UI.ChangeUI(UI.gameResultScreen);
            UI.UpdateGameResult(playerScore, opponentScore);
            UI.OverwriteHighscore(playerScore - opponentScore, difficulty); // move this call to within UI.UpdateGameResult maybe ?
        }
    }

    public void RestartGame(int diff)
    {
        // organize scene
        ClearAllPucks();
        table.GetComponent<TableScript>().ShowBoard();
        // reset game variables
        RandomizeCPUPuckSprite();
        difficulty = diff;
        playerScore = 0;
        opponentScore = 0;
        playerPuckCount = 5;
        opponentPuckCount = 5;
        isPlayersTurn = false;
        isPlayerShooting = false;
        isCPUShooting = true;
        gameIsRunning = true;
        // reset UI text
        puckHalo.SetActive(diff == 0);
        UI.ChangeUI(UI.gameHud);
    }

    // create a puck. bool parameter of if its the player's puck or not so we can set the sprite
    public void CreatePuck(bool IsPlayersPuck)
    {
        if (IsPlayersPuck)
        {
            activePlayerPuckObject = Instantiate(puck, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
            activePlayerPuckScript = activePlayerPuckObject.GetComponent<PuckScript>();
            activePlayerPuckScript.InitPuck(true, playerPuckSprite);
        }
        else
        {
            activeCPUPuckObject = Instantiate(puck, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
            activeCPUPuckScript = activeCPUPuckObject.GetComponent<PuckScript>();
            activeCPUPuckScript.InitPuck(false, CPUPuckSprite);
        }
    }

    // useful helper function for deciding when to allow actions. Returns true when all pucks have stopped moving
    private PuckScript pucki;
    public bool AllPucksAreStopped()
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            pucki = puck.GetComponent<PuckScript>();
            // IF shot, but not stopped, return false
            if (pucki.IsShot() && !pucki.IsStopped())
            {
                return false;
            }
        }
        // IF all shot puts are stopped, return true
        return true;
    }

    // useful helper function for deciding when to allow actions. Returns true when all pucks have slowed down
    public bool AllPucksAreSlowed()
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            pucki = puck.GetComponent<PuckScript>();
            // IF shot, but not stopped, return false
            if (pucki.IsShot() && !pucki.IsSlowed())
            {
                return false;
            }
        }
        // IF all shot puts are stopped, return true
        return true;
    }

    // update the scores
    public void UpdateScores()
    {
        int playerSum = 0;
        int CPUSum = 0;
        var objects = GameObject.FindGameObjectsWithTag("puck");
        foreach (var obj in objects)
        {
            pucki = obj.GetComponent<PuckScript>();
            if (pucki.IsPlayersPuck())
            {
                playerSum += pucki.ComputeValue();
            }
            else
            {
                CPUSum += pucki.ComputeValue();
            }  
        }
        playerScore = playerSum;
        opponentScore = CPUSum;
        UI.UpdateScores(playerScore, opponentScore);
    }

    // destroy pucks out of bounds
    private void CleanupDeadPucks()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("puck"))
        {
            pucki = obj.GetComponent<PuckScript>();
            if (!pucki.IsSafe() && pucki.IsStopped())
            {
                Destroy(obj);
            }
        }
    }

    // destroy ALL pucks
    private void ClearAllPucks()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("puck"))
        {
            Destroy(obj);
        }
    }

    // helper with the puck customization buttons
    private Sprite ColorIDtoPuckSprite(int id)
    {
        Sprite[] puckSprites = { puckBlue, puckGreen, puckGrey, puckOrange, puckPink, puckPurple, puckRed, puckYellow, puckRainbow, puckCanada, puckDonut, puckCaptain };
        return (puckSprites[id]);
    }

    // helper with the puck customization buttons
    public void SelectPlayerPuckSprite(int id)
    {
        playerPuckSprite = ColorIDtoPuckSprite(id);
        UI.SetPlayerPuckIcon(playerPuckSprite);
        PlayerPrefs.SetInt("puck", id);
        RandomizeCPUPuckSprite();
    }

    // helper for randomize puck button
    public void RandomizePlayerPuckSprite()
    {
        var prev = playerPuckSprite;
        int rng;
        do
        {
            rng = Random.Range(0, 8);
            playerPuckSprite = ColorIDtoPuckSprite(rng);
        } while (prev == playerPuckSprite);
        UI.SetPlayerPuckIcon(playerPuckSprite);
        PlayerPrefs.SetInt("puck", rng);
        RandomizeCPUPuckSprite();
    }

    // randomize CPU puck. This is called before every match
    public void RandomizeCPUPuckSprite()
    {
        do
        {
            CPUPuckSprite = ColorIDtoPuckSprite(Random.Range(0, 8));
        } while (CPUPuckSprite == playerPuckSprite);
        UI.SetOpponentPuckIcon(CPUPuckSprite);
    }

    // back button after match is over
    public void ReturnToMenu()
    {
        gameIsRunning = false;
        playerPuckCount = 0;
        opponentPuckCount = 0;
        UI.ChangeUI(UI.titleScreen);
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
            if (pathi.GetPucksInPath().Count == 0)
            {
                Debug.Log("PATH FOUND. VALUE: " + pathi.value.ToString());
                if (pathi.value > highestValue)
                {
                    best = pathi;
                    highestValue = pathi.value;
                }
            }
        }
        // chose the highest-value unblocked path
        if (highestValue > 0)
        {
            return (best.angle, best.power, best.spin);
        }
        // otherwise, Shoot random
        else
        {
            Debug.Log("No path :(");
            return (Random.Range(35.0f, 65.0f), Random.Range(40.0f, 70.0f), Random.Range(40.0f, 50.0f));
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
