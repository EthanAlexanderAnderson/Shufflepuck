using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;

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

    // Lobby
    public TMP_Text lobbyCodeText;
    public GameObject waitingGif;
    public TMP_Text waitingText;

    // HUD
    public Text turnText;

    public Text playerPuckCountText;
    public Text opponentPuckCountText;

    public Text playerScoreText;
    public Text opponentScoreText;

    // result
    public Text gameResultText;
    public Text gameResultHighscoreMessageText;

    // local
    public GameObject activeUI;

    public string TurnText
    {
        get => turnText.text;
        set
        {
            turnText.text = value;
        }
    }

    public void SetTurnTextActive()
    {
        turnText.gameObject.SetActive(true);
    }

    public void PostShotUpdate(int playerPuckCount, int opponentPuckCount)
    {
        turnText.gameObject.SetActive(false);
        playerPuckCountText.text = playerPuckCount.ToString();
        opponentPuckCountText.text = opponentPuckCount.ToString();
    }
    public void UpdateScores(int playerScore, int opponentScore)
    {
        playerScoreText.text = playerScore.ToString();
        opponentScoreText.text = opponentScore.ToString();
    }

    public void UpdateGameResult(int playerScore, int opponentScore, int difficulty)
    {
        int scoreDifference = playerScore - opponentScore;
        string[] highscoresPlayerPrefsKeys = { "easyHighscore", "mediumHighscore", "hardHighscore" };

        if (playerScore > opponentScore)
        {
            gameResultText.text = "You Win!";
            if (scoreDifference > PlayerPrefs.GetInt(highscoresPlayerPrefsKeys[difficulty]))
            {
                gameResultHighscoreMessageText.text = "You won by " + scoreDifference + " points.\nNew Highscore!";
                OverwriteHighscore(scoreDifference, difficulty);
            }
        }
        else if (playerScore < opponentScore)
        {
            gameResultText.text = "You Lose";
            gameResultHighscoreMessageText.text = "";
        }
        else
        {
            gameResultText.text = "Tie";
            gameResultHighscoreMessageText.text = "";
        }

    }

    // write highscore to file and profile
    public void OverwriteHighscore(int newHighscore, int difficulty)
    {
        string[] highscoresPlayerPrefsKeys = { "easyHighscore", "mediumHighscore", "hardHighscore" };

        PlayerPrefs.SetInt(highscoresPlayerPrefsKeys[difficulty], newHighscore);
        UpdateProfileText();
    }

    // write highscores from player prefs to profile
    public void UpdateProfileText()
    {
        profilePopupText.text =
            "Highscores: \n" +
            "\nEasy: " + PlayerPrefs.GetInt("easyHighscore") +
            "\nMedium: " + PlayerPrefs.GetInt("mediumHighscore") +
            "\nHard: " + PlayerPrefs.GetInt("hardHighscore");
    }

    // unlock the unlockables based on player highscores
    public void UpdateLocks()
    {
        string[] highscoresPlayerPrefsKeys = { "easyHighscore", "mediumHighscore", "hardHighscore" };
        string[] difficultyLocksPlayerPrefsKeys = { "easyLock", "mediumLock", "hardLock" };
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

            combinedHighscore += iHighscore;
        }

        // combined highscore locks
        string[] combinedHighscoreLocksPlayerPrefsKeys = { "combined20Lock", "combined22Lock" };
        for (int i = 20; i <= combinedHighscore; i += 2)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(combinedHighscoreLocksPlayerPrefsKeys[(i - 20) / 2]))
            {
                go.SetActive(false);
            }
        }
    }

    // shows up when you click something locked
    public void SetErrorMessage(string msg)
    {
        errorMessage.text = msg;
    }
  

    public void ChangeUI(GameObject newUI)
    {
        activeUI.SetActive(false);
        newUI.SetActive(true);
        activeUI = newUI;
        SetErrorMessage("");
        UpdateLocks();
        ResetHUD();
    }

    public void ResetHUD()
    {
        gameResultText.text = "";
        playerScoreText.text = 0.ToString();
        opponentScoreText.text = 0.ToString();
        playerPuckCountText.text = 5.ToString();
        opponentPuckCountText.text = 5.ToString();
    }

    public void ResetWaitingScreen(string waitingTextInput = "Searching for Opponent...")
    {
        waitingText.text = waitingTextInput;
        lobbyCodeText.text = "";
        waitingGif.SetActive(true);
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

    public void Host()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void Join()
    {
        NetworkManager.Singleton.StartClient();

    }

    // there needs to be a delay on the ready button to prevent errors with async stuff
    public void EnableReadyButton()
    {
        waitingText.text = "0/2 Players Ready";
        enabledReadyButton = true;
        CooldownTime = 1.0f;
        waitingGif.SetActive(false);
    }

    float CooldownTime;
    bool enabledReadyButton;

    private void Update()
    {
        CooldownTime -= Time.deltaTime;
        if (enabledReadyButton && CooldownTime <= 0f)
        {
            readyButton.SetActive(true);
            enabledReadyButton = false;
        }
    }

    public void FailedToFindMatch()
    {
        waitingText.text = "Failed to find Opponent";
        waitingGif.SetActive(false);
    }

    public void Toggle(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject);
    }
}
