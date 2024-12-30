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
    private UIManagerScript UI;

    [SerializeField] private GameObject titleScreen;

    [SerializeField] private TMP_Text countdownText;

    [SerializeField] private TMP_Text challenge1Text;
    [SerializeField] private TMP_Text challenge2Text;

    [SerializeField] private TMP_Text reward1Text;
    [SerializeField] private TMP_Text reward2Text;

    [SerializeField] private Button claim1;
    [SerializeField] private Button claim2;
    [SerializeField] private GameObject glow1;
    [SerializeField] private GameObject glow2;

    private string[] easyChallengeText = { "Claimed", "Beat the easy CPU", "Beat the medium CPU", "Beat the easy CPU by 3 or more points", "Beat the medium CPU by 3 or more points", "Beat the easy CPU by 5 or more points" };
    private int[] easyChallengeReward = { 0, 40, 60, 60, 80, 80 };
    private int[,] easyChallengeCondition = { { 0, 0, 0 }, { 0, 1, 0 }, { 1, 1, 0 }, { 0, 3, 0 }, { 1, 3, 0 }, { 0, 5, 0 } }; // {difficulty, score difference, isOnline}
    private string[] hardChallengeText = { "Claimed", "Beat the hard CPU", "Beat the hard CPU by 3 or more points", "Beat the medium CPU by 5 or more points", "Beat the hard CPU by 5 or more points", "Win an online match" };
    private int[] hardChallengeReward = { 0, 80, 100, 100, 120, 200 };
    private int[,] hardChallengeCondition = { { 0, 0, 0 }, { 2, 1, 0 }, { 2, 3, 0 }, { 1, 5, 0 }, { 2, 5, 0 }, { 2, 1, 1 } }; // {difficulty, score difference, isOnline}

    // Daily streak stuff
    [SerializeField] private GameObject streakCounter1;
    [SerializeField] private GameObject streakCounter2;
    [SerializeField] private GameObject streakCounter3;
    [SerializeField] private GameObject streakCounter4;
    [SerializeField] private GameObject streakCounter5;
    [SerializeField] private GameObject streakCounter6;
    [SerializeField] private GameObject streakCounter7;
    [SerializeField] private GameObject[] streakCounters;
    [SerializeField] private TMP_Text streakRewardText;

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
        UI = UIManagerScript.Instance;
    }

    public void SetText()
    {
        IncrementStreak();
        CheckForNewDailyChallenge();
        int DC1 = PlayerPrefs.GetInt("DailyChallenge1", 0);
        int DC2 = PlayerPrefs.GetInt("DailyChallenge2", 0);
        // if the challenge is completed, the value is negative
        claim1.interactable = DC1 < 0;
        claim2.interactable = DC2 < 0;
        glow1.SetActive(claim1.interactable);
        glow2.SetActive(claim2.interactable);

        // assert the challenge ID is within range, prevent index error
        if (DC1 >= easyChallengeText.Length || DC1 <= (easyChallengeText.Length * -1) || DC1 <= (easyChallengeReward.Length * -1) || DC1 <= (easyChallengeReward.Length * -1))
        {
            DC1 = 0;
            PlayerPrefs.SetInt("DailyChallenge1", 0);
        }
        if (DC2 >= hardChallengeText.Length || DC2 <= (hardChallengeText.Length * -1) || DC2 >= hardChallengeReward.Length || DC2 <= (hardChallengeReward.Length * -1))
        {
            DC2 = 0;
            PlayerPrefs.SetInt("DailyChallenge2", 0);
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
            // Compare the last saved date to today's date & make sure current date is NEWER than lastChallengeDate to prevent device time tampering
            if (DateTime.Today.Subtract(lastChallengeDate).Days >= 1)
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

        // bonus XP for score
        string pointBonus = "\nScore Bonus +" + scoreDifference + "XP";

        // bonus XP for first win of the day
        string dailyWin = "";
        string lastSavedDate = PlayerPrefs.GetString("LastDailyWinDate", string.Empty);
        DateTime lastChallengeDate;

        if (DateTime.TryParse(lastSavedDate, out lastChallengeDate))
        {
            // Compare the last saved date to today's date & make sure current date is NEWER than lastChallengeDate to prevent device time tampering
            if (DateTime.Today.Subtract(lastChallengeDate).Days >= 1)
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

        // assert the daily challenges IDs are within range, prevent index error
        int challenge1 = PlayerPrefs.GetInt("DailyChallenge1", 0);
        int challenge2 = PlayerPrefs.GetInt("DailyChallenge2", 0);
        if (challenge1 >= easyChallengeCondition.Length || challenge1 <= (easyChallengeCondition.Length * -1))
        {
            challenge1 = 0;
            PlayerPrefs.SetInt("DailyChallenge1", 0);
        }
        if (challenge2 >= hardChallengeCondition.Length || challenge2 <= (hardChallengeCondition.Length * -1))
        {
            challenge2 = 0;
            PlayerPrefs.SetInt("DailyChallenge2", 0);
        }

        // Evaluate daily challenges
        if (challenge1 > 0 && difficulty == easyChallengeCondition[challenge1, 0] && scoreDifference >= easyChallengeCondition[challenge1, 1] && isOnline == easyChallengeCondition[challenge1, 2])
        {
            PlayerPrefs.SetInt("DailyChallenge1", -challenge1);
        }
        if (challenge2 > 0 && difficulty == hardChallengeCondition[challenge2, 0] && scoreDifference >= hardChallengeCondition[challenge2, 1] && isOnline == hardChallengeCondition[challenge2, 2])
        {
            PlayerPrefs.SetInt("DailyChallenge2", -challenge2);
        }

        if (scoreDifference > 0 && isOnline == 0)
        {
            levelManager.AddXP((difficulty + 1) * 10);
            levelManager.AddXP(scoreDifference);
            return xpFeedback[difficulty] + pointBonus + dailyWin;
        }
        else if (scoreDifference > 0 && isOnline == 1)
        {
            return dailyWin;
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
            try { levelManager.AddXP(easyChallengeReward[Mathf.Abs(DC1)]); } // add the reward to the player's XP
            catch (IndexOutOfRangeException e) { levelManager.AddXP(50); Debug.LogError(e); }
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
            try { levelManager.AddXP(hardChallengeReward[Mathf.Abs(DC2)]); } // add the reward to the player's XP
            catch (IndexOutOfRangeException e) { levelManager.AddXP(100); Debug.LogError(e); }
            Debug.Log("Claimed reward 2!");
            PlayerPrefs.SetInt("DailyChallenge2", 0); // 0 means the reward is claimed
            SetText();
            sound.PlayWinSFX();
        }
        titleScreen.GetComponent<TitleScreenScript>().UpdateAlerts();
    }

    private void IncrementStreak()
    {
        var streak = PlayerPrefs.GetInt("Streak", 1);

        // Get the last saved date or use a default if it's the first time
        string lastSavedDate = PlayerPrefs.GetString("LastChallengeDate", string.Empty);
        DateTime lastChallengeDate;

        if (DateTime.TryParse(lastSavedDate, out lastChallengeDate))
        {
            // If the last recored date is yesterday, increment the streak
            if (lastChallengeDate.Date == DateTime.Today.AddDays(-1))
            {
                PlayerPrefs.SetInt("Streak", streak + 1);
                streak++;
                if (((streak % 7) == 0 && streak > 0))
                {
                    var reward = ((streak - 1) / 7 + 1);
                    var dropped = PlayerPrefs.GetInt("PlinkoPegsDropped", 0);
                    PlayerPrefs.SetInt("PlinkoPegsDropped", dropped - reward);
                    Debug.Log("Unlocked: +" + reward + " drops for streak reward!");
                    UI.SetErrorMessage("Unlocked: +" + reward + " drops for streak reward!");
                }
            }
            // If the last recorded date is not yesterday or today, reset the streak
            else if (lastChallengeDate.Date != DateTime.Today)
            {
                PlayerPrefs.SetInt("Streak", 1);
                streak = 1;
            }
        }
        else // no date ever written
        {
            PlayerPrefs.SetInt("Streak", 1);
        }

        Debug.Log("Streak: " + streak);
        // set the correct number of streak counters to green, 1-7 for a week
        for (int i = 0; i < streakCounters.Length; i++)
        {
            // since our streak is weekly, we mod the streak by 7. If the streak is divisible by 7 (a full week), we set all counters to green
            var streakModded = ((streak % 7) == 0 && streak > 0) ? 7 : streak % 7;
            if (i < streakModded)
            {
                streakCounters[i].GetComponent<Image>().color = new Color(0.4862745f, 0.7725491f, 0.4627451f);
            }
            else
            {
                streakCounters[i].GetComponent<Image>().color = new Color(1, 1, 1);
            }
        }

        // set the streak reward text, +1 drop for each 7 days of streak, rounded up
        var dropDrops = streak > 7 ? " Drops" : " Drop";
        streakRewardText.text = "+" + ((streak - 1) / 7 + 1) + dropDrops;

        // if streak is divisible by 7, set the streak reward text to green to indicate a reward was granted
        // NOTE: this doesn't work right now because of darkmode text color switching, may or may not fix this later if i feel like it
        if (((streak % 7) == 0 && streak > 0))
        {
            streakRewardText.color = new Color(0.4862745f, 0.7725491f, 0.4627451f);
        }
    }
}
