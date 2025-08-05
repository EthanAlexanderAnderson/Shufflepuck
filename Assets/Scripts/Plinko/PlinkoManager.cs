using System;
using System.Collections.Generic;
using System.Globalization;
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

    // fallback default reward
    const int fallbackXPReward = 100;

    // Plinko Unlockable Ids & probability weights
    int[,] plinkoUnlockableIDs =
    {
        { 37, 10 },
        { 52, 10 },
        { 48, 10 },
        { 34, 10 },
        { 50, 8 },
        { 41, 8 },
        { 49, 8 },
        { 42, 8 },
        { 47, 4 },
        { 45, 4 },
        { 33, 4 },
        { 38, 4 },
        { 35, 2 },
        { 51, 2 },
        { 46, 2 },
        { 43, 2 },
        { 39, 1 },
        { 36, 1 },
        { 44, 1 },
        { 40, 1 }
    };
    // plinko reward <= 0 means we reward plus packs, greater than 0 means we reward that much XP

# if UNITY_EDITOR
    // tracking for odds calculation in editor
    private float dropped = 0;
    private float mainRewards = 0;
    private float sideRewards = 0;
    private float bonusRewards = 0;

    public void IncrementDropped(int type)
    {
        if (type == 0) { dropped++; }
        else if (type == 1) { mainRewards++; }
        else if (type == 2) { sideRewards++; }
        else if (type == 3) { bonusRewards++; }
        Debug.Log("Main Reward Odds: " + mainRewards / dropped);
        Debug.Log("Side Reward Odds: " + sideRewards / dropped);
        Debug.Log("Bonus Reward Odds: " + bonusRewards / dropped);
    }
#endif

    private bool IDisPlinkoUnlockable(int ID)
    {
        for (int i = 0; i < plinkoUnlockableIDs.GetLength(0); i++)
        {
            if (plinkoUnlockableIDs[i, 0] == ID)
            {
                return true;
            }
        }
        return false;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    // OnEnabling the plinko screen, set the text and images
    private void OnEnable()
    {
        int plinkoReward = PlayerPrefs.GetInt("PlinkoReward");
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        // skin reward
        if (IDisPlinkoUnlockable(plinkoReward))
        {
            rewardText.text = PuckSkinManager.Instance.IsPlinkoSkinUnlocked(plinkoReward) ? "" : "UNLOCK";
            rewardImage.enabled = true;
            rewardImage.sprite = PuckSkinManager.Instance.ColorIDtoPuckSprite(plinkoReward);
        }
        // plus pack reward
        else if (plinkoReward <= 0 && (int)(level / 10f) >= 1)
        {
            rewardText.text = (int)(level / 10f) > 1 ? $"+{(int)(level / 10f)} +PACKS" : $"+{(int)(level / 10f)} +PACK";
            rewardImage.enabled = true;
            rewardImage.sprite = plusPack;
        }
        // XP REWARD
        else
        {
            // if the reward would be 0 or negative XP, default it to the fallbackXPReward
            if (plinkoReward <= 0)
            {
                plinkoReward = fallbackXPReward;
                PlayerPrefs.SetInt("PlinkoReward", fallbackXPReward);
            }
            rewardText.text = "+" + plinkoReward.ToString() + "XP";
            rewardImage.enabled = false;
        }

        rewardImageAnimation.SetActive(plinkoReward == 40);
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
        // Try to read the DateTime from the "LastPlinkoRewardDate" PlayerPref
        if (DateTime.TryParseExact(PlayerPrefs.GetString("LastPlinkoRewardDate", string.Empty), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastPlinkoRewardDate))
        {
            // Compare the last saved date to today's date & make sure current date is NEWER than lastChallengeDate to prevent device time tampering
            if (DateTime.Today.Subtract(lastPlinkoRewardDate).Days >= 1)
            {
                AssignNewPlinkoReward();
                // subtract welcome bonus
                int welcomeBonus = PlayerPrefs.GetInt("WelcomeBonus");
                if (welcomeBonus > 0)
                {
                    PlayerPrefs.SetInt("WelcomeBonus", welcomeBonus - 1);
                }
            }
            else
            {
                Debug.Log("Daily Plinko Reward already assigned: " + PlayerPrefs.GetInt("PlinkoReward"));
            }
            // if the lastChallengeDate is 7 or more days ago, set welcome bonus
            if (DateTime.Today.Subtract(lastPlinkoRewardDate).Days >= 7)
            {
                PlayerPrefs.SetInt("WelcomeBonus", (int)DateTime.Today.Subtract(lastPlinkoRewardDate).Days / 7);
            }
        }
        else // no date ever written
        {
            AssignNewPlinkoReward();
            PlayerPrefs.SetInt("WelcomeBonus", 3);
        }

        // enable welcome bonus buckets
        int newWelcomeBonus = PlayerPrefs.GetInt("WelcomeBonus");
        bonusBucketLeft.SetActive(newWelcomeBonus > 0);
        bonusBucketRight.SetActive(newWelcomeBonus > 0);
        if (newWelcomeBonus > 0) { Debug.Log("Remaining Welcome Bonus Days: " + PlayerPrefs.GetInt("WelcomeBonus")); }

        TitleScreenScript.Instance.UpdateAlerts();
    }

    private void AssignNewPlinkoReward()
    {
        PlayerPrefs.SetString("LastPlinkoRewardDate", DateTime.Today.ToString("yyyy-MM-dd"));

        int currentReward = PlayerPrefs.GetInt("PlinkoReward");

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

        // if we have no different locked pucks we can set as the new reward
        if (totalWeight <= 0)
        {
            // if the current reward is a locked puck, keep it (make sure it's actually an unlockable ID)
            if (!PuckSkinManager.Instance.IsPlinkoSkinUnlocked(currentReward) && IDisPlinkoUnlockable(currentReward))
            {
                return;
            }
            // all pucks are unlocked
            else
            {
                Debug.Log("No Plinko puck skins remain.");
                var (_, level) = LevelManager.Instance.GetXPAndLevel();
                // plus pack reward
                if ((int)(level / 10f) >= 1)
                {
                    PlayerPrefs.SetInt("PlinkoReward", 0);
                    rewardText.text = (int)(level / 10f) > 1 ? $"+{(int)(level / 10f)} +PACKS" : $"+{(int)(level / 10f)} +PACK";
                    rewardImage.enabled = true;
                    rewardImage.sprite = plusPack;
                }
                // fallback XP Reward
                else
                {
                    PlayerPrefs.SetInt("PlinkoReward", fallbackXPReward);
                    rewardText.text = "+" + fallbackXPReward.ToString() + "XP";
                    rewardImage.enabled = false;
                }

                rewardImageAnimation.SetActive(false);
                return;
            }
        }

        int rand = Random.Range(0, totalWeight);
        int threshold = 0;

        for (int i = 0; i < plinkoUnlockableIDsCopy.GetLength(0); i++)
        {
            threshold += plinkoUnlockableIDsCopy[i, 1];

            if (rand < threshold)
            {
                int plinkoReward = plinkoUnlockableIDsCopy[i, 0];
                PlayerPrefs.SetInt("PlinkoReward", plinkoReward);
                rewardText.text = PuckSkinManager.Instance.IsPlinkoSkinUnlocked(plinkoReward) ? "" : "UNLOCK";
                rewardImage.enabled = true;
                rewardImage.sprite = PuckSkinManager.Instance.ColorIDtoPuckSprite(plinkoReward);
                rewardImageAnimation.SetActive(plinkoReward == 40);
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

    public void MainReward(Transform self)
    {
#if UNITY_EDITOR
        IncrementDropped(1);
#endif

        int plinkoReward = PlayerPrefs.GetInt("PlinkoReward");
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        // input checking
        if (self == null)
        {
            Debug.LogError("PlinkoManager.MainReward() : Error : Plinko Reward not given.");
            UIManagerScript.Instance.SetErrorMessage("Error: Plinko Reward not given.");
            return;
        }
        if (plinkoReward < 0)
        {
            plinkoReward = 0;
            PlayerPrefs.SetInt("PlinkoReward", 0);
        }

        // SFX for auditory feedback
        SoundManagerScript.Instance.PlayWinSFX();

        GameObject floatingText = Instantiate(floatingTextPrefab, self.position, Quaternion.identity, transform);

        // give the reward to the player
        // skin reward
        if (IDisPlinkoUnlockable(plinkoReward))
        {
            // if the puck has not been unlocked already, unlock it. otherwise it is a duplicate and we should grant a drop
            if (!PuckSkinManager.Instance.IsPlinkoSkinUnlocked(plinkoReward))
            {
                PuckSkinManager.Instance.UnlockPuckID(plinkoReward, true);
                PuckSkinManager.Instance.UnlockPlinkoSkin(plinkoReward);
                UIManagerScript.Instance.SetErrorMessage("New puck unlocked!");
                rewardText.text = "";
                // visual feedback for reward
                UIManagerScript.Instance.SetErrorMessage("Unlocked: New Shuffleplink skin.");
                floatingText.GetComponent<FloatingTextScript>().Initialize("UNLOCKED!", 0.5f, 15);
            }
            else
            {
                PlayerPrefs.SetInt("PlinkoPegsDropped", PlayerPrefs.GetInt("PlinkoPegsDropped") - 1);
                plinkoDropButton.GetComponent<PlinkoDropButtonScript>().SetDropButtonText();
                // visual feedback for reward
                UIManagerScript.Instance.SetErrorMessage("Duplicate reward. Adding +1 drop instead.");
                floatingText.GetComponent<FloatingTextScript>().Initialize("+1 drop", 0.5f, 15);
            }
        }
        // plus packs
        else if (plinkoReward <= 0 && (int)(level / 10f) >= 1)
        {
            // give the reward to the player
            PackManager.Instance.RewardPacks(true, (int)(level / 10f));

            // floating text for visual feedback
            floatingText.GetComponent<FloatingTextScript>().Initialize((int)(level / 10f) > 1 ? $"+{(int)(level / 10f)} +PACKS" : $"+{(int)(level / 10f)} +PACK", 0.5f, 15);
        }
        // XP reward
        else
        {
            // if the reward would be 0 or negative XP, default it to the fallbackXPReward
            if (plinkoReward <= 0)
            {
                plinkoReward = fallbackXPReward;
                PlayerPrefs.SetInt("PlinkoReward", fallbackXPReward);
            }
            // reward the player with the XP
            LevelManager.Instance.AddXP(plinkoReward);
            // floating text for visual feedback
            floatingText.GetComponent<FloatingTextScript>().Initialize("+" + plinkoReward + "XP", 0.5f, 15);
        }
    }

    public void SideReward(Transform self)
    {
#if UNITY_EDITOR
        IncrementDropped(2);
#endif
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
#if UNITY_EDITOR
        IncrementDropped(3);
#endif
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