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

    // UI Parent Objects
    public GameObject titleScreen;
    public GameObject gameHud;
    public GameObject gameResultScreen;
    public GameObject customizeScreen;

    // title
    [SerializeField] private GameObject playerPuckIcon;
    [SerializeField] private GameObject opponentPuckIcon;
    [SerializeField] private GameObject activePuckIcon;
    [SerializeField] private TMP_Text errorMessage;
    [SerializeField] private TMP_Text profilePopupText;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject screenLog;
    [SerializeField] private GameObject titleScreenBackground;
    [SerializeField] private GameObject FPS30Button;
    [SerializeField] private GameObject puckAlert;

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

    // HUD
    [SerializeField] private Text turnText;

    public Text playerPuckCountText;
    public Text opponentPuckCountText;

    [SerializeField] private Text playerScoreText;
    [SerializeField] private Text opponentScoreText;

    public Text shotClockText;

    [SerializeField] private Text AngleDebugText;
    [SerializeField] private Text PowerDebugText;
    [SerializeField] private Text SpinDebugText;

    [SerializeField] private GameObject restartButton;

    // result
    [SerializeField] private Text gameResultText;
    [SerializeField] private Text gameResultHighscoreMessageText;

    [SerializeField] private GameObject rematchButton;

    // local
    [SerializeField] private GameObject activeUI;
    private GameObject previousActiveUI;

    [SerializeField] private TMP_Text wallText;

    public string TurnText
    {
        get => turnText.text;
        set
        {
            turnText.text = value;
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        tutorialPages = new GameObject[] { page0, page1, page2, page3, page4, page5, page6, page7, page8 };
    }

    private void Start()
    {
        logic = LogicScript.Instance;
        sound = SoundManagerScript.Instance;
        puckAlert.SetActive(PlayerPrefs.GetInt("ShowNewSkinAlert", 0) == 1);
    }

    float cooldownTime;
    bool enabledReadyButton;
    private void Update()
    {
        cooldownTime -= Time.deltaTime;
        if (enabledReadyButton && cooldownTime <= 0f)
        {
            readyButton.SetActive(true);
            waitingText.text = "0/2 Players Ready";
            waitingGif.SetActive(false);
            enabledReadyButton = false;
            waitingBackButton.SetActive(true);
        }
        // Check if the android back button (Escape key) is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Handle the back button press
            HandleBackButton();
        }

        if (logic.tutorialActive) { tutorialMenu.SetActive(true);  }

        if ( Input.GetMouseButtonDown(0) && tutorialMenu.activeInHierarchy && iPage != 6)
        {
            AdvanceTutorial();
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

    public void PostShotUpdate(int playerPuckCount, int opponentPuckCount)
    {
        playerPuckCountText.text = playerPuckCount.ToString();
        opponentPuckCountText.text = opponentPuckCount.ToString();
    }
    public void UpdateScores(int playerScore, int opponentScore)
    {
        if (!playerScoreText || !opponentScoreText) { return; }
        playerScoreText.text = playerScore.ToString();
        opponentScoreText.text = opponentScore.ToString();
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

    public void UpdateGameResult(int playerScore, int opponentScore, int difficulty, bool isLocal, bool isOnline = false)
    {
        // for online mode
        if (playerScore == -1) playerScore = Int32.Parse(playerScoreText.text);
        if (opponentScore == -1) opponentScore = Int32.Parse(opponentScoreText.text);

        int scoreDifference = playerScore - opponentScore;

        // generic win/loss/tie playerpref 
        if (playerScore > opponentScore)
        {
            IncrementPlayerPref("win");
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
            if (opponentScore < playerScore)
            {
                gameResultText.text = "You Win!";
                gameResultHighscoreMessageText.text = "You won by " + System.Math.Abs(scoreDifference) + " points.";
                IncrementPlayerPref("onlineWin");
            }
            else if (opponentScore > playerScore)
            {
                gameResultText.text = "You Lose";
                gameResultHighscoreMessageText.text = "They won by " + System.Math.Abs(scoreDifference) + " points.";
                IncrementPlayerPref("onlineLoss");
            }
            else
            {
                gameResultText.text = "Tie";
                gameResultHighscoreMessageText.text = "";
                IncrementPlayerPref("onlineTie");
            }
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
            if (logic.powerupsAreEnabled) { return; }
            if (scoreDifference > PlayerPrefs.GetInt(highscoresPlayerPrefsKeys[difficulty]) && !logic.powerupsAreEnabled)
            {
                gameResultHighscoreMessageText.text = gameResultHighscoreMessageText.text + "\nNew Highscore!";
                OverwriteHighscore(scoreDifference, difficulty);
            }
            IncrementPlayerPref(winPlayerPrefsKeys[difficulty]);
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
            "Highscores: \n\n" +
            "\nEasy: " + PlayerPrefs.GetInt("easyHighscore") +
            "\nMedium: " + PlayerPrefs.GetInt("mediumHighscore") +
            "\nHard: " + PlayerPrefs.GetInt("hardHighscore");
    }

    // unlock the unlockables based on player highscores
    public void UpdateLocks()
    {
        string[] highscoresPlayerPrefsKeys = { "easyHighscore", "mediumHighscore", "hardHighscore" };
        string[] difficultyLocksPlayerPrefsKeys = { "easyLock", "mediumLock", "hardLock" };
        int[] IDs = { 9, 10, 11 };
        int combinedHighscore = 0;

        // for each different highscore, if it is greater than zero, unlock all objects of cooresponding type
        for (int i = 0; i < highscoresPlayerPrefsKeys.Length; i++)
        {
            int iHighscore = PlayerPrefs.GetInt(highscoresPlayerPrefsKeys[i]);

            if (iHighscore > 0)
                foreach (GameObject go in GameObject.FindGameObjectsWithTag(difficultyLocksPlayerPrefsKeys[i]))
                {
                    go.SetActive(false);
                }
                PuckSkinManager.Instance.UnlockPuckID(IDs[i]);
                PuckSkinManager.Instance.UnlockPuckID(IDs[i]*-1);
            combinedHighscore += iHighscore;
        }

        // combined highscore locks
        for (int i = 10; i <= combinedHighscore; i += 2)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(i + "CombinedLock"))
            {
                go.SetActive(false);
            }
            PuckSkinManager.Instance.UnlockPuckID(12 + ((i-10) / 2));
            PuckSkinManager.Instance.UnlockPuckID((12 + ((i-10) / 2)) * -1);
        }

        // custom locks
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("customLock"))
        {
            var id = go.GetComponent<CustomUnlockScript>().Unlock();
            if (id > 0)
            {
                PuckSkinManager.Instance.UnlockPuckID(id);
                PuckSkinManager.Instance.UnlockPuckID(id * -1);
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
        UpdateLocks();
        if (newUI == gameHud)
        {
            ResetHUD();
            titleScreenBackground.SetActive(false);
        }
        else if (newUI == gameResultScreen)
        {
            titleScreenBackground.SetActive(false);
        }
        else if (newUI == titleScreen)
        {
            titleScreenBackground.SetActive(true);
            customizeScreen.SetActive(true);
            UpdateLocks();
            customizeScreen.SetActive(false);
        }
        else
        {
            titleScreenBackground.SetActive(true);
        }
    }

    // handle android back button / esc key
    void HandleBackButton()
    {
        if (previousActiveUI != gameHud)
        {
            ChangeUI(previousActiveUI);
        }
    }
    // reset in-game HUD
    public void ResetHUD()
    {
        gameResultText.text = "";
        playerScoreText.text = 0.ToString();
        opponentScoreText.text = 0.ToString();
        playerPuckCountText.text = 5.ToString();
        opponentPuckCountText.text = 5.ToString();
    }

    public void ResetWaitingScreen(string waitingTextInput)
    {
        waitingText.text = waitingTextInput;
        lobbyCodeText.text = "";
        waitingGif.SetActive(true);
    }

    public void SetReButtons(bool boolean)
    {
        restartButton.SetActive(boolean);
        rematchButton.SetActive(boolean);
    }

    public void SetPlayerPuckIcon(Sprite sprite)
    {
        playerPuckIcon.GetComponent<Image>().sprite = sprite;
        activePuckIcon.GetComponent<Image>().sprite = sprite;
    }

    public void SetOpponentPuckIcon(Sprite sprite)
    {
        opponentPuckIcon.GetComponent<Image>().sprite = sprite;
    }
    

    // there needs to be a delay on the ready button to prevent errors with async stuff
    public void EnableReadyButton()
    {
        enabledReadyButton = true;
        cooldownTime = 3.0f;
        waitingBackButton.SetActive(false);
        // the waiting text & gif also update after cooldown to prevent confusion
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

    // helper for debug mode button
    int debugMode = 0;
    public void DebugMode()
    {
        debugMode++;
        if (debugMode == 10) {
            screenLog.SetActive(true);
            Debug.Log("Started up logging.");
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
