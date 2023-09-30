using UnityEngine;
using UnityEngine.UI;
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

    // HUD
    public Text turnText;

    public Text playerPuckCountText;
    public Text opponentPuckCountText;

    public Text playerScoreText;
    public Text opponentScoreText;

    // result
    public Text gameResultText;

    // local
    public GameObject activeUI;

    public string TurnText { get; set; }

    /*public void SwapTurn(bool swappingToPlayersTurn, bool isMultiplayer)
    {
        turnText.gameObject.SetActive(true);
        if (swappingToPlayersTurn)
        {
            turnText.text = "Your Turn";
        }
        else
        {
            turnText.text = isMultiplayer ? "Opponent's Turn" : "CPU's Turn";
        }
    }*/

    public void PostShotUpdate(int playerPuckCount, int opponentPuckCount)
    {
        turnText.gameObject.SetActive(false);
        playerPuckCountText.text = playerPuckCount.ToString();
        opponentPuckCountText.text = opponentPuckCount.ToString();
    }

    public void UpdateGameResult(int playerScore, int opponentScore)
    {
        if (playerScore > opponentScore)
        {
            gameResultText.text = "You Win!";
        }
        else if (playerScore < opponentScore)
        {
            gameResultText.text = "You Lose";
        }
        else
        {
            gameResultText.text = "Tie";
        }
        //gameResultScreen.SetActive(true);
    }

    public void UpdateScores(int playerScore, int opponentScore)
    {
        playerScoreText.text = playerScore.ToString();
        opponentScoreText.text = opponentScore.ToString();
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
        string[] locksPlayerPrefsKeys = { "easyLock", "mediumLock", "hardLock" };

        // for each different highscore, if it is greater than zero, unlock all objects of cooresponding type
        for (int i = 0; i < highscoresPlayerPrefsKeys.Length; i++)
        {
            if (PlayerPrefs.GetInt(highscoresPlayerPrefsKeys[i]) > 0)
                foreach (GameObject go in GameObject.FindGameObjectsWithTag(locksPlayerPrefsKeys[i]))
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

    // write highscore to file and profile
    public void OverwriteHighscore(int newHighscore, int difficulty)
    {
        string[] highscoresPlayerPrefsKeys = { "easyHighscore", "mediumHighscore", "hardHighscore" };
        string activeHighscorePlayerPrefsKey = highscoresPlayerPrefsKeys[difficulty];

        if (newHighscore > PlayerPrefs.GetInt(activeHighscorePlayerPrefsKey))
        {
            PlayerPrefs.SetInt(activeHighscorePlayerPrefsKey, newHighscore);
            UpdateProfileText();
        }
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
/*
    public void ChangeUIHelper(string key)
    {
        ChangeUI(key switch
        {
            "title" => titleScreen,
            "result" => gameResultScreen,
            "HUD" => gameHud,
        });
    }
*/
    public void ResetHUD()
    {
        gameResultText.text = "";
        playerScoreText.text = 0.ToString();
        opponentScoreText.text = 0.ToString();
        playerPuckCountText.text = 5.ToString();
        opponentPuckCountText.text = 5.ToString();
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
}
