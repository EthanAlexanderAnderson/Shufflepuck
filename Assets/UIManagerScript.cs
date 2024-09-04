// Most stuff involved with chaning or managing UI elements is done here
// Methods are self explanatory for the most part

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManagerScript : MonoBehaviour
{
    // UI Parent Objects
    public GameObject titleScreen;
    public GameObject gameHud;
    public GameObject gameResultScreen;

    // title
    public GameObject playerPuckIcon;
    public GameObject opponentPuckIcon;
    public GameObject activePuckIcon;
    public TMP_Text errorMessage;
    public TMP_Text profilePopupText;
    public GameObject readyButton;
    public GameObject screenLog;
    public GameObject titleScreenBackground;
    public GameObject FPS30Button;
    public GameObject puckAlert;

    // Lobby
    public TMP_Text lobbyCodeText;
    public GameObject waitingGif;
    public TMP_Text waitingText;
    public GameObject waitingBackButton;

    // HUD
    public Text turnText;

    public Text playerPuckCountText;
    public Text opponentPuckCountText;

    public Text playerScoreText;
    public Text opponentScoreText;

    public Text shotClockText;

    public Text AngleDebugText;
    public Text PowerDebugText;
    public Text SpinDebugText;

    public GameObject restartButton;

    // result
    public Text gameResultText;
    public Text gameResultHighscoreMessageText;

    public GameObject rematchButton;

    // local
    public GameObject activeUI;
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

    // TODO: ideally unlink logic eventually
    private LogicScript logic;
    private SoundManagerScript sound;

    private void Start()
    {
        logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();
        sound = GameObject.FindGameObjectWithTag("sound").GetComponent<SoundManagerScript>();
        puckAlert.SetActive(PlayerPrefs.GetInt("ShowNewSkinAlert", 0) == 1);
    }

    public void PostShotUpdate(int playerPuckCount, int opponentPuckCount)
    {
        playerPuckCountText.text = playerPuckCount.ToString();
        opponentPuckCountText.text = opponentPuckCount.ToString();
    }
    public void UpdateScores(int playerScore, int opponentScore)
    {
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
                logic.UnlockPuckID(IDs[i]);
                logic.UnlockPuckID(IDs[i]*-1);
            combinedHighscore += iHighscore;
        }

        // combined highscore locks
        for (int i = 10; i <= combinedHighscore; i += 2)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(i + "CombinedLock"))
            {
                go.SetActive(false);
            }
            logic.UnlockPuckID(12 + ((i-10) / 2));
            logic.UnlockPuckID((12 + ((i-10) / 2)) * -1);
        }

        // custom locks
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("customLock"))
        {
            var id = go.GetComponent<CustomUnlockScript>().Unlock();
            if (id > 0)
            {
                logic.UnlockPuckID(id);
                logic.UnlockPuckID(id * -1);
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
        cooldownTime = 2.0f;
        waitingBackButton.SetActive(false);
        // the waiting text & gif also update after cooldown to prevent confusion
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

    public void EnableScreenLog()
    {
        screenLog.SetActive(true);
        Debug.Log("Started up logging.");
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
