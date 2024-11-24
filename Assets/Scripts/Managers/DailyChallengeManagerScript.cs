using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyChallengeManagerScript : MonoBehaviour
{
    // self
    public static DailyChallengeManagerScript Instance;
    // dependencies
    private LevelManager levelManager;
    private SoundManagerScript sound;

    [SerializeField] private GameObject titleScreen;
    //
    [SerializeField] private TMP_Text countdownText;

    [SerializeField] private TMP_Text challenge1Text;
    [SerializeField] private TMP_Text challenge2Text;

    [SerializeField] private TMP_Text reward1Text;
    [SerializeField] private TMP_Text reward2Text;

    [SerializeField] private Button claim1;
    [SerializeField] private Button claim2;

    private string[] easyChallengeText = { "Claimed", "Beat the easy CPU", "Beat the medium CPU", "Beat the easy CPU by 3 or more points", "Beat the medium CPU by 3 or more points", "Beat the easy CPU by 5 or more points" };
    private int[] easyChallengeReward = { 0, 40, 60, 60, 80, 80 };
    private int[,] easyChallengeCondition = { { 0, 0, 0 }, { 0, 1, 0 }, { 1, 1, 0 }, { 0, 3, 0 }, { 1, 3, 0 }, { 0, 5, 0 } }; // {difficulty, score difference, isOnline}
    private string[] hardChallengeText = { "Claimed", "Beat the hard CPU", "Beat the hard CPU by 3 or more points", "Beat the medium CPU by 5 or more points", "Beat the hard CPU by 5 or more points", "Win an online match" };
    private int[] hardChallengeReward = { 0, 80, 100, 100, 120, 200 };
    private int[,] hardChallengeCondition = { { 0, 0, 0 }, { 2, 1, 0 }, { 2, 3, 0 }, { 1, 5, 0 }, { 2, 5, 0 }, { 2, 1, 1 } }; // {difficulty, score difference, isOnline}

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    void Start()
    {
        levelManager = LevelManager.Instance;
        sound = SoundManagerScript.Instance;
    }

    private void OnEnable()
    {
        CheckForNewDailyChallenge();
    }

    public void SetText()
    {
        CheckForNewDailyChallenge();
        int DC1 = PlayerPrefs.GetInt("DailyChallenge1", 0);
        int DC2 = PlayerPrefs.GetInt("DailyChallenge2", 0);
        // if the challenge is completed, the value is negative
        claim1.interactable = DC1 < 0;
        claim2.interactable = DC2 < 0;
        // assert the challenge ID is within range, prevent index error
        if (DC1 >= easyChallengeReward.Length || DC1 <= (easyChallengeReward.Length * -1))
        {
            DC1 = 0;
        }
        if (DC2 >= easyChallengeReward.Length || DC2 <= (easyChallengeReward.Length * -1))
        {
            DC2 = 0;
        }

        challenge1Text.text = easyChallengeText[Mathf.Abs(DC1)];
        challenge2Text.text = hardChallengeText[Mathf.Abs(DC2)];
        reward1Text.text = easyChallengeReward[Mathf.Abs(DC1)].ToString() + " XP";
        reward2Text.text = hardChallengeReward[Mathf.Abs(DC2)].ToString() + " XP";
        levelManager.SetText();
    }

    void Update()
    {
        UpdateCountdown();
    }
    private void UpdateCountdown()
    {
        // Calculate the time remaining until midnight
        DateTime now = DateTime.Now;
        DateTime tomorrow = DateTime.Today.AddDays(1);
        TimeSpan timeUntilMidnight = tomorrow - now;

        if (countdownText != null)
        {
            // Update countdown text
            countdownText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                timeUntilMidnight.Hours, timeUntilMidnight.Minutes, timeUntilMidnight.Seconds);
        }

        // Check if the countdown has reached zero
        if (timeUntilMidnight.TotalSeconds <= 0)
        {
            AssignNewChallenge();
        }
    }

    private void CheckForNewDailyChallenge()
    {
        // Get the last saved date or use a default if it's the first time
        string lastSavedDate = PlayerPrefs.GetString("LastChallengeDate", string.Empty);
        DateTime lastChallengeDate;

        if (DateTime.TryParse(lastSavedDate, out lastChallengeDate))
        {
            // Compare the last saved date to today's date
            if (lastChallengeDate.Date != DateTime.Today)
            {
                AssignNewChallenge();
            }
            else
            {
                Debug.Log("Today's challenge is already assigned. " + PlayerPrefs.GetInt("DailyChallenge1", 0) + " " + PlayerPrefs.GetInt("DailyChallenge2", 0));
            }
        }
        else // no date ever written
        {
            AssignNewChallenge();
        }
    }

    private void AssignNewChallenge()
    {
        PlayerPrefs.SetString("LastChallengeDate", DateTime.Today.ToString("yyyy-MM-dd"));

        // only overwrite the challenge if it's not already completed (not a negative int id)
        if (PlayerPrefs.GetInt("DailyChallenge1", 0) >= 0)
        {
            PlayerPrefs.SetInt("DailyChallenge1", UnityEngine.Random.Range(1, easyChallengeText.Length));
            // early game override
            if (PlayerPrefs.GetInt("easyHighscore", 0) == 0) { PlayerPrefs.SetInt("DailyChallenge1", 1); }
        }
        if (PlayerPrefs.GetInt("DailyChallenge2", 0) >= 0)
        {
            PlayerPrefs.SetInt("DailyChallenge2", UnityEngine.Random.Range(1, hardChallengeText.Length));
            // early game override
            if (PlayerPrefs.GetInt("hardHighscore", 0) == 0) { PlayerPrefs.SetInt("DailyChallenge2", 1); }
        }

        Debug.Log($"New daily challenges assigned. " + PlayerPrefs.GetInt("DailyChallenge1", 0) + " " + PlayerPrefs.GetInt("DailyChallenge2", 0));
    }

    public string EvaluateChallenge(int difficulty, int scoreDifference, int isOnline)
    {
        // base XP for winning
        string[] xpFeedback = { "\nEasy Win +10XP", "\nMedium Win +20XP", "\nHard Win +30XP", };

        // bonus XP for first win of the day
        string dailyWin = "";
        string lastSavedDate = PlayerPrefs.GetString("LastDailyWinDate", string.Empty);
        DateTime lastChallengeDate;

        if (DateTime.TryParse(lastSavedDate, out lastChallengeDate))
        {
            // Compare the last saved date to today's date
            if (lastChallengeDate.Date != DateTime.Today)
            {
                dailyWin += "\nDaily Win +50XP";
                levelManager.AddXP(50);
                PlayerPrefs.SetString("LastDailyWinDate", DateTime.Today.ToString("yyyy-MM-dd"));
            }
        } 
        else // no date ever written
        {
            dailyWin += "\nDaily Win +50XP";
            levelManager.AddXP(50);
            PlayerPrefs.SetString("LastDailyWinDate", DateTime.Today.ToString("yyyy-MM-dd"));
        }

        // Evaluate daily challenges
        int challenge1 = PlayerPrefs.GetInt("DailyChallenge1", 0);
        int challenge2 = PlayerPrefs.GetInt("DailyChallenge2", 0);

        if (difficulty == easyChallengeCondition[challenge1, 0] && scoreDifference >= easyChallengeCondition[challenge1, 1] && isOnline == easyChallengeCondition[challenge1, 2] && challenge1 > 0)
        {
            PlayerPrefs.SetInt("DailyChallenge1", -challenge1);
        }
        if (difficulty == hardChallengeCondition[challenge2, 0] && scoreDifference >= hardChallengeCondition[challenge2, 1] && isOnline == hardChallengeCondition[challenge2, 2] && challenge2 > 0)
        {
            PlayerPrefs.SetInt("DailyChallenge2", -challenge2);
        }

        if (scoreDifference > 0 && isOnline == 0)
        {
            levelManager.AddXP((difficulty + 1) * 10);
            return xpFeedback[difficulty] + dailyWin;
        } 
        else
        {
            return string.Empty;
        }
    }

    // called by claim button
    public void ClaimReward1()
    {
        int DC1 = PlayerPrefs.GetInt("DailyChallenge1", 0);
        // Check if the reward is complete (negative value)
        if (DC1 < 0)
        {
            levelManager.AddXP(easyChallengeReward[Mathf.Abs(DC1)]); // add the reward to the player's XP
            Debug.Log("Claimed reward 1!");
            PlayerPrefs.SetInt("DailyChallenge1", 0); // 0 means the reward is claimed
            SetText();
            sound.PlayWinSFX();
        }
        titleScreen.GetComponent<TitleScreenScript>().UpdateAlerts();
    }

    // called by claim button
    public void ClaimReward2()
    {
        int DC2 = PlayerPrefs.GetInt("DailyChallenge2", 0);
        // Check if the reward is complete (negative value)
        if (DC2 < 0)
        {
            levelManager.AddXP(hardChallengeReward[Mathf.Abs(DC2)]); // add the reward to the player's XP
            Debug.Log("Claimed reward 2!");
            PlayerPrefs.SetInt("DailyChallenge2", 0); // 0 means the reward is claimed
            SetText();
            sound.PlayWinSFX();
        }
        titleScreen.GetComponent<TitleScreenScript>().UpdateAlerts();
    }
}
