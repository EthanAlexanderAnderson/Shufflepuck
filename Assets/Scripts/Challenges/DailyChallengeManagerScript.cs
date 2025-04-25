using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DailyChallengeManagerScript : MonoBehaviour
{
    // self
    public static DailyChallengeManagerScript Instance;

    [SerializeField] private GameObject titleScreen;

    [SerializeField] private TMP_Text countdownText;

    [SerializeField] private TMP_Text challenge1Text;
    [SerializeField] private TMP_Text challenge2Text;

    [SerializeField] private TMP_Text challenge1Reward1Text;
    [SerializeField] private TMP_Text challenge1Reward2Text;

    [SerializeField] private TMP_Text challenge2Reward1Text;
    [SerializeField] private TMP_Text challenge2Reward2Text;

    [SerializeField] private Button claim1;
    [SerializeField] private Button claim2;
    [SerializeField] private GameObject glow1;
    [SerializeField] private GameObject glow2;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    // Called at start after the challenges are instantiated by ChallengeManager. also called when a challenge is assigned/completed or a reward is claimed.
    public void SetText()
    {
        CheckForNewDailyChallenge();
        int DC1 = PlayerPrefs.GetInt("DailyChallenge1", 0);
        int DC2 = PlayerPrefs.GetInt("DailyChallenge2", 0);
        // if the challenge is completed, the value is negative
        claim1.interactable = DC1 < 0;
        claim2.interactable = DC2 < 0;
        glow1.SetActive(claim1.interactable);
        glow2.SetActive(claim2.interactable);

        List<Challenge> easyDailyChallenges = ChallengeManager.Instance.challengeData.easyDailyChallenges;
        List<Challenge> hardDailyChallenges = ChallengeManager.Instance.challengeData.hardDailyChallenges;
        int numberOfEasyDailyChallenges = easyDailyChallenges.Count;
        int numberOfHardDailyChallenges = hardDailyChallenges.Count;

        // assert the challenge ID is within range, prevent index error
        if (DC1 >= numberOfEasyDailyChallenges || DC1 <= (numberOfEasyDailyChallenges * -1) || DC1 <= (numberOfEasyDailyChallenges * -1) || DC1 <= (numberOfEasyDailyChallenges * -1))
        {
            DC1 = 0;
            PlayerPrefs.SetInt("DailyChallenge1", 0);
        }
        if (DC2 >= numberOfHardDailyChallenges || DC2 <= (numberOfHardDailyChallenges * -1) || DC2 >= numberOfHardDailyChallenges || DC2 <= (numberOfHardDailyChallenges * -1))
        {
            DC2 = 0;
            PlayerPrefs.SetInt("DailyChallenge2", 0);
        }

        challenge1Reward1Text.text = "";
        challenge1Reward2Text.text = "";
        List<string> easyRewardStrings = easyDailyChallenges[Mathf.Abs(DC1)].GetRewardStrings();
        TMP_Text[] easyRewardTexts = { challenge1Reward1Text, challenge1Reward2Text };
        for (int i = 0; i < easyRewardStrings.Count; i++)
        {
            easyRewardTexts[i].text = easyRewardStrings[i];
        }
        challenge1Text.text = easyDailyChallenges[Mathf.Abs(DC1)].challengeText;

        challenge2Reward1Text.text = "";
        challenge2Reward2Text.text = "";
        List<string> hardRewardStrings = hardDailyChallenges[Mathf.Abs(DC2)].GetRewardStrings();
        TMP_Text[] hardRewardTexts = { challenge2Reward1Text, challenge2Reward2Text };
        for (int i = 0; i < hardRewardStrings.Count; i++)
        {
            hardRewardTexts[i].text = hardRewardStrings[i];
        }
        challenge2Text.text = hardDailyChallenges[Mathf.Abs(DC2)].challengeText;
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
                Debug.Log("Today's daily challenges are already assigned. easy: " + PlayerPrefs.GetInt("DailyChallenge1", 0) + "   hard: " + PlayerPrefs.GetInt("DailyChallenge2", 0));
            }
        }
        else // no date ever written
        {
            AssignNewChallenge();
        }
    }

    private void AssignNewChallenge()
    {
        List<Challenge> easyDailyChallenges = ChallengeManager.Instance.challengeData.easyDailyChallenges;
        List<Challenge> hardDailyChallenges = ChallengeManager.Instance.challengeData.hardDailyChallenges;
        int numberOfEasyDailyChallenges = easyDailyChallenges.Count;
        int numberOfHardDailyChallenges = hardDailyChallenges.Count;

        PlayerPrefs.SetString("LastChallengeDate", DateTime.Today.ToString("yyyy-MM-dd"));

        // only overwrite the challenge if it's not already completed (not a negative int id)
        if (PlayerPrefs.GetInt("DailyChallenge1", 0) >= 0)
        {
            int DC1 = UnityEngine.Random.Range(1, numberOfEasyDailyChallenges);

            // ensure challenge is assignable
            if (PlayerPrefs.GetInt("easyHighscore", 0) == 0)
            {
                PlayerPrefs.SetInt("DailyChallenge1", 1);
            }
            else
            {
                Challenge selectedChallenge = easyDailyChallenges[DC1];
                int failsafe = 0;
                while (!selectedChallenge.condition.IsAssignable() && failsafe < 1000)
                {
                    selectedChallenge = easyDailyChallenges[UnityEngine.Random.Range(1, numberOfEasyDailyChallenges)];
                    failsafe++;
                    if (failsafe >= 1000)
                    {
                        Debug.Log("failsafe triggered");
                        selectedChallenge = easyDailyChallenges[1];
                    }
                }
                PlayerPrefs.SetInt("DailyChallenge1", easyDailyChallenges.IndexOf(selectedChallenge));
            }
        }
        if (PlayerPrefs.GetInt("DailyChallenge2", 0) >= 0)
        {
            int DC2 = UnityEngine.Random.Range(1, numberOfHardDailyChallenges);

            // ensure challenge is assignable
            if (PlayerPrefs.GetInt("hardHighscore", 0) == 0)
            {
                PlayerPrefs.SetInt("DailyChallenge2", 1);
            }
            else
            {
                Challenge selectedChallenge = hardDailyChallenges[DC2];
                int failsafe = 0;
                while (!selectedChallenge.condition.IsAssignable() && failsafe < 1000)
                {
                    selectedChallenge = hardDailyChallenges[UnityEngine.Random.Range(1, numberOfHardDailyChallenges)];
                    failsafe++;
                    if (failsafe >= 1000)
                    {
                        Debug.Log("failsafe triggered");
                        selectedChallenge = hardDailyChallenges[1];
                    }
                }
                PlayerPrefs.SetInt("DailyChallenge2", hardDailyChallenges.IndexOf(selectedChallenge));
            }
        }

        SetText();
        Debug.Log($"New daily challenges assigned. " + PlayerPrefs.GetInt("DailyChallenge1", 0) + " " + PlayerPrefs.GetInt("DailyChallenge2", 0));
    }

    // TODO: seperate xpfeedback & point bonus from daily challenges
    public string EvaluateChallenge(int difficulty, int scoreDifference, int isOnline)
    {
        // base XP for winning
        string[] xpFeedback = { "\nEasy Win +10XP", "\nMedium Win +20XP", "\nHard Win +30XP", };

        // bonus XP for score
        string pointBonus = "\nScore Bonus +" + (difficulty + 1) * scoreDifference + "XP";

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
                LevelManager.Instance.AddXP(50);
                PlayerPrefs.SetString("LastDailyWinDate", DateTime.Today.ToString("yyyy-MM-dd"));
            }
        } 
        else // no date ever written
        {
            dailyWin += "\nDaily Win +50XP";
            LevelManager.Instance.AddXP(50);
            PlayerPrefs.SetString("LastDailyWinDate", DateTime.Today.ToString("yyyy-MM-dd"));
        }

        // assert the daily challenges IDs are within range, prevent index error
        int DC1 = PlayerPrefs.GetInt("DailyChallenge1", 0);
        int DC2 = PlayerPrefs.GetInt("DailyChallenge2", 0);
        List<Challenge> easyDailyChallenges = ChallengeManager.Instance.challengeData.easyDailyChallenges;
        List<Challenge> hardDailyChallenges = ChallengeManager.Instance.challengeData.hardDailyChallenges;
        int numberOfEasyDailyChallenges = easyDailyChallenges.Count;
        int numberOfHardDailyChallenges = hardDailyChallenges.Count;
        if (DC1 >= numberOfEasyDailyChallenges || DC1 <= (numberOfEasyDailyChallenges * -1))
        {
            DC1 = 0;
            PlayerPrefs.SetInt("DailyChallenge1", 0);
        }
        if (DC1 >= numberOfHardDailyChallenges || DC1 <= (numberOfHardDailyChallenges * -1))
        {
            DC1 = 0;
            PlayerPrefs.SetInt("DailyChallenge2", 0);
        }

        if (isOnline == 1) { difficulty = -1; }

        // Evalute condition is met
        if (DC1 > 0 && easyDailyChallenges[DC1].CheckCompletion(scoreDifference, difficulty))
        {
            PlayerPrefs.SetInt("DailyChallenge1", -DC1);
        }
        if (DC2 > 0 && hardDailyChallenges[DC2].CheckCompletion(scoreDifference, difficulty))
        {
            PlayerPrefs.SetInt("DailyChallenge2", -DC2);
        }
        SetText();

        if (scoreDifference > 0 && isOnline == 0)
        {
            LevelManager.Instance.AddXP((difficulty + 1) * 10);
            LevelManager.Instance.AddXP((difficulty + 1) * scoreDifference);
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
            List<Challenge> easyDailyChallenges = ChallengeManager.Instance.challengeData.easyDailyChallenges;
            try { easyDailyChallenges[Mathf.Abs(DC1)].ClaimRewards(); } // add the reward to the player's XP
            catch (IndexOutOfRangeException e) { LevelManager.Instance.AddXP(50); Debug.LogError(e); }
            Debug.Log("Claimed reward 1!");
            PlayerPrefs.SetInt("DailyChallenge1", 0); // 0 means the reward is claimed
            SetText();
            SoundManagerScript.Instance.PlayWinSFX();
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
            List<Challenge> hardDailyChallenges = ChallengeManager.Instance.challengeData.hardDailyChallenges;
            try { hardDailyChallenges[Mathf.Abs(DC2)].ClaimRewards(); } // add the reward to the player's XP
            catch (IndexOutOfRangeException e) { LevelManager.Instance.AddXP(100); Debug.LogError(e); }
            Debug.Log("Claimed reward 2!");
            PlayerPrefs.SetInt("DailyChallenge2", 0); // 0 means the reward is claimed
            SetText();
            SoundManagerScript.Instance.PlayWinSFX();
        }
        titleScreen.GetComponent<TitleScreenScript>().UpdateAlerts();
    }
}
