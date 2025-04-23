using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlinkoManager : MonoBehaviour
{
    // self
    public static PlinkoManager Instance;

    // objects in scene
    [SerializeField] private Image rewardImage;
    [SerializeField] private GameObject rewardImageAnimation;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject bonusBucketLeft;
    [SerializeField] private TMP_Text bonusBucketLeftText;
    [SerializeField] private GameObject bonusBucketRight;
    [SerializeField] private TMP_Text bonusBucketRightText;
    [SerializeField] private GameObject plinkoDropButton;

    // asset imports
    [SerializeField] private Sprite plusPack;

    // floating text
    [SerializeField] private GameObject floatingTextPrefab;

    // Plinko Unlockable Ids & probability weights
    int[,] plinkoUnlockableIDs =
    {
        { 37, 15 },
        { 34, 15 },
        { 41, 15 },
        { 42, 10 },
        { 33, 10 },
        { 38, 10 },
        { 35, 6 },
        { 43, 6 },
        { 39, 6 },
        { 36, 3 },
        { 44, 3 },
        { 40, 1 }
    };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void OnEnable()
    {
        int PlinkoReward = PlayerPrefs.GetInt("PlinkoReward", 100);
        if (PlinkoReward >= 100) // 100+ = XP REWARD
        {
            rewardText.text = "+" + PlinkoReward.ToString() + "XP";
            rewardImageAnimation.SetActive(false);
        }
        else if (PlinkoReward > 0) // skin reward
        {
            rewardText.text = "UNLOCK";
            rewardImage.sprite = PuckSkinManager.Instance.ColorIDtoPuckSprite(PlinkoReward);
            rewardImageAnimation.SetActive(PlinkoReward == 40);
        }
        else
        {
            var (_, level) = LevelManager.Instance.GetXPAndLevel();
            rewardText.text = (int)(level / 10f) > 1 ? $"+{(int)(level / 10f)} +PACKS" : $"+{(int)(level / 10f)} +PACK";
            rewardImage.sprite = plusPack;
        }
        rewardImage.enabled = PlinkoReward < 100; // reward image should be disabled if the reward is XP
        UpdateSideBucketText();
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

    public void CheckForNewDailyPlinkoReward()
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
                // subtract welcome bonus
                var welcomeBonus = PlayerPrefs.GetInt("WelcomeBonus");
                if (welcomeBonus > 0)
                {
                    PlayerPrefs.SetInt("WelcomeBonus", welcomeBonus - 1);
                }
            }
            else
            {
                Debug.Log("Today's Plinko Reward is already assigned. " + PlayerPrefs.GetInt("PlinkoReward"));
            }
            // if the lastChallengeDate is 7 or more days ago, set welcome bonus
            if (DateTime.Today.Subtract(lastChallengeDate).Days >= 7)
            {
                PlayerPrefs.SetInt("WelcomeBonus", (int)DateTime.Today.Subtract(lastChallengeDate).Days / 7);
            }
        }
        else // no date ever written
        {
            AssignNewPlinkoReward();
            PlayerPrefs.SetInt("WelcomeBonus", 3);
        }

        // enable welcome bonus buckets
        if (PlayerPrefs.GetInt("WelcomeBonus") > 0)
        {
            bonusBucketLeft.SetActive(true);
            bonusBucketRight.SetActive(true);
            Debug.Log("Remaining Welcome Bonus Days: " + PlayerPrefs.GetInt("WelcomeBonus"));
        }

        TitleScreenScript.Instance.UpdateAlerts();
    }

    private void AssignNewPlinkoReward()
    {
        PlayerPrefs.SetString("LastPlinkoRewardDate", DateTime.Today.ToString("yyyy-MM-dd"));

        int currentReward = PlayerPrefs.GetInt("PlinkoReward", 100);

        int totalWeight = 0;

        // Shallow copy so we don't override real weights
        int[,] plinkoUnlockableIDsCopy = (int[,])plinkoUnlockableIDs.Clone();

        // get currently unlocked plinko skins
        List<int> alreadyUnlocked = PuckSkinManager.Instance.GetAllUnlockedPlinkoSkins();

        for (int i = 0; i < plinkoUnlockableIDsCopy.GetLength(0); i++)
        {
            int puckID = plinkoUnlockableIDsCopy[i, 0];
            // if a puck has been unlocked already or was the previous reward, set it's weight (probability of being the next reward) to be zero
            if (alreadyUnlocked.Contains(puckID) || puckID == currentReward)
            {
                plinkoUnlockableIDsCopy[i, 1] = 0;
            }
            else
            {
                totalWeight += plinkoUnlockableIDsCopy[i, 1];
            }
        }

        // if we have no pucks remaining
        if (totalWeight == 0 && currentReward > 0 && currentReward < 100)
        {
            // no pucks are left to unlock
            if (PuckSkinManager.Instance.IsPlinkoSkinUnlocked(currentReward))
            {
                Debug.Log("No puck skin rewards remain.");
                PlayerPrefs.SetInt("PlinkoReward", 0);
                var (_, level) = LevelManager.Instance.GetXPAndLevel();
                rewardText.text = (int)(level / 10f) > 1 ? $"+{(int)(level / 10f)} +PACKS" : $"+{(int)(level / 10f)} +PACK";
                rewardImage.sprite = plusPack;
                return;
            }
            // only one puck left to unlock
            else
            {
                return;
            }
        }

        int rand = Random.Range(0, (totalWeight - 1));
        int threshold = 0;

        for (int i = 0; i < plinkoUnlockableIDsCopy.GetLength(0); i++)
        {
            threshold += plinkoUnlockableIDsCopy[i, 1];

            if (rand < threshold)
            {
                PlayerPrefs.SetInt("PlinkoReward", plinkoUnlockableIDsCopy[i, 0]);
                rewardText.text = "UNLOCK";
                rewardImage.sprite = PuckSkinManager.Instance.ColorIDtoPuckSprite(plinkoUnlockableIDsCopy[i, 0]);
                rewardImageAnimation.SetActive(plinkoUnlockableIDsCopy[i, 0] == 40);
                Debug.Log("New Plinko Reward: " + PlayerPrefs.GetInt("PlinkoReward"));
                return;
            }
        }
    }

    public void UpdateSideBucketText()
    {
        // get level for pack reward quantity
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        string text = level > 1 ? $"+{level} PACKS" : $"+{level} PACK";
        bonusBucketLeftText.text = text;
        bonusBucketRightText.text = text;
    }

    public void MainReward( Transform self )
    {
        int plinkoreward = PlayerPrefs.GetInt("PlinkoReward");

        // input checking
        if (self == null || plinkoreward < 0)
        {
            Debug.LogError("PlinkoManager.MainReward() : Error : Plinko Reward not given.");
            UIManagerScript.Instance.SetErrorMessage("Error: Plinko Reward not given.");
            return;
        }

        // SFX for auditory feedback
        SoundManagerScript.Instance.PlayWinSFX();

        GameObject floatingText = Instantiate(floatingTextPrefab, self.position, Quaternion.identity, transform);

        // give the reward to the player
        // XP reward
        if (plinkoreward >= 100)
        {
            LevelManager.Instance.AddXP(plinkoreward);
            // floating text for visual feedback
            floatingText.GetComponent<FloatingTextScript>().Initialize("+" + plinkoreward + "XP", 0.5f, 15);
        }
        // plus packs
        else if (plinkoreward == 0)
        {
            // get level for pack reward quantity
            var (_, level) = LevelManager.Instance.GetXPAndLevel();

            // give the reward to the player
            PackManager.Instance.RewardPacks(true, (int)(level / 10f));

            // floating text for visual feedback
            floatingText.GetComponent<FloatingTextScript>().Initialize((int)(level / 10f) > 1 ? $"+{(int)(level / 10f)} +PACKS" : $"+{(int)(level / 10f)} +PACK", 0.5f, 15);
        }
        // skin reward
        else if (plinkoreward > 0 && plinkoreward < 100)
        {
            // if the puck has not been unlocked already, unlock it. otherwise it is a duplicate and we should grant XP
            if (!PuckSkinManager.Instance.IsPlinkoSkinUnlocked(plinkoreward))
            {
                PuckSkinManager.Instance.UnlockPuckID(plinkoreward);
                PuckSkinManager.Instance.UnlockPlinkoSkin(plinkoreward);
                UIManagerScript.Instance.SetErrorMessage("New puck unlocked!");
            }
            else
            {
                plinkoreward = 100;
                LevelManager.Instance.AddXP(plinkoreward);
                UIManagerScript.Instance.SetErrorMessage("Duplicate reward. Adding +100XP instead.");
            }
            // floating text for visual feedback
            floatingText.GetComponent<FloatingTextScript>().Initialize((plinkoreward >= 100) ? "+" + plinkoreward + "XP" : "UNLOCKED!", 0.5f, 15);
        }
    }

    public void SideReward( Transform self )
    {
        // get level for pack reward quantity
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        // give the reward to the player
        PackManager.Instance.RewardPacks(false, level);

        // SFX for auditory feedback
        SoundManagerScript.Instance.PlayWinSFX();

        // floating text for visual feedback
        var floatingText = Instantiate(floatingTextPrefab, self.position, Quaternion.identity, transform);
        floatingText.GetComponent<FloatingTextScript>().Initialize(level > 1 ? $"+{level} PACKS" : $"+{level} PACK", 0.5f, 15);
    }

    public void BonusReward(Transform self)
    {
        // give the reward to the player
        LevelManager.Instance.AddXP(100);

        // SFX for auditory feedback
        SoundManagerScript.Instance.PlayWinSFX();

        // floating text for visual feedback
        var floatingText = Instantiate(floatingTextPrefab, self.position, Quaternion.identity, transform);
        floatingText.GetComponent<FloatingTextScript>().Initialize("+100XP", 0.5f, 15);

        // add a drop if we leveled up
        plinkoDropButton.GetComponent<PlinkoDropButtonScript>().SetDropButtonText();
    }
}