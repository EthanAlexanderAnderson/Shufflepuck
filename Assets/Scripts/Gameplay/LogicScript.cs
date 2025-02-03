/* Logic Script is kinda like the "main" function for playing vs CPU, 
 * it controls most things happening in game and directs other scripts.
 */

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

    // Hard CPU shot paths
    GameObject[] CPUPaths;

    // powerups
    public bool powerupsAreEnabled = false;
    [SerializeField] private GameObject powerupsMenu; // set in editor
    private int powerupsUsedThisTurn = 0;
    [SerializeField] private GameObject powerupsToggle;

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
        // check if tutorial should be active
        tutorialActive = PlayerPrefs.GetInt("tutorialCompleted") == 0 && PlayerPrefs.GetInt("easyWin") == 0 && PlayerPrefs.GetInt("easyHighscore") == 0;
        // load if powerups should be active
        powerupsAreEnabled = PlayerPrefs.GetInt("PowerupsEnabled", 0) == 1;
        powerupsToggle.GetComponent<Toggle>().isOn = powerupsAreEnabled;
        // initialize CPU paths
        CPUPaths = GameObject.FindGameObjectsWithTag("cpu_path");
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
            // clear pucks in non-safe zones
            puckManager.CleanupDeadPucks();

            // update wall status
            if (wallCount == 0 && puckManager.AllPucksAreSlowedMore())
            {
                WallScript.Instance.WallEnabled(false);
                UI.UpdateWallText(wallCount);
                wallCount--;
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
        else if (gameIsRunning && puckManager.AllPucksAreStopped())
        {
            gameIsRunning = false;
            UpdateScores();
            UI.ChangeUI(UI.gameResultScreen);
            UI.UpdateGameResult(player.score, opponent.score, difficulty, isLocal);
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
        if (player.puckCount == 0 && opponent.puckCount > 0)
        {
            player.isTurn = false;
            opponent.isTurn = true;
            return;
        }

        activeCompetitor = player;
        nonActiveCompetitor = opponent;

        if (difficulty >= 2 && !isLocal)
        {
            if (powerupsAreEnabled) { powerupManager.ShufflePowerups(); }
            powerupsMenu.SetActive(powerupsAreEnabled);
            powerupsUsedThisTurn = 0;
        }
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
        // if opponents starts their turn with no pucks remaining, skip their turn
        if (opponent.puckCount <= 0 && player.puckCount > 0)
        {
            opponent.isTurn = false;
            player.isTurn = true;
            return;
        }

        activeCompetitor = opponent;
        nonActiveCompetitor = player;

        powerupsUsedThisTurn = 0;
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
            if (difficulty >= 2 && !isLocal && powerupsAreEnabled && powerupsUsedThisTurn < 3) { CPUPreShotPowerups(); }
            (CPUShotAngle, CPUShotPower, CPUShotSpin) = FindOpenPath();
            if (difficulty >= 2 && !isLocal && powerupsAreEnabled && powerupsUsedThisTurn < 3) { CPUPostShotPowerups(); }
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
        powerupManager.DisableForceFieldIfNecessary();
        Debug.Log("Shooting: " + angle + " | " + power + " | " + spin);
        activeBar = bar.ChangeBar("none");
        line.isActive = false;
        arrow.SetActive(false);
        GameHUDManager.Instance.ChangeTurnText(string.Empty);
        activeCompetitor.ShootActivePuck(angle, power, spin);
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
            if (powerupsAreEnabled) { powerupManager.LoadDeck(); }
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
        FogScript.Instance.DisableFog();
        WallScript.Instance.WallEnabled(false);
#if (UNITY_EDITOR)
        foreach (var path in CPUPaths)
        {
            var pathi = path.GetComponent<CPUPathScript>();
            pathi.DisablePathVisualization();
        }
#endif
    }

    // this is the CPU AI for hard mode
    private (float, float, float) FindOpenPath()
    {
        // if Fog is active and foresight isn't, shoot random
        if (FogScript.Instance.FogEnabled() && !HaloScript.Instance.HaloEnabled()) { return (Random.Range(35.0f, 65.0f), Random.Range(40.0f, 70.0f), Random.Range(45.0f, 55.0f)); }

        CPUPathScript best = null;
        int highestValue = 0;
        // check all paths to see which are unblocked
        foreach (var path in CPUPaths)
        {
            var pathi = path.GetComponent<CPUPathScript>();
            pathi.DisablePathVisualization();
            var pathiShotValue = pathi.CalculateValue();
            float randomValue = Random.Range(0f, 1f);
            if (pathiShotValue > highestValue || (pathiShotValue == highestValue && randomValue < 0.25))
            {
                // handle phase powerup shots
                if (!powerupsAreEnabled && pathi.DoesPathRequirePhasePowerup())
                {
                    continue;
                }
                // handle phase powerup shots
                if (!powerupsAreEnabled && pathi.DoesPathRequireExplosionPowerup())
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

            // handle powerup shots
            if (best.DoesPathRequirePhasePowerup() && powerupsAreEnabled && powerupsUsedThisTurn < 3)
            {
                powerupManager.PhasePowerup();
                powerupsUsedThisTurn++;
            }
            else if (best.DoesPathRequireExplosionPowerup() && powerupsAreEnabled && powerupsUsedThisTurn < 3)
            {
                powerupManager.ExplosionPowerup();
                powerupsUsedThisTurn++;
            }
            else if (best.IsPathAContactShot() && powerupsAreEnabled && powerupsUsedThisTurn < 3)
            {
                powerupManager.ForceFieldPowerup();
                powerupsUsedThisTurn++;
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

    private void CPUPreShotPowerups()
    {
        float randomValue = Random.Range(0f, 1f);
        // first two shots use block
        if (opponent.puckCount > 3 && randomValue < 0.75)
        {
            powerupManager.BlockPowerup();
            powerupsUsedThisTurn++;
        }
        // shot three and four check if bolt is needed, if it isn't use foresight
        else if (opponent.puckCount > 1 && randomValue < 0.75)
        {
            // if the ratio of player pucks to opponent pucks is greater than 2, use bolt
            var allPucks = GameObject.FindGameObjectsWithTag("puck");
            float playerPucks = 0;
            float opponentPucks = 0.001f; // so we don't divide by zero
            foreach (var puck in allPucks)
            {
                var puckScript = puck.GetComponent<PuckScript>();
                if (puckScript.IsPlayersPuck() && puckScript.ComputeValue() > 0 && !puckScript.IsHydra())
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
                powerupManager.BoltPowerup();
                powerupsUsedThisTurn++;
            }
            else if (FogScript.Instance.FogEnabled())
            {
                powerupManager.ForesightPowerup();
                powerupsUsedThisTurn++;
            }
        }
        // last shot, cull
        else if (opponent.puckCount == 1 && randomValue < 0.75)
        {
            powerupManager.CullPowerup();
        }
    }

    private void CPUPostShotPowerups()
    {
        // plus one, growth, hydra
        float randomValue = Random.Range(0f, 1f);
        if (opponent.puckCount > 3 && randomValue < 0.75)
        {
            powerupManager.GrowthPowerup();
        }
        else if (opponent.puckCount < 3 && randomValue < 0.75)
        {
            powerupManager.PlusOnePowerup();
        }
        else if (opponent.puckCount == 3 && randomValue < 0.75)
        {
            powerupManager.HydraPowerup();
        }
        powerupsUsedThisTurn++;
        if (powerupsUsedThisTurn >= 3) { return; }

        // lock, fog
        randomValue = Random.Range(0f, 1f);
        if (opponent.puckCount > 3 && randomValue < 0.75)
        {
            powerupManager.LockPowerup();
        }
        else if ((opponent.puckCount == 3 || opponent.puckCount == 2) && randomValue < 0.50)
        {
            powerupManager.FogPowerup();
        }
        powerupsUsedThisTurn++;
        if (powerupsUsedThisTurn >= 3) { return; }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    [SerializeField] GameObject deckScreen;
    public void SetPowerups(bool value)
    {
        powerupsAreEnabled = value;
        PlayerPrefs.SetInt("PowerupsEnabled", value ? 1 : 0);
        if (value)
        {
            UI.ChangeUI(deckScreen); // TODO: make this not dumb
        }
    }

    [SerializeField] private AudioSource puckDestroySFX;
    public void playDestroyPuckSFX(float SFXvolume)
    {
        puckDestroySFX.volume = SFXvolume;
        puckDestroySFX.Play();
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

}
