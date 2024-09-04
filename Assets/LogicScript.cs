/* Logic Script is kinda like the "main" function for playing vs CPU, 
 * it controls most things happening in game and directs other scripts.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogicScript : MonoBehaviour
{
    UIManagerScript UI;

    // prefabs
    public GameObject puckPrefab; // default puck prefab

    // sprites
    
    [SerializeField] Sprite puckFlower;
    [SerializeField] Sprite puckMissing;

    [SerializeField] Sprite puckBlue;
    [SerializeField] Sprite puckGreen;
    [SerializeField] Sprite puckGrey;
    [SerializeField] Sprite puckOrange;
    [SerializeField] Sprite puckPink;
    [SerializeField] Sprite puckPurple;
    [SerializeField] Sprite puckRed;
    [SerializeField] Sprite puckYellow;

    [SerializeField] Sprite puckRainbow;
    [SerializeField] Sprite puckCanada;
    [SerializeField] Sprite puckDonut;
    [SerializeField] Sprite puckCaptain;
    [SerializeField] Sprite puckNuke;
    [SerializeField] Sprite puckWreath;
    [SerializeField] Sprite puckSky;
    [SerializeField] Sprite puckDragon;
    [SerializeField] Sprite puckNinja;
    [SerializeField] Sprite puckEgg;
    [SerializeField] Sprite puckMonster;
    [SerializeField] Sprite puckEye;
    [SerializeField] Sprite puckCamo;
    [SerializeField] Sprite puckYingYang;
    [SerializeField] Sprite puckCow;
    [SerializeField] Sprite puckCraft;
    [SerializeField] Sprite puckPlanet;
    [SerializeField] Sprite puckLove;
    [SerializeField] Sprite puckAura;
    [SerializeField] Sprite puckCheese;
    [SerializeField] Sprite puckScotia;

    [SerializeField] Sprite puckBlueAlt;
    [SerializeField] Sprite puckGreenAlt;
    [SerializeField] Sprite puckGreyAlt;
    [SerializeField] Sprite puckOrangeAlt;
    [SerializeField] Sprite puckPinkAlt;
    [SerializeField] Sprite puckPurpleAlt;
    [SerializeField] Sprite puckRedAlt;
    [SerializeField] Sprite puckYellowAlt;

    [SerializeField] Sprite puckRainbowAlt;
    [SerializeField] Sprite puckCanadaAlt;
    [SerializeField] Sprite puckDonutAlt;
    [SerializeField] Sprite puckCaptainAlt;
    [SerializeField] Sprite puckNukeAlt;
    [SerializeField] Sprite puckWreathAlt;
    [SerializeField] Sprite puckSkyAlt;
    [SerializeField] Sprite puckDragonAlt;
    [SerializeField] Sprite puckNinjaAlt;
    [SerializeField] Sprite puckEggAlt;
    [SerializeField] Sprite puckMonsterAlt;
    [SerializeField] Sprite puckEyeAlt;
    [SerializeField] Sprite puckCamoAlt;
    [SerializeField] Sprite puckYingYangAlt;
    [SerializeField] Sprite puckCowAlt;
    [SerializeField] Sprite puckCraftAlt;
    [SerializeField] Sprite puckPlanetAlt;
    [SerializeField] Sprite puckLoveAlt;
    [SerializeField] Sprite puckAuraAlt;
    [SerializeField] Sprite puckCheeseAlt;
    [SerializeField] Sprite puckScotiaAlt;

    [SerializeField] GameObject puckHalo;

    public GameObject SFXParent;
    public GameObject musicParent;
    public GameObject soundManager;

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
    private bool playedAGame = false;

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
        SelectPlayerPuckSprite(PlayerPrefs.GetInt("puck") == 0 && PlayerPrefs.GetInt("hardHighscore") <= 5 ? 1 : PlayerPrefs.GetInt("puck"));
        activeCompetitor = opponent;
        nonActiveCompetitor = player;
        // apply audio preferences
        soundManager.GetComponent<SoundManagerScript>().Load();
    }

    // Update is called once per frame
    void Update()
    {
        // only do this stuff when game is running (not in menu etc.)
        if (gameIsRunning && (player.puckCount > 0 || opponent.puckCount > 0))
        {
            // clear pucks in non-safe zones
            CleanupDeadPucks();

            // update wall status
            if (wallCount == 0 && AllPucksAreSlowedMore())
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
                activeBar = bar.ChangeBar("angle", leftsTurn());
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
                soundManager.GetComponent<SoundManagerScript>().PlayClickSFX();
                // change state on tap depending on current state
                switch (activeBar)
                {
                    case "angle":
                        angle = line.value;
                        activeBar = bar.ChangeBar("power", leftsTurn());
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
                            activeBar = bar.ChangeBar("spin", leftsTurn());
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
            if (opponent.isTurn && (player.activePuckScript == null || (player.activePuckScript.IsSlowed() && (AllPucksAreSlowed() && difficulty < 2 || AllPucksAreSlowedMore()))))
            {
                activeBar = bar.ChangeBar("angle", leftsTurn());
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
                    activeBar = bar.ChangeBar("power", leftsTurn());
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
                        activeBar = bar.ChangeBar("spin", leftsTurn());
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
        else if (gameIsRunning && (player.puckCount <= 0 || opponent.puckCount <= 0) && AllPucksAreStopped())
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
        ClearAllPucks();
        UI.titleScreenBackground.SetActive(false);
        // reset game variables
        RandomizeCPUPuckSprite();
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
        // IF all shot puts are slowed, return true
        return true;
    }

    // useful helper function for deciding when to allow actions. Returns true when all pucks have slowed down more
    public bool AllPucksAreSlowedMore()
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            pucki = puck.GetComponent<PuckScript>();
            // IF shot, but not stopped, return false
            if (pucki.IsShot() && !pucki.IsSlowedMore())
            {
                return false;
            }
        }
        // IF all shot puts are slowed more, return true
        return true;
    }

    public bool leftsTurn()
    {
        return (((player.isTurn || player.isShooting) && goingFirst) || ((opponent.isTurn || opponent.isShooting) && !goingFirst));
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
        Sprite[] puckSprites = { puckFlower, puckBlue, puckGreen, puckGrey, puckOrange, puckPink, puckPurple, puckRed, puckYellow, puckRainbow, puckCanada, puckDonut, puckCaptain, puckNuke, puckWreath, puckSky, puckDragon, puckNinja, puckEgg, puckMonster, puckEye, puckCamo, puckYingYang, puckCow, puckCraft, puckPlanet, puckLove, puckAura, puckCheese, puckScotia };
        Sprite[] puckAltSprites = { null, puckBlueAlt, puckGreenAlt, puckGreyAlt, puckOrangeAlt, puckPinkAlt, puckPurpleAlt, puckRedAlt, puckYellowAlt, puckRainbowAlt, puckCanadaAlt, puckDonutAlt, puckCaptainAlt, puckNukeAlt, puckWreathAlt, puckSkyAlt, puckDragonAlt, puckNinjaAlt, puckEggAlt, puckMonsterAlt, puckEyeAlt, puckCamoAlt, puckYingYangAlt, puckCowAlt, puckCraftAlt, puckPlanetAlt, puckLoveAlt, puckAuraAlt, puckCheeseAlt, puckScotiaAlt };

        // if out of range, return missing
        if ((id >= puckSprites.Length) || (id <= puckSprites.Length * -1))
        {
            return puckMissing;
        }

        // if postitive, return regular, else return alt
        try
        {
            if (id >= 0)
            {
                return (puckSprites[id]);
            }
            else
            {
                return (puckAltSprites[id * -1]);
            }
        }
        catch (System.IndexOutOfRangeException)
        {
            return puckMissing;
        }
    }

    // helper with the puck customization buttons
    public void SelectPlayerPuckSprite(int id)
    {
        player.puckSpriteID = id;
        player.puckSprite = ColorIDtoPuckSprite(id);
        UI.SetPlayerPuckIcon(player.puckSprite);
        PlayerPrefs.SetInt("puck", id);
        RandomizeCPUPuckSprite();
        PlayerPrefs.SetInt("ShowNewSkinAlert", 0);
    }


    private List<int> unlockedPuckIDs = new List<int> { -8, -7, -6, -5, -4, -3, -2, -1, 1, 2, 3, 4, 5, 6, 7, 8 };

    public void UnlockPuckID(int id)
    {
        // if not already unlocked, add to list
        if (!unlockedPuckIDs.Contains(id)) {
            unlockedPuckIDs.Add(id);
            if (playedAGame)
            {
                PlayerPrefs.SetInt("ShowNewSkinAlert", 1);
            }
        }
    }

    // helper for randomize puck button
    public void RandomizePlayerPuckSprite()
    {
        var prev = player.puckSprite;
        int rng;
        do
        {
            rng = Random.Range(0, unlockedPuckIDs.Count);

            player.puckSpriteID = unlockedPuckIDs[rng];
            player.puckSprite = ColorIDtoPuckSprite(unlockedPuckIDs[rng]);
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
            opponent.puckSprite = ColorIDtoPuckSprite(Random.Range(1, 9));
        } while (opponent.puckSprite == player.puckSprite);
        UI.SetOpponentPuckIcon(opponent.puckSprite);
    }

    // helper for customize puck menu buttons
    public void SwapToAltSkin(GameObject gameObject)
    {
        if (gameObject.tag == "alt")
        {
            SelectPlayerPuckSprite(player.puckSpriteID * -1);
            gameObject.tag = "Untagged";

        } 
        else
        {
            gameObject.tag = "alt";
        }

        gameObject.GetComponent<Image>().sprite = ColorIDtoPuckSprite(player.puckSpriteID * -1);
    }

    // helper for easter egg button
    int easterEggCounter = 0;
    [SerializeField] Transform easterEggBox;
    [SerializeField] Transform antiEasterEggBox;
    public void EasterEgg()
    {
        easterEggCounter++;
        easterEggBox.position += transform.right * 1.4f;
        antiEasterEggBox.position += transform.right * 1.4f;
        if (easterEggCounter == 11)
        {
            SelectPlayerPuckSprite(0);
            ResetEasterEgg();
        }
    }
    public void ResetEasterEgg()
    {
        easterEggCounter = 0;
        easterEggBox.position = new Vector3(-7.10f, 14.40f, 0f);
        antiEasterEggBox.position = new Vector3(2.50f, 14.40f, 0f);
    }

    // helper for debug mode button
    int debugMode = 0;
    public void DebugMode()
    {
        debugMode++;
        if (debugMode == 10)
        {
            UI.EnableScreenLog();
        }
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
        return soundManager.GetComponent<SoundManagerScript>().GetSFXVolume();
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
