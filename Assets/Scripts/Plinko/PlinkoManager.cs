using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlinkoManager : MonoBehaviour
{
    // self
    public static PlinkoManager Instance;

    //dependacies
    private PuckSkinManager puckSkinManager;
    private LevelManager levelManager;
    private SoundManagerScript sound;
    private UIManagerScript UI;

    // imports
    [SerializeField] private SpriteRenderer rewardSprite;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text countdownText;

    // Plinko Unlockable Ids
    private int[] plinkoUnlockableIDs = { 33, 34, 35, 36 };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void OnEnable()
    {
        puckSkinManager = PuckSkinManager.Instance;
        levelManager = LevelManager.Instance;
        sound = SoundManagerScript.Instance;
        UI = UIManagerScript.Instance;

        CheckForNewDailyPlinkoReward();
        int PlinkoReward = PlayerPrefs.GetInt("PlinkoReward", 100);
        if (PlinkoReward >= 100) // 100+ = XP REWARD
        {
            rewardText.text = "+" + PlinkoReward.ToString() + "XP";
            rewardSprite.sprite = null;
        }
        else // skin reward
        {
            rewardText.text = "UNLOCK";
            rewardSprite.sprite = puckSkinManager.ColorIDtoPuckSprite(PlinkoReward);
        }
    }

    // Update is called once per frame
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
            AssignNewPlinkoReward();
        }
    }

    private void CheckForNewDailyPlinkoReward()
    {
        // Get the last saved date or use a default if it's the first time
        string lastSavedDate = PlayerPrefs.GetString("LastPlinkoRewardDate", string.Empty);
        DateTime lastChallengeDate;

        if (DateTime.TryParse(lastSavedDate, out lastChallengeDate))
        {
            // Compare the last saved date to today's date
            if (lastChallengeDate.Date != DateTime.Today)
            {
                AssignNewPlinkoReward();
            }
            else
            {
                Debug.Log("Today's Plinko Reward is already assigned. " + PlayerPrefs.GetInt("PlinkoReward"));
            }
        }
        else // no date ever written
        {
            AssignNewPlinkoReward();
        }
    }

    private void AssignNewPlinkoReward()
    {
        PlayerPrefs.SetString("LastPlinkoRewardDate", DateTime.Today.ToString("yyyy-MM-dd"));

        int currentReward = PlayerPrefs.GetInt("PlinkoReward", 100);
        float rand = Random.Range(0f, 1f);

        // 50% chance of XP reward
        if (rand <= 0.5f)
        {
            if (currentReward == 100) // can't get XP reward back to back
            {
                rand += 0.5f;
            }
            else
            {
                Debug.Log(rand + " > 0");
                PlayerPrefs.SetInt("PlinkoReward", 100);
                return;
            }
        }

        // 50% chance of a skin reward
        for (int i = (plinkoUnlockableIDs.Length - 1); i >= 0; i--)
        {
            double threshold = 1f - Math.Pow(0.5f, i + 1f);
            if (rand > threshold)
            {
                Debug.Log(rand + " > " + threshold);
                // 0 = 0 = XP
                // 1 = 0.5 = First skin
                // 2 = 0.75
                // 3 = 0.875
                // 4 = 0.9375
                // 5 = 0.96875
                if (currentReward != plinkoUnlockableIDs[i] && PlayerPrefs.GetInt("puck"+ plinkoUnlockableIDs[i]+"unlocked") == 0)  // can't get same puck reward back to back, or if it's already unlocked
                {
                    PlayerPrefs.SetInt("PlinkoReward", plinkoUnlockableIDs[i]);
                }
                else
                {
                    PlayerPrefs.SetInt("PlinkoReward", 100*(i+2)); // XP scales with how rare the puck should have been
                }
                return;
            }
        }
    }

    public void MainReward()
    {
        int plinkoreward = PlayerPrefs.GetInt("PlinkoReward");
        if (plinkoreward >= 100) // XP reward
        {
            levelManager.AddXP(plinkoreward);
        }
        else // skin reward
        {
            puckSkinManager.UnlockPuckID(plinkoreward);
            PlayerPrefs.SetInt("puck" + plinkoreward.ToString() + "unlocked", 1);
            UI.SetErrorMessage("New puck unlocked!");
        }
        sound.PlayWinSFX();
    }

    public void SideReward()
    {
        levelManager.AddXP(100);
        sound.PlayWinSFX();
    }
}