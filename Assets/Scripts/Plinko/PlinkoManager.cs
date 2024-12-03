using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private Image rewardImage;
    [SerializeField] private GameObject rewardImageAnimation;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject bonusBucketLeft;
    [SerializeField] private GameObject bonusBucketRight;

    // floating text
    [SerializeField] private GameObject floatingTextPrefab;

    // Plinko Unlockable Ids
    private int[] plinkoUnlockableIDs = { 37, 33, 34, 38, 35, 39, 36, 40 };

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
            rewardImage.sprite = null;
        }
        else // skin reward
        {
            rewardText.text = "UNLOCK";
            rewardImage.sprite = puckSkinManager.ColorIDtoPuckSprite(PlinkoReward);
            rewardImageAnimation.SetActive(PlinkoReward == 40);
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
            // Compare the last saved date to today's date & make sure current date is NEWER than lastChallengeDate to prevent device time tampering
            if (DateTime.Today.Subtract(lastChallengeDate).Days >= 1)
            {
                AssignNewPlinkoReward();
                // if the lastChallengeDate is 7 or more days ago, enable welcome back bonus buckets
                if (DateTime.Today.Subtract(lastChallengeDate).Days >= 7)
                {
                    bonusBucketLeft.SetActive(true);
                    bonusBucketRight.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Today's Plinko Reward is already assigned. " + PlayerPrefs.GetInt("PlinkoReward"));
            }
        }
        else // no date ever written
        {
            AssignNewPlinkoReward();
            bonusBucketLeft.SetActive(true);
            bonusBucketRight.SetActive(true);
        }
    }

    private void AssignNewPlinkoReward()
    {
        PlayerPrefs.SetString("LastPlinkoRewardDate", DateTime.Today.ToString("yyyy-MM-dd"));

        // Already unlocked IDs
        int[] plinkoUnlockedIDs = { 0, 0, 0, 0, 0, 0, 0, 0 };
        for (int i = 0; i < plinkoUnlockableIDs.Length; i++)
        {
            if (PlayerPrefs.GetInt("puck" + plinkoUnlockableIDs[i] + "unlocked") == 1)
            {
                plinkoUnlockedIDs[i] = plinkoUnlockableIDs[i];
            }
        }

        int currentReward = PlayerPrefs.GetInt("PlinkoReward", 100);
        int randMax = (int)Math.Pow(2, plinkoUnlockableIDs.Length);
        int rand = Random.Range(0, (randMax-1));
        int count = 0;

        for (int i = 0; i < plinkoUnlockableIDs.Length; i++)
        {
            double threshold = randMax - Math.Pow(2, (plinkoUnlockableIDs.Length - 1) - i); // 256 - 2^(7 - i)
            if (rand < threshold)
            {
                // i = 0 : rand must be less than 128 ( 128/256 : 50%)
                // i = 1 : rand must be less than 192 (  64/256 : 25%)
                // i = 2 : rand must be less than 224 (  32/256 : 12.5%)
                // i = 3 : rand must be less than 240 (  16/256 : 6.25%)
                // i = 4 : rand must be less than 248 (   8/256 : 3.125%)
                // i = 5 : rand must be less than 252 (   4/256 : 1.5625%)
                // i = 6 : rand must be less than 254 (   2/256 : 0.78125%)
                // i = 7 : rand must be less than 255 (   1/256 : 0.390625%)

                // set the new reward if it's not the current reward and not already unlocked
                if (currentReward != plinkoUnlockableIDs[i] && Array.IndexOf(plinkoUnlockedIDs, plinkoUnlockableIDs[i]) == -1)
                {
                    PlayerPrefs.SetInt("PlinkoReward", plinkoUnlockableIDs[i]);
                    return;
                }
                // otherwise, reroll
                else
                {
                    rand = Random.Range(0, (randMax-1));
                    i = 0;
                    count++;
                    // infinite loop stopper
                    if (count > 100000)
                    {
                        // interate through all unlockable IDs and pick first one that is not already unlocked
                        for (int j = 0; j < plinkoUnlockableIDs.Length; j++)
                        {
                            if (Array.IndexOf(plinkoUnlockedIDs, plinkoUnlockableIDs[j]) == -1)
                            {
                                PlayerPrefs.SetInt("PlinkoReward", plinkoUnlockableIDs[j]);
                                return;
                            }
                        }
                        // if all skins are unlocked, XP reward
                        Debug.Log("No puck skin rewards remain.");
                        PlayerPrefs.SetInt("PlinkoReward", 999);
                        return;
                    }
                }
            }
        }
    }

    public void MainReward( Transform self )
    {
        int plinkoreward = PlayerPrefs.GetInt("PlinkoReward");

        // input checking
        if (self == null || plinkoreward == 0)
        {
            Debug.LogError("PlinkoManager.MainReward() : Error : Plinko Reward not given.");
            UI.SetErrorMessage("Error: Plinko Reward not given.");
            return;
        }

        // give the reward to the player
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

        // SFX for auditory feedback
        sound.PlayWinSFX();

        // floating text for visual feedback
        var floatingText = Instantiate(floatingTextPrefab, self.position, Quaternion.identity, transform);
        floatingText.GetComponent<FloatingTextScript>().Initialize(0.5f, 15);
        floatingText.GetComponent<TMP_Text>().text = (plinkoreward >= 100) ? "+" + plinkoreward + "XP" : "UNLOCKED!";
    }

    public void SideReward( Transform self )
    {
        // give the reward to the player
        levelManager.AddXP(100);

        // SFX for auditory feedback
        sound.PlayWinSFX();

        // floating text for visual feedback
        var floatingText = Instantiate(floatingTextPrefab, self.position, Quaternion.identity, transform);
        floatingText.GetComponent<FloatingTextScript>().Initialize(0.5f, 15);
        floatingText.GetComponent<TMP_Text>().text = "+100XP";
    }
}