using System;
using System.Globalization;
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
        // Try to get the streak. If no streak has ever been saved, defaults to 0.
        streak = PlayerPrefs.GetInt("Streak", 0);

        // Try to read the DateTime from the "LastStreakDate" PlayerPref
        if (DateTime.TryParseExact(PlayerPrefs.GetString("LastStreakDate", string.Empty), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime LastStreakDate))
        {
            // If the last recored date is yesterday, increment the streak
            if (LastStreakDate.Date == DateTime.Today.AddDays(-1))
            {
                streak++;
                PlayerPrefs.SetInt("Streak", streak);
                if (((streak % 7) == 0 && streak > 0))
                {
                    var reward = ((streak - 1) / 7 + 1);
                    var dropped = PlayerPrefs.GetInt("PlinkoPegsDropped", 0);
                    PlayerPrefs.SetInt("PlinkoPegsDropped", dropped - reward);
                    Debug.Log("Rewarded: +" + reward + " drops for streak reward!");
                    // TODO: change startup function calling to not change UI screens so this message still shows
                    if (UIManagerScript.Instance != null) UIManagerScript.Instance.SetErrorMessage("Rewarded: +" + reward + " drops for streak reward!");
                }
            }
            // If the last recorded date is not yesterday or today, reset the streak
            else if (LastStreakDate.Date != DateTime.Today)
            {
                streak = 1;
                PlayerPrefs.SetInt("Streak", streak);
            }
        }
        // Can't Parse the last streak date (No streak date has ever been saved)
        else
        {
            streak = 1;
            PlayerPrefs.SetInt("Streak", streak);
        }

        PlayerPrefs.SetString("LastStreakDate", DateTime.Today.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
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
