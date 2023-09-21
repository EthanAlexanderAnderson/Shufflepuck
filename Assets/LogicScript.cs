using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LogicScript : MonoBehaviour
{
    // UI
    [HideInInspector] public GameObject titleScreen;
    [HideInInspector] public GameObject gameResultScreen;
    [HideInInspector] public Text turnText;
    [HideInInspector] public Text gameResultText;

    [HideInInspector] public GameObject gameHud;

    [HideInInspector] public GameObject playerPuckIcon;
    [HideInInspector] public GameObject CPUPuckIcon;

    [HideInInspector] public Text playerPuckCountText;
    [HideInInspector] public Text CPUPuckCountText;

    [HideInInspector] public Text playerScoreText;
    [HideInInspector] public Text CPUScoreText;

    [HideInInspector] public GameObject playButton;

    [HideInInspector] public GameObject puckHalo;

    [HideInInspector] public TMP_Text errorMessage;

    // pop ups
    [HideInInspector] public GameObject customizePopup;
    [HideInInspector] public TMP_Text profilePopupText;

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

    // bar and line
    private BarScript bar;
    private LineScript line;
    public string activeBar = "none";
    public float angle;
    public float power;
    public float spin;

    // scoring
    public int playerScore;
    public int CPUScore;

    public int playerPuckCount;
    public int CPUPuckCount;

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
    private GameObject[] CPUPaths;

    // debug
    public bool debug = false;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        // connect scripts
        bar = GameObject.FindGameObjectWithTag("bar").GetComponent<BarScript>();
        line = GameObject.FindGameObjectWithTag("line").GetComponent<LineScript>();
        gameResultText.text = "";
        CPUPaths = GameObject.FindGameObjectsWithTag("cpu_path");
        updateProfileText();
        updateLocks();
    }

    // Update is called once per frame
    void Update()
    {
        // only do this stuff when game is running (not in menu etc.)
        if (gameIsRunning && (playerPuckCount > 0 || CPUPuckCount > 0))
        {
            //Debug.Log(allPucksAreStopped().ToString());
            // clear pucks in non-safe zones
            cleanupDeadPucks();

            // input
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && gameIsRunning && isPlayerShooting)
            {
                switch (activeBar)
                {
                    case "angle":
                        angle = line.value;
                        bar.changeBar("power");
                        activeBar = "power";
                        break;
                    case "power":
                        power = line.value;
                        // if non-hard diff, end turn
                        if (difficulty < 2)
                        {
                            bar.changeBar("none");
                            activeBar = "none";
                            activePlayerPuckScript.shoot(angle, power);
                            isPlayersTurn = false;
                            isPlayerShooting = false;
                            turnText.text = "";
                            playerPuckCount--;
                            playerPuckCountText.text = playerPuckCount.ToString();
                            isCPUShooting = true;
                        }
                        else
                        {
                            bar.changeBar("spin");
                            activeBar = "spin";
                        }
                        break;
                    // if hard, select spin
                    case "spin":
                        spin = line.value;
                        bar.changeBar("none");
                        activeBar = "none";
                        activePlayerPuckScript.shoot(angle, power, spin);
                        isPlayersTurn = false;
                        isPlayerShooting = false;
                        turnText.text = "";
                        playerPuckCount--;
                        playerPuckCountText.text = playerPuckCount.ToString();
                        isCPUShooting = true;
                        break;
                }
            }

            // Players turn
            if (isPlayersTurn && activeCPUPuckScript.isShot() && activeCPUPuckScript.isSlowed())
            {
                turnText.text = "Your Turn";
                createPuck(true);
                bar.changeBar("angle");
                activeBar = "angle";
                isPlayersTurn = false;
                isPlayerShooting = true;
            }

            // CPU's turn
            if (!isPlayersTurn && (activePlayerPuckScript == null || (activePlayerPuckScript.isSlowed() && allPucksAreSlowed())))
            {
                // timestamp the beginning of CPU's turn for delays
                if (tempTime == 0)
                {
                    tempTime = timer;
                }

                // do this stuff immediately, once
                if (isCPUShooting)
                { 
                    bar.changeBar("angle");
                    activeBar = "angle";
                    turnText.text = "CPU's Turn";
                    createPuck(false);
                    isCPUShooting = false;
                    // first turn on med-hard diff, CPU shoots perfect
                    if (difficulty == 1 && CPUPuckCount == 5)
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
                        (CPUShotAngle, CPUShotPower, CPUShotSpin) = findOpenPath();
                    }

                }

                if (tempTime + 1.5 < timer && Mathf.Abs(line.value - CPUShotAngle) < (1.0f + difficulty) && activeBar == "angle")
                {
                    bar.changeBar("power");
                    activeBar = "power";
                }

                if (tempTime + 3 < timer && Mathf.Abs(line.value - CPUShotPower) < (1.0f + difficulty) && activeBar == "power")
                {
                    if (difficulty < 2)
                    {
                        activeCPUPuckScript.shoot(CPUShotAngle, CPUShotPower);
                        isPlayersTurn = true;
                        turnText.text = "";
                        bar.changeBar("none");
                        activeBar = "none";
                        tempTime = 0;
                        CPUPuckCount--;
                        CPUPuckCountText.text = CPUPuckCount.ToString();
                    }
                    else
                    {
                        bar.changeBar("spin");
                        activeBar = "spin";
                    }
                }
                // spin for hard mode only
                if (tempTime + 4.5 < timer && Mathf.Abs(line.value - CPUShotSpin) < (1.0f + difficulty) && activeBar == "spin")
                {
                    activeCPUPuckScript.shoot(CPUShotAngle, CPUShotPower, CPUShotSpin);
                    isPlayersTurn = true;
                    turnText.text = "";
                    bar.changeBar("none");
                    activeBar = "none";
                    tempTime = 0;
                    CPUPuckCount--;
                    CPUPuckCountText.text = CPUPuckCount.ToString();
                }
            }

            // lastly, increment timer while game is running
            timer += Time.deltaTime;
        }
        // ran out of pucks (game over)
        else if (gameIsRunning && (playerPuckCount <= 0 || CPUPuckCount <= 0) && allPucksAreStopped())
        {
            gameIsRunning = false;
            updateScores();

            if (playerScore > CPUScore)
            {
                gameResultText.text = "You Win!";
                updateHighscore();
            }
            else if (playerScore < CPUScore)
            {
                gameResultText.text = "You Lose";
            }
            else
            {
                gameResultText.text = "Tie";
            }

            gameHud.SetActive(false);
            gameResultScreen.SetActive(true);
        }

        if (debug)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                activePlayerPuckScript.shoot(50.0f, 10.0f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                activePlayerPuckScript.shoot(50.0f, 20.0f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                activePlayerPuckScript.shoot(50.0f, 30.0f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                activePlayerPuckScript.shoot(50.0f, 40.0f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                activePlayerPuckScript.shoot(50.0f, 50.0f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                activePlayerPuckScript.shoot(50.0f, 60.0f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                activePlayerPuckScript.shoot(50.0f, 70.0f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                activePlayerPuckScript.shoot(50.0f, 80.0f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                activePlayerPuckScript.shoot(50.0f, 90.0f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                activePlayerPuckScript.shoot(50.0f, 100.0f);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                activePlayerPuckScript.rb.AddTorque(500f);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                activePlayerPuckScript.rb.AddTorque(-500f);
            }
        }
    }

    public void restartGame(int diff)
    {
        difficulty = diff;
        clearAllPucks();
        gameHud.SetActive(true);
        titleScreen.SetActive(false);
        table.GetComponent<TableScript>().showBoard();
        playerPuckCount = 5;
        CPUPuckCount = 5;
        randomizeCPUPuckSprite();
        // reset UI
        gameResultText.text = "";
        playerScoreText.text = 0.ToString();
        CPUScoreText.text = 0.ToString();
        playerPuckCountText.text = 5.ToString();
        CPUPuckCountText.text = 5.ToString();
        // flags
        gameIsRunning = true;
        isCPUShooting = true;
        puckHalo.SetActive(diff == 0);
    }

    public void createPuck(bool isPlayersPuck)
    {
        if (isPlayersPuck)
        {
            activePlayerPuckObject = Instantiate(puck, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
            activePlayerPuckScript = activePlayerPuckObject.GetComponent<PuckScript>();
            activePlayerPuckScript.initPuck(true, playerPuckSprite);
        }
        else
        {
            activeCPUPuckObject = Instantiate(puck, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
            activeCPUPuckScript = activeCPUPuckObject.GetComponent<PuckScript>();
            activeCPUPuckScript.initPuck(false, CPUPuckSprite);
        }
    }

    private PuckScript pucki;
    public bool allPucksAreStopped()
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            pucki = puck.GetComponent<PuckScript>();
            // IF shot, but not stopped, return false
            if (pucki.isShot() && !pucki.isStopped())
            {
                return false;
            }
        }
        // IF all shot puts are stopped, return true
        return true;
    }

    public bool allPucksAreSlowed()
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            pucki = puck.GetComponent<PuckScript>();
            // IF shot, but not stopped, return false
            if (pucki.isShot() && !pucki.isSlowed())
            {
                return false;
            }
        }
        // IF all shot puts are stopped, return true
        return true;
    }

    public void updateScores()
    {
        int playerSum = 0;
        int CPUSum = 0;
        var objects = GameObject.FindGameObjectsWithTag("puck");
        foreach (var obj in objects)
        {
            pucki = obj.GetComponent<PuckScript>();
            if (pucki.isPlayersPuck())
            {
                playerSum += pucki.computeValue();
            }
            else
            {
                CPUSum += pucki.computeValue();
            }
            
        }
        playerScore = playerSum;
        playerScoreText.text = playerScore.ToString();
        
        CPUScore = CPUSum;
        CPUScoreText.text = CPUScore.ToString();
    }

    private void cleanupDeadPucks()
    {
        var objects = GameObject.FindGameObjectsWithTag("puck");
        var objectCount = objects.Length;
        foreach (var obj in objects)
        {
            pucki = obj.GetComponent<PuckScript>();
            Rigidbody2D puckiRB = GetComponent<Rigidbody2D>();
            if (!pucki.isSafe() && pucki.isStopped())
            {
                Destroy(obj);
            }
        }
    }

    private void clearAllPucks()
    {
        var objects = GameObject.FindGameObjectsWithTag("puck");
        var objectCount = objects.Length;
        foreach (var obj in objects)
        {
            Destroy(obj);
        }
    }

    private Sprite colorIDtoPuckSprite(int id)
    {
        switch (id)
        {
            case (0):
                return puckBlue;
            case (1):
                return puckGreen;
            case (2):
                return puckGrey;
            case (3):
                return puckOrange;
            case (4):
                return puckPink;
            case (5):
                return puckPurple;
            case (6):
                return puckRed;
            case (7):
                return puckYellow;
            case (8):
                return puckRainbow;
            case (9):
                return puckCanada;
            case (10):
                return puckDonut;
            case (11):
                return puckCaptain;
            default:
                return puckBlue;
        }
    }

    public void selectPlayerPuckSprite(int id)
    {
        playerPuckSprite = colorIDtoPuckSprite(id);
        playerPuckIcon.GetComponent<Image>().sprite = playerPuckSprite;
        while (CPUPuckSprite == playerPuckSprite)
        {
            CPUPuckSprite = colorIDtoPuckSprite(Random.Range(0, 8));
            CPUPuckIcon.GetComponent<Image>().sprite = CPUPuckSprite;
        }
        togglePopup(customizePopup);
    }

    public void randomizeCPUPuckSprite()
    {
        CPUPuckSprite = colorIDtoPuckSprite(Random.Range(0, 8));
        CPUPuckIcon.GetComponent<Image>().sprite = CPUPuckSprite;
        while (CPUPuckSprite == playerPuckSprite)
        {
            CPUPuckSprite = colorIDtoPuckSprite(Random.Range(0, 8));
            CPUPuckIcon.GetComponent<Image>().sprite = CPUPuckSprite;
        }
    }

    public void togglePopup(GameObject popup)
    {
        popup.SetActive(!popup.activeInHierarchy);
        toggleMainMenuButtons();
        updateLocks();
        errorMessage.text = "";
    }

    [ContextMenu("Toggle Main Menu Buttons")]
    private void toggleMainMenuButtons()
    {
        Transform[] allChildren = titleScreen.transform.GetComponentsInChildren<Transform>(true);
        for (int i = 1; i < allChildren.Length; i++)
        {
            //Debug.Log(allChildren[i].gameObject);
            //allChildren.transform.parent.gameObject.SetActive(!gameObject.activeInHierarchy);
            allChildren[i].gameObject.SetActive(!allChildren[i].gameObject.activeInHierarchy);
        }
    }

    public void returnToMenu()
    {
        gameIsRunning = false;
        playerPuckCount = 0;
        CPUPuckCount = 0;
        gameHud.SetActive(false);
        gameResultScreen.SetActive(false);
        titleScreen.SetActive(true);
    }

    private CPUPathScript best = null;
    private (float, float, float) findOpenPath()
    {
        var pathCount = CPUPaths.Length;
        var highestValue = 0;
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
        if (highestValue > 0)
        {
            return (best.angle, best.power, best.spin);
        }
        else
        {
            Debug.Log("No path :(");
            return (Random.Range(35.0f, 65.0f), Random.Range(40.0f, 70.0f), Random.Range(40.0f, 50.0f));
        }
    }

    private void updateHighscore()
    {
        var currentHighscore = playerScore - CPUScore;
        switch (difficulty)
        {
            case (0):
                if (currentHighscore > PlayerPrefs.GetInt("easyHighscore"))
                {
                    PlayerPrefs.SetInt("easyHighscore", currentHighscore);
                    updateProfileText();
                }
                break;
            case (1):
                if (currentHighscore > PlayerPrefs.GetInt("mediumHighscore"))
                {
                    PlayerPrefs.SetInt("mediumHighscore", currentHighscore);
                    updateProfileText();
                }
                break;
            case (2):
                if (currentHighscore > PlayerPrefs.GetInt("hardHighscore"))
                {
                    PlayerPrefs.SetInt("hardHighscore", currentHighscore);
                    updateProfileText();
                }
                break;
        }
    }

    private void updateProfileText() 
    {
        profilePopupText.text = "Highscores: \n" +
            "\nEasy: " + PlayerPrefs.GetInt("easyHighscore") +
            "\nMedium: " + PlayerPrefs.GetInt("mediumHighscore") +
            "\nHard: " + PlayerPrefs.GetInt("hardHighscore");
    }

    private void updateLocks()
    {
        //Debug.Log("updateLocks");
        if (PlayerPrefs.GetInt("easyHighscore") > 0)
        {
            //Debug.Log("easyHighscore");

            GameObject[] gameObjectArray = GameObject.FindGameObjectsWithTag("easyLock");

            foreach (GameObject go in gameObjectArray)
            {
                go.SetActive(false);
            }
        }
        if (PlayerPrefs.GetInt("mediumHighscore") > 0)
        {
            //Debug.Log("mediumHighscore");

            GameObject[] gameObjectArray = GameObject.FindGameObjectsWithTag("mediumLock");

            foreach (GameObject go in gameObjectArray)
            {
                go.SetActive(false);
            }
        }
        if (PlayerPrefs.GetInt("hardHighscore") > 0)
        {
            //Debug.Log("updateLocks");

            GameObject[] gameObjectArray = GameObject.FindGameObjectsWithTag("hardLock");

            foreach (GameObject go in gameObjectArray)
            {
                go.SetActive(false);
            }
        }
    }

    public void setErrorMessage(string msg)
    {
        errorMessage.text = msg;
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    /*
    // this only exists cuz of a stupid error idk how to fix yet
    private bool puckIsShotAndStopped(PuckScript ps)
    {
        if (ps == null)
        {
            return false;
        }
        return (ps.isShot() && ps.isStopped());
    }

    private bool puckIsShotAndSlowed(PuckScript ps)
    {
        if (ps == null)
        {
            return false;
        }
        return (ps.isShot() && ps.isSlowed());
    }
    */
}
