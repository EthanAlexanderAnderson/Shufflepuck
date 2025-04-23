using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StreakManager : MonoBehaviour
{
    // self
    public static StreakManager Instance;

    // Daily streak stuff
    private int streak;

    [SerializeField] private GameObject[] streakCounters;
    [SerializeField] private TMP_Text streakRewardText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void Start()
    {
        IncrementStreak();
    }

    public void IncrementStreak()
    {
        streak = PlayerPrefs.GetInt("Streak", 1);

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
                    UIManagerScript.Instance.SetErrorMessage("Unlocked: +" + reward + " drops for streak reward!");
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

        Debug.Log("Daily Login Streak: " + streak);
        SetText();
    }

    public void SetText()
    {
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
