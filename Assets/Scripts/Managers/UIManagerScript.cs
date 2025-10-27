// Most stuff involved with chaning or managing UI elements is done here
// Methods are self explanatory for the most part

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManagerScript : MonoBehaviour
{
    // self
    public static UIManagerScript Instance;

    // dependancies
    private LogicScript logic;
    private SoundManagerScript sound;
    private DailyChallengeManagerScript dailyChallenge;

    // UI Parent Objects
    public GameObject titleScreen;
    public GameObject gameHud;
    public GameObject gameResultScreen;
    public GameObject puckScreen;
    public GameObject deckScreen;
    public GameObject rewardsScreen;
    public GameObject questsScreen;
    public GameObject profileScreen;
    public GameObject shopScreen;
    public GameObject plinkoScreen;
    public GameObject waitingScreen;
    public GameObject packOpenScreen;

    // title
    [SerializeField] private GameObject playerPuckIcon;
    [SerializeField] private GameObject playerPuckIconAnimation;
    [SerializeField] private GameObject opponentPuckIcon;
    [SerializeField] private GameObject opponentPuckIconAnimation;
    [SerializeField] private GameObject activePuckIcon;
    [SerializeField] private GameObject activePuckIconAnimation;
    [SerializeField] private TMP_Text errorMessage;
    [SerializeField] private TMP_Text profilePopupText;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject titleScreenBackground;
    [SerializeField] private GameObject puckAlert;
    [SerializeField] private GameObject profileAlert;

    // Lobby
    public TMP_Text lobbyCodeText;
    public GameObject waitingGif;
    public TMP_Text waitingText;
    [SerializeField] private GameObject waitingBackButton;

    // tutorial
    public GameObject tutorialMenu;
    private GameObject[] tutorialPages;
    public int iPage = 0;
    public GameObject page0;
    public GameObject page1;
    public GameObject page2;
    public GameObject page3;
    public GameObject page4;
    public GameObject page5;
    public GameObject page6;
    public GameObject page7;
    public GameObject page8;
    public GameObject page9;

    // HUD
    [SerializeField] private Text turnText;

    public TMP_Text playerUsernameText;
    public TMP_Text opponentUsernameText;

    public Text playerPuckCountText;
    public Text opponentPuckCountText;

    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text opponentScoreText;
    [SerializeField] private Text playerScoreBonusText;
    [SerializeField] private Text opponentScoreBonusText;

    private int playerWins;
    private int opponentWins;
    [SerializeField] private GameObject playerWinsObject;
    [SerializeField] private GameObject opponentWinsObject;
    [SerializeField] private Text playerWinsText;
    [SerializeField] private Text opponentWinsText;

    public Text shotClockText;

    [SerializeField] private Text AngleDebugText;
    [SerializeField] private Text PowerDebugText;
    [SerializeField] private Text SpinDebugText;

    [SerializeField] private GameObject restartButton;

    [SerializeField] private GameObject ExitConfirmation;
    [SerializeField] private GameObject RestartConfirmation;

    // result
    [SerializeField] private Text gameResultText;
    [SerializeField] private Text gameResultHighscoreMessageText;

    [SerializeField] private GameObject rematchButton;
    public GameObject onlineRematchButton;

    // local
    [SerializeField] private GameObject activeUI;
    private GameObject previousActiveUI;

    [SerializeField] private TMP_Text wallText;
    [SerializeField] private GameObject table;
    [SerializeField] private GameObject slider;

    // dark / light mode assets
    private bool darkMode = false;
    public bool GetDarkMode() { return darkMode; }
    [SerializeField] private GameObject darkModeToggle;
    [SerializeField] private Sprite titleScreenLight;
    [SerializeField] private Sprite titleScreenDark;
    [SerializeField] private Sprite titleScreenBackgroundLight;
    [SerializeField] private Sprite titleScreenBackgroundDark;
    [SerializeField] private Sprite tableLight;
    [SerializeField] private Sprite tableDark;

    // settings menu misc
    [SerializeField] private GameObject experimentalFeaturesToggle;

    public string TurnText
    {
        get => turnText.text;
        set
        {
            LeanTween.alpha(turnText.gameObject, 0f, 0.001f);
            LeanTween.alpha(turnText.gameObject, 1f, 1f).setDelay(1f);
            turnText.text = value;
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        tutorialPages = new GameObject[] { page0, page1, page2, page3, page4, page5, page6, page7, page8, page9 };
    }

    private void Start()
    {
        logic = LogicScript.Instance;
        sound = SoundManagerScript.Instance;
        dailyChallenge = DailyChallengeManagerScript.Instance;

        puckScreen.SetActive(true);
        UpdateLocks();
        puckScreen.SetActive(false);
        plinkoScreen.SetActive(true);
        PlayerDataManager.Instance.PlinkoDataSwap();
        PlinkoManager.Instance.CheckForNewDailyPlinkoReward();
        plinkoScreen.SetActive(false);
        if (!PlayerPrefs.HasKey("DailyChallenge1")) { PlayerPrefs.SetInt("DailyChallenge1", 1); }
        if (!PlayerPrefs.HasKey("DailyChallenge2")) { PlayerPrefs.SetInt("DailyChallenge2", 1); }

        if (PlayerPrefs.GetInt("darkMode", 0) == 1)
        {
            darkMode = true;
            ApplyDarkMode();
            darkModeToggle.GetComponent<Toggle>().isOn = true;
        }

        experimentalFeaturesToggle.GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("experimental") == 1;
    }

    float cooldownTime;
    bool enabledReadyButton;
    private void Update()
    {
        cooldownTime -= Time.deltaTime;
        if (enabledReadyButton && cooldownTime <= 0f)
        {
            readyButton.SetActive(true);
            if (waitingText.text != "1/2 Players Ready")
            {
                waitingText.text = "0/2 Players Ready";
            }
            waitingGif.SetActive(false);
            enabledReadyButton = false;
        }
        if (!enabledReadyButton && cooldownTime <= -5f)
        {
            waitingBackButton.SetActive(true);
        }
        // Check if the android back button (Escape key) is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Handle the back button press
            HandleBackButton();
        }

        if (Input.GetMouseButtonDown(0) && tutorialMenu.activeInHierarchy && iPage != 6)
        {
            // make sure click is not on a puck
            if (logic.ClickNotOnPuck())
            {
                AdvanceTutorial();
            }
        }
    }

    public void GameHUDExitButtonHandler()
    {
        if (!ClientLogicScript.Instance.isRunning && LogicScript.Instance.player.puckCount >= 5)
        {
            LogicScript.Instance.ReturnToMenu();
        }
        else
        {
            ExitConfirmation.SetActive(true);
        }
    }

    public void GameHUDRestartButtonHandler()
    {
        if (!ClientLogicScript.Instance.isRunning && LogicScript.Instance.player.puckCount >= 5)
        {
            LogicScript.Instance.RestartGame(-1);
        }
        else
        {
            RestartConfirmation.SetActive(true);
        }
    }

    public void AdvanceTutorial()
    {
        tutorialPages[iPage].SetActive(false);
        if (iPage < (tutorialPages.Length - 1))
        {
            iPage++;
            tutorialPages[iPage].SetActive(true);
        } 
        else
        {
            logic.tutorialActive = false;
            PlayerPrefs.SetInt("tutorialCompleted", 1);
        }
    }

    int prevPlayerPuckCount = 5;
    int prevOpponentPuckCount = 5;
    public void PostShotUpdate(int playerPuckCount, int opponentPuckCount)
    {
        if (!playerPuckCountText || !opponentPuckCountText) { return; }

        var newPlayerPuckCount = Mathf.Max(0, playerPuckCount);
        GameHUDManager.Instance.ChangePuckCountText(true, newPlayerPuckCount.ToString(), newPlayerPuckCount != prevPlayerPuckCount);
        prevPlayerPuckCount = playerPuckCount;

        var newOpponentPuckCount = Mathf.Max(0, opponentPuckCount);
        GameHUDManager.Instance.ChangePuckCountText(false, newOpponentPuckCount.ToString(), newOpponentPuckCount != prevOpponentPuckCount);
        prevOpponentPuckCount = opponentPuckCount;
    }

    int prevPlayerScore;
    int prevOpponentScore;
    public void UpdateScores(int playerScore, int opponentScore)
    {
        if (!playerScoreText || !opponentScoreText) { return; }

        var newPlayerScore = Mathf.Max(0, playerScore);
        GameHUDManager.Instance.ChangeScoreText(true, newPlayerScore.ToString(), newPlayerScore != prevPlayerScore);
        prevPlayerScore = newPlayerScore;

        var newOpponentScore = Mathf.Max(0, opponentScore);
        GameHUDManager.Instance.ChangeScoreText(false, newOpponentScore.ToString(), newOpponentScore != prevOpponentScore);
        prevOpponentScore = newOpponentScore;

        GameHUDManager.Instance.UpdateParticleEffects(playerScore, opponentScore);
    }

    public void UpdateScoreBonuses(int playerScoreBonus, int opponentScoreBonus)
    {
        if (!playerScoreBonusText || !opponentScoreBonusText) { return; }

        if (playerScoreBonus != 0)
        {
            playerScoreBonusText.text = playerScoreBonus.ToString();
        }
        else
        {
            playerScoreBonusText.text = string.Empty;
        }

        if (opponentScoreBonus != 0)
        {
            opponentScoreBonusText.text = opponentScoreBonus.ToString();
        }
        else
        {
            opponentScoreBonusText.text = string.Empty;
        }

    }

    public void UpdateWallText(int wallCount)
    {
        if (wallCount <= 0)
        {
            wallText.text = "";
            return;
        }
        wallText.text = "wall drops in " + wallCount;
    }

    public void UpdateGameResult(int playerScore, int opponentScore, int difficulty, bool isLocal, bool isOnline = false, int opponentElo = -1)
    {
        // for online mode
        if (playerScore == -1) playerScore = Int32.Parse(playerScoreText.text);
        if (opponentScore == -1) opponentScore = Int32.Parse(opponentScoreText.text);

        int scoreDifference = playerScore - opponentScore;

        // generic win/loss/tie playerpref 
        if (playerScore > opponentScore)
        {
            IncrementPlayerPref("win");
            if (!isLocal) { PowerupManager.Instance.UpdateWonUsingPlayerPref(); }
            sound.PlayWinSFX();
        } 
        else if (playerScore < opponentScore)
        {
            IncrementPlayerPref("loss");
        }
        else
        {
            IncrementPlayerPref("tie");
        }

        if (isOnline)
        {
            playerWinsObject.SetActive(true);
            opponentWinsObject.SetActive(true);
            if (opponentScore < playerScore)
            {
                gameResultText.text = "You Win!";
                gameResultHighscoreMessageText.text = "You won by " + System.Math.Abs(scoreDifference) + " points.";
                IncrementPlayerPref("onlineWin");
                playerWins++;
                if (playerWins + opponentWins >= 2)
                {
                    gameResultHighscoreMessageText.text +=  $"\nWins: {playerWins} - {opponentWins}";

                }
                gameResultHighscoreMessageText.text += dailyChallenge.EvaluateChallenge(2, scoreDifference, 1);
                playerWinsText.text = playerWins.ToString();
                opponentWinsText.text = opponentWins.ToString();

                // update players elo
                if (opponentElo >= 0)
                {
                    var (newElo, _) = EloCalculator.CalculateElo(PlayerPrefs.GetInt("Elo", 100), opponentElo, 1f);
                    PlayerPrefs.SetInt("Elo", newElo);
                }
            }
            else if (opponentScore > playerScore)
            {
                gameResultText.text = "You Lose";
                gameResultHighscoreMessageText.text = "They won by " + System.Math.Abs(scoreDifference) + " points.";
                IncrementPlayerPref("onlineLoss");
                opponentWins++;
                if (playerWins + opponentWins >= 2)
                {
                    gameResultHighscoreMessageText.text += $"\nWins: {playerWins} - {opponentWins}";

                }
                playerWinsText.text = playerWins.ToString();
                opponentWinsText.text = opponentWins.ToString();

                // update players elo
                if (opponentElo >= 0)
                {
                    var (newElo, _) = EloCalculator.CalculateElo(PlayerPrefs.GetInt("Elo", 100), opponentElo, 0f);
                    PlayerPrefs.SetInt("Elo", newElo);
                }
            }
            else
            {
                gameResultText.text = "Tie";
                gameResultHighscoreMessageText.text = "";
                IncrementPlayerPref("onlineTie");
                if (playerWins + opponentWins >= 2)
                {
                    gameResultHighscoreMessageText.text += $"\nWins: {playerWins} - {opponentWins}";

                }

                // update players elo
                if (opponentElo >= 0)
                {
                    var (newElo, _) = EloCalculator.CalculateElo(PlayerPrefs.GetInt("Elo", 100), opponentElo, 0.5f);
                    PlayerPrefs.SetInt("Elo", newElo);
                }
            }
            OngoingChallengeManagerScript.Instance.EvaluateChallengeAndSetText();
            return;
        }

        if (isLocal)
        {
            if (opponentScore < playerScore)
            {
                gameResultText.text = "Player 1 Won!";
                IncrementPlayerPref("localWin");
            }
            else if (opponentScore > playerScore)
            {
                gameResultText.text = "Player 2 Won!";
                IncrementPlayerPref("localLoss");
            }
            else
            {
                gameResultText.text = "Tie";
                IncrementPlayerPref("localTie");
                return;
            }
            gameResultHighscoreMessageText.text = "They won by " + System.Math.Abs(scoreDifference) + " points.";
            OngoingChallengeManagerScript.Instance.EvaluateChallengeAndSetText();
            return;
        }

        string[] highscoresPlayerPrefsKeys = { "easyHighscore", "mediumHighscore", "hardHighscore" };
        string[] winPlayerPrefsKeys = { "easyWin", "mediumWin", "hardWin" };
        string[] lossPlayerPrefsKeys = { "easyLoss", "mediumLoss", "hardLoss" };
        string[] tiePlayerPrefsKeys = { "easyTie", "mediumTie", "hardTie" };

        if (playerScore > opponentScore)
        {
            gameResultText.text = "You Win!";
            gameResultHighscoreMessageText.text = "You won by " + scoreDifference + " points.";

            if (scoreDifference > PlayerPrefs.GetInt(highscoresPlayerPrefsKeys[difficulty]))
            {
                gameResultHighscoreMessageText.text += "\nNew Highscore!";
                OverwriteHighscore(scoreDifference, difficulty);
            }

            IncrementPlayerPref(winPlayerPrefsKeys[difficulty]);
            gameResultHighscoreMessageText.text += dailyChallenge.EvaluateChallenge(difficulty, scoreDifference, 0);
        }
        else if (playerScore < opponentScore)
        {
            gameResultText.text = "You Lose";
            gameResultHighscoreMessageText.text = "";
            IncrementPlayerPref(lossPlayerPrefsKeys[difficulty]);
        }
        else
        {
            gameResultText.text = "Tie";
            gameResultHighscoreMessageText.text = "";
            IncrementPlayerPref(tiePlayerPrefsKeys[difficulty]);
        }
        OngoingChallengeManagerScript.Instance.EvaluateChallengeAndSetText();
    }

    // write highscore to file and profile
    public void OverwriteHighscore(int newHighscore, int difficulty)
    {
        string[] highscoresPlayerPrefsKeys = { "easyHighscore", "mediumHighscore", "hardHighscore" };

        PlayerPrefs.SetInt(highscoresPlayerPrefsKeys[difficulty], newHighscore);
        UpdateProfileText();
    }

    public void IncrementPlayerPref(string key)
    {
        PlayerPrefs.SetInt(key, (PlayerPrefs.GetInt(key) + 1));
    }

    // write highscores from player prefs to profile
    public void UpdateProfileText()
    {
        profilePopupText.text =
            "Easy: " + PlayerPrefs.GetInt("easyHighscore") +
            "\nMedium: " + PlayerPrefs.GetInt("mediumHighscore") +
            "\nHard: " + PlayerPrefs.GetInt("hardHighscore");
    }

    // unlock the unlockables based on player highscores
    public void UpdateLocks(bool setAlert = false)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("customLock"))
        {
            var CUS = go.GetComponent<CustomUnlockScript>();
            if (CUS == null) { break; }
            var id = CUS.Unlock();
            if (id > 0)
            {
                PuckSkinManager.Instance.UnlockPuckID(id, setAlert);
                PuckSkinManager.Instance.UnlockPuckID(id * -1, setAlert);
            }
        }
        puckAlert.SetActive(PlayerPrefs.GetInt("ShowNewSkinAlert", 0) == 1);
    }

    // profile reset highscores button
    int[] highscoreBackup = new int[] { 0, 0, 0 };
    public void ResetHighscores()
    {
        highscoreBackup[0] = PlayerPrefs.GetInt("easyHighscore");
        highscoreBackup[1] = PlayerPrefs.GetInt("mediumHighscore");
        highscoreBackup[2] = PlayerPrefs.GetInt("hardHighscore");
        PlayerPrefs.SetInt("easyHighscore", 0);
        PlayerPrefs.SetInt("mediumHighscore", 0);
        PlayerPrefs.SetInt("hardHighscore", 0);
        UpdateProfileText();
    }

    public void UndoResetHighscores()
    {
        PlayerPrefs.SetInt("easyHighscore", highscoreBackup[0]);
        PlayerPrefs.SetInt("mediumHighscore", highscoreBackup[1]);
        PlayerPrefs.SetInt("hardHighscore", highscoreBackup[2]);
        UpdateProfileText();
    }

    // shows up when you click something locked
    public void SetErrorMessage(string msg)
    {
        // modifiers here are for puck select screen
        if (msg.Contains("Unlocked") || msg.Contains("Rewarded"))
        {
            errorMessage.color = new Color(0.4862745f, 0.7725491f, 0.4627451f);
        }
        else
        {
            errorMessage.color = new Color(0.9490197f, 0.4235294f, 0.3098039f);
        }
        if (msg.Contains("{combined}"))
        {
            msg = msg.Replace("{combined}", (PlayerPrefs.GetInt("easyHighscore") + PlayerPrefs.GetInt("mediumHighscore") + PlayerPrefs.GetInt("hardHighscore")).ToString());
        }
        // the REAL point of this function
        errorMessage.text = msg;
    }
  
    // switch UI to new screen
    public void ChangeUI(GameObject newUI)
    {
        previousActiveUI = activeUI;
        activeUI.SetActive(false);
        newUI.SetActive(true);
        activeUI = newUI;
        SetErrorMessage("");
        UpdateLocks(true); // for non-puck locks (difficulty locks)
        if (newUI == gameHud)
        {
            ResetHUD();
        }
        else if (newUI == titleScreen)
        {
            // blink puck screen for unlocks
            puckScreen.SetActive(true);
            UpdateLocks(true);
            puckScreen.SetActive(false);
            playerWins = 0;
            opponentWins = 0;
        }
        else if (newUI == rewardsScreen)
        {
            StreakManager.Instance.SetText();
        }
        // sadly, this has to be here I think
        else if (newUI == questsScreen)
        {
            DailyChallengeManagerScript.Instance.SetText();
            OngoingChallengeManagerScript.Instance.EvaluateChallengeAndSetText();
        }
        titleScreenBackground.SetActive(newUI != gameHud && newUI != gameResultScreen);
        table.SetActive(newUI == gameHud || newUI == gameResultScreen);
        slider.SetActive(newUI == gameHud || newUI == gameResultScreen || newUI == titleScreen);
        ApplyDarkMode();
        if (newUI != gameHud)
        {
            newUI.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
            LeanTween.cancel(newUI);
            LeanTween.scale(newUI, new Vector3(1f, 1f, 1f), 0.1f).setEase(LeanTweenType.easeOutBack).setDelay(0.01f);
        }
    }

    // handle android back button / esc key
    void HandleBackButton()
    {
        // don't allow android back button if...
        if (
            activeUI == gameHud || // we're in game
            previousActiveUI == gameHud || // trying to go back to game HUD
            previousActiveUI == gameResultScreen || // trying to go back to game result screen
            activeUI == plinkoScreen || // we're on the plinko screen
            activeUI == waitingScreen // we're joining/hosting an online game
            )
        {
            return;
        }
        ChangeUI(previousActiveUI);
    }

    // reset in-game HUD
    public void ResetHUD()
    {
        if (logic.gameIsRunning) { tutorialMenu.SetActive(logic.tutorialActive); };
        gameResultText.text = "";
        playerScoreText.text = 0.ToString();
        opponentScoreText.text = 0.ToString();
        playerPuckCountText.text = 5.ToString();
        opponentPuckCountText.text = 5.ToString();
        playerWinsObject.SetActive(playerWins > 0 || opponentWins > 0);
        opponentWinsObject.SetActive(playerWins > 0 || opponentWins > 0);
        ExitConfirmation.SetActive(false);
        RestartConfirmation.SetActive(false);

        // set HUD username text
        string username = PlayerAuthentication.Instance.GetUsername();
        if (!string.IsNullOrEmpty(username))
        {
            playerUsernameText.text = username;
        }
        else
        {
            playerUsernameText.text = "you";
        }

        if (LogicScript.Instance.gameIsRunning && !LogicScript.Instance.IsLocal)
        {
            string[] diffStrings = { "easy", "medium", "hard" };
            opponentUsernameText.text = diffStrings[LogicScript.Instance.GetDifficulty()] + " CPU";
        }
        else if (ClientLogicScript.Instance.isRunning || LogicScript.Instance.IsLocal)
        {
            opponentUsernameText.text = "opponent";
        }
    }

    public void ResetWaitingScreen(string waitingTextInput)
    {
        waitingText.text = waitingTextInput;
        lobbyCodeText.text = "";
        waitingGif.SetActive(true);
        readyButton.SetActive(false);
        waitingBackButton.SetActive(true);
    }

    public void SetReButtons(bool boolean)
    {
        restartButton.SetActive(boolean);
        rematchButton.SetActive(boolean);
    }

    public void SetPlayerPuckIcon(Sprite sprite, bool enableAnimation = false)
    {
        playerPuckIcon.GetComponent<Image>().sprite = sprite;
        activePuckIcon.GetComponent<Image>().sprite = sprite;
        playerPuckIconAnimation.SetActive(enableAnimation);
        activePuckIconAnimation.SetActive(enableAnimation);
    }

    public void SetOpponentPuckIcon(Sprite sprite, bool enableAnimation = false)
    {
        opponentPuckIcon.GetComponent<Image>().sprite = sprite;
        opponentPuckIconAnimation.SetActive(enableAnimation);
    }
    

    // there needs to be a delay on the ready button to prevent errors with async stuff
    public void EnableReadyButton()
    {
        enabledReadyButton = true;
        cooldownTime = 1f;
        waitingBackButton.SetActive(false);
        // the waiting text & gif also update after cooldown to prevent confusion
    }

    public void DisableReadyButton()
    {
        enabledReadyButton = false;
        readyButton.SetActive(false);
    }

    public void FailedToFindMatch()
    {
        waitingText.text = "Failed to find Opponent";
        waitingGif.SetActive(false);
    }

    public void Toggle(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    public void SetDarkMode(bool booly)
    {
        darkMode = booly;
        ApplyDarkMode();
        PlayerPrefs.SetInt("darkMode", darkMode ? 1 : 0);
    }

    public void ApplyDarkMode(GameObject parentObject = null)
    {
        // hacky way of enabling darkmode for disabled objects (run this with parameter onenable)
        if (parentObject == null) { parentObject = activeUI; }

        if (parentObject == gameHud)
        {
            table.GetComponent<SpriteRenderer>().sprite = darkMode ? tableDark : tableLight;
        }
        else if (parentObject.tag == "mainMenu")
        {
            titleScreenBackground.GetComponent<Image>().sprite = darkMode ? titleScreenBackgroundDark : titleScreenBackgroundLight;
            parentObject.GetComponent<Image>().sprite = darkMode ? titleScreenDark : titleScreenLight;
        }
        // force dark mode for pack open because it looks better with particle effects (lame i know)
        else if (parentObject == packOpenScreen)
        {
            titleScreenBackground.GetComponent<Image>().sprite = titleScreenBackgroundDark;
            parentObject.GetComponent<Image>().sprite = titleScreenDark;
        }
        // swap text color to white for all children TMP objects with the blackText tag
        foreach (TMP_Text text in parentObject.GetComponentsInChildren<TMP_Text>())
        {
            if (text.tag == "blackText")
            {
                var alpha = text.alpha;
                text.color = darkMode ? Color.white : Color.black;
                text.alpha = alpha;
            }
        }
        // swap text color to white for all children Text objects with the blackText tag
        foreach (Text text in parentObject.GetComponentsInChildren<Text>())
        {
            if (text.tag == "blackText")
            {
                text.color = darkMode ? Color.white : Color.black;
            }
        }
        // swap color to white for all children sprite renderer objects with the blackText tag
        foreach (SpriteRenderer sr in parentObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.tag == "blackText")
            {
                sr.color = darkMode ? Color.white : Color.black;
            }
        }
        // swap color to white for all children image objects with the blackText tag
        foreach (Image img in parentObject.GetComponentsInChildren<Image>())
        {
            if (img.tag == "blackText")
            {
                img.color = darkMode ? Color.white : Color.black;
            }
        }
    }

    // helper for experimental mode button in settings menu
    public void ToggleExperimentalFeatures(bool enabled)
    {
        PlayerPrefs.SetInt("experimental", enabled ? 1 : 0);
    }

    // helper for debug mode button
    public int debugMode = 0;
    public void DebugMode()
    {
        debugMode++;
        if (debugMode == 10) {
            ScreenLog.Instance.gameObject.SetActive(true);
            if (PlayerPrefs.GetInt("tutorialCompleted") == 0)
            {
                PlayerPrefs.SetInt("tutorialCompleted", 1);
            }
            if (PlayerPrefs.GetInt("easyWin") == 0)
            {
                PlayerPrefs.SetInt("easyWin", 1);
            }
            if (PlayerPrefs.GetInt("easyHighscore") == 0)
            {
                PlayerPrefs.SetInt("easyHighscore", 1);
            }
            if (PlayerPrefs.GetInt("mediumWin") == 0)
            {
                PlayerPrefs.SetInt("mediumWin", 1);
            }
            if (PlayerPrefs.GetInt("mediumHighscore") == 0)
            {
                PlayerPrefs.SetInt("mediumHighscore", 1);
            }
            PlayerPrefs.SetInt("debug", 1);
            Debug.Log("Started up logging.");
            debugMode = 0;
        }
        else if (debugMode == 5)
        {
            ScreenLog.Instance.gameObject.SetActive(false);
            PlayerPrefs.SetInt("debug", 0);
        }
    }

    public void UpdateShotDebugText(float angleParameter, float powerParameter, float spinParameter = 50)
    {
#if (UNITY_EDITOR)
        AngleDebugText.text = angleParameter.ToString();
        PowerDebugText.text = powerParameter.ToString();
        SpinDebugText.text = spinParameter.ToString();
#endif
    }
}
