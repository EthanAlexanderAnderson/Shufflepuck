/* Logic Script is kinda like the "main" function, 
 * it controls most things happening in game and directs
 * other scripts.
 */

using UnityEngine;

public class LogicScript : MonoBehaviour
{
    private UIManagerScript UI;
    private ServerLogicScript serverLogic;

    // prefabs
    public GameObject puckPrefab; // default puck prefab
    //public GameObject competitorPrefab;
    //public GameObject CPUPrefab;

    // sprites
    [SerializeField] private Sprite puckBlue;
    [SerializeField] private Sprite puckGreen;
    [SerializeField] private Sprite puckGrey;
    [SerializeField] private Sprite puckOrange;
    [SerializeField] private Sprite puckPink;
    [SerializeField] private Sprite puckPurple;
    [SerializeField] private Sprite puckRed;
    [SerializeField] private Sprite puckYellow;

    [SerializeField] private Sprite puckRainbow;
    [SerializeField] private Sprite puckCanada;
    [SerializeField] private Sprite puckDonut;
    [SerializeField] private Sprite puckCaptain;

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

    // game state
    public bool gameIsRunning;
    public float timer = 0;
    public int difficulty; // 0 easy 1 medium 2 hard
    public bool goingFirst;

    //public bool isLocal;
    public bool isLocal;
    public bool IsLocal
    {
        get => isLocal;
        set
        {
            isLocal = value;
        }
    }

    public bool isOnline;
    public bool IsOnline
    {
        get => isOnline;
        set
        {
            isOnline = value;
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

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetInt("hardHighscore", 0);
        // create player objects
        //player = Instantiate(competitorPrefab).GetComponent<Competitor>();
        player = new Competitor();
        player.isPlayer = true;
        //opponent = Instantiate(competitorPrefab).GetComponent<Competitor>();
        opponent = new Competitor();
        opponent.isPlayer = false;
        Application.targetFrameRate = 30;
        // connect scripts
        bar = GameObject.FindGameObjectWithTag("bar").GetComponent<BarScript>();
        line = GameObject.FindGameObjectWithTag("line").GetComponent<LineScript>();
        UI = GameObject.FindGameObjectWithTag("ui").GetComponent<UIManagerScript>();
        serverLogic = GameObject.FindGameObjectWithTag("logic").GetComponent<ServerLogicScript>();
        // load play prefs data
        UI.UpdateProfileText();
        UI.UpdateLocks();
        SelectPlayerPuckSprite(PlayerPrefs.GetInt("puck"));
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
            CleanupDeadPucks();

            // start Players turn, do this then start shooting
            if (player.isTurn && !player.isShooting && (goingFirst || opponent.activePuckScript.IsSlowed()))
            {
                activeBar = bar.ChangeBar("angle");
                line.isActive = true;
                UI.SetTurnTextActive();
                UI.TurnText = "Your Turn";
                CreatePuck(true);
                player.isTurn = false;
                player.isShooting = true;
            }

            // now player may shoot
            if ((Input.GetMouseButtonDown(0)) && gameIsRunning && (player.isShooting || isLocal))
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
                        spin = line.value;
                        Shoot(angle, power, spin);
                        break;
                }
            }

            // start CPU's turn, do this then start shooting
            if (opponent.isTurn && (player.activePuckScript == null || (player.activePuckScript.IsSlowed() && AllPucksAreSlowed())))
            {
                activeBar = bar.ChangeBar("angle");
                line.isActive = true;
                UI.SetTurnTextActive();
                UI.TurnText = isLocal ? "Opponent's Turn":"CPU's Turn";
                CreatePuck(false);
                opponent.isTurn = false;
                opponent.isShooting = true;
                // first turn on med-hard diff, CPU Shoots perfect
                if (difficulty == 1 && opponent.puckCount == 5)
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
            // now opponent may shoot
            if (opponent.isShooting && !isLocal)
            {
                // timestamp the beginning of CPU's turn for delays
                if (tempTime == 0)
                {
                    tempTime = timer;
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
                        Shoot(CPUShotAngle, CPUShotPower);
                        tempTime = 0;
                    }
                    else
                    {
                        activeBar = bar.ChangeBar("spin");
                    }
                }
                // after 4.5 seconds elapsed, CPU selects spin (for hard mode only)
                if (tempTime + 4.5 < timer && Mathf.Abs(line.value - CPUShotSpin) < (1.0f + difficulty) && activeBar == "spin")
                {
                    Shoot(CPUShotAngle, CPUShotPower, CPUShotSpin);
                    tempTime = 0;
                }
            }

            // lastly, increment timer while game is running
            timer += Time.deltaTime;
        }
        // ran out of pucks (game over)
        else if (gameIsRunning && (player.puckCount <= 0 || opponent.puckCount <= 0) && AllPucksAreStopped())
        {
            gameIsRunning = false;
            isLocal = false;
            UpdateScores();
            UI.ChangeUI(UI.gameResultScreen);
            UI.UpdateGameResult(player.score, opponent.score, difficulty);
        }
    }

    private void Shoot(float angle, float power, float spin = 50.0f)
    {
        activeBar = bar.ChangeBar("none");
        line.isActive = false;
        activeCompetitor.activePuckScript.Shoot(angle, power, spin);
        activeCompetitor.isShooting = false;
        activeCompetitor.puckCount--;
        UI.PostShotUpdate(player.puckCount, opponent.puckCount);
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
        // organize scene
        ClearAllPucks();
        // reset game variables
        RandomizeCPUPuckSprite();
        difficulty = diff;
        player.score = 0;
        opponent.score = 0;
        player.puckCount = 5;
        opponent.puckCount = 5;
        player.isTurn = false;
        player.isShooting = false;
        opponent.isTurn = true;
        opponent.isShooting = false;
        gameIsRunning = true;
        // reset UI text
        puckHalo.SetActive(diff == 0);
        UI.ChangeUI(UI.gameHud);
    }

    public void RestartGameOnline()
    {
        // organize scene
        ClearAllPucks();
        // reset game variables
        RandomizeCPUPuckSprite();
        difficulty = 2;
        player.score = 0;
        opponent.score = 0;
        player.puckCount = 5;
        opponent.puckCount = 5;
        player.isTurn = false;
        player.isShooting = false;
        opponent.isTurn = false;
        opponent.isShooting = false;
        gameIsRunning = true;
        // reset UI text
        puckHalo.SetActive(false);
        UI.ChangeUI(UI.gameHud);
        UI.TurnText = "Opponent's Turn"; // this gets overwritten by StartTurn if going first
    }

    public void StartTurn()
    {
        goingFirst = true;
        player.isTurn = true;
    }

    // create a puck. bool parameter of if its the player's puck or not so we can set the sprite
    public void CreatePuck(bool isPlayersPuck)
    {
        if (isPlayersPuck)
        {
            player.activePuckObject = Instantiate(puckPrefab, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
            player.activePuckScript = player.activePuckObject.GetComponent<PuckScript>();
            player.activePuckScript.InitPuck(true, player.puckSprite);
        }
        else
        {
            opponent.activePuckObject = Instantiate(puckPrefab, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
            opponent.activePuckScript = opponent.activePuckObject.GetComponent<PuckScript>();
            opponent.activePuckScript.InitPuck(false, opponent.puckSprite);
        }
    }
    /*
    public void IntializePuck(bool IsPlayersPuck, GameObject activePuckObject)
    {
        Debug.Log("Calling IntializePuck");
        if (IsPlayersPuck)
        {
            player.activePuckObject = activePuckObject;
            player.activePuckScript = player.activePuckObject.GetComponent<PuckScript>();
            player.activePuckScript.InitPuck(true, player.puckSprite);
        }
        else
        {
            opponent.activePuckObject = activePuckObject;
            opponent.activePuckScript = opponent.activePuckObject.GetComponent<PuckScript>();
            opponent.activePuckScript.InitPuck(false, opponent.puckSprite);
        }
    }
    */
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
        int opponentSum = 0;
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
                opponentSum += pucki.ComputeValue();
            }  
        }
        player.score = playerSum;
        opponent.score = opponentSum;
        UI.UpdateScores(player.score, opponent.score);
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
    public void ClearAllPucks()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("puck"))
        {
            Destroy(obj);
        }
    }

    // helper with the puck customization buttons
    public Sprite ColorIDtoPuckSprite(int id)
    {
        Sprite[] puckSprites = { puckBlue, puckGreen, puckGrey, puckOrange, puckPink, puckPurple, puckRed, puckYellow, puckRainbow, puckCanada, puckDonut, puckCaptain };
        return (puckSprites[id]);
    }

// helper with the puck customization buttons
    public void SelectPlayerPuckSprite(int id)
    {
        player.puckSpriteID = id;
        player.puckSprite = ColorIDtoPuckSprite(id);
        UI.SetPlayerPuckIcon(player.puckSprite);
        PlayerPrefs.SetInt("puck", id);
        RandomizeCPUPuckSprite();
    }

    // helper for randomize puck button
    public void RandomizePlayerPuckSprite()
    {
        var prev = player.puckSprite;
        int rng;
        do
        {
            rng = Random.Range(0, 8);
            player.puckSpriteID = rng;
            player.puckSprite = ColorIDtoPuckSprite(rng);
        } while (prev == player.puckSprite);
        UI.SetPlayerPuckIcon(player.puckSprite);
        PlayerPrefs.SetInt("puck", rng);
        RandomizeCPUPuckSprite();
    }

    // randomize CPU puck. This is called before every match
    public void RandomizeCPUPuckSprite()
    {
        do
        {
            opponent.puckSprite = ColorIDtoPuckSprite(Random.Range(0, 8));
        } while (opponent.puckSprite == player.puckSprite);
        UI.SetOpponentPuckIcon(opponent.puckSprite);
    }

    // back button after match is over
    public void ReturnToMenu()
    {
        gameIsRunning = false;
        player.puckCount = 0;
        opponent.puckCount = 0;
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
