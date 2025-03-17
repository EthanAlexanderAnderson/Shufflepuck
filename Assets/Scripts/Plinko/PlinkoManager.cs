using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlinkoManager : MonoBehaviour
{
    // self
    public static PlinkoManager Instance;

    // imports
    [SerializeField] private Image rewardImage;
    [SerializeField] private GameObject rewardImageAnimation;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject bonusBucketLeft;
    [SerializeField] private GameObject bonusBucketRight;
    [SerializeField] private GameObject plinkoDropButton;

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

    private void Start()
    {
        CheckForNewDailyPlinkoReward();

        TitleScreenScript.Instance.UpdateAlerts();
    }

    private void OnEnable()
    {
        int PlinkoReward = PlayerPrefs.GetInt("PlinkoReward", 100);
        if (PlinkoReward >= 100) // 100+ = XP REWARD
        {
            rewardText.text = "+" + PlinkoReward.ToString() + "XP";
            rewardImageAnimation.SetActive(false);
        }
        else // skin reward
        {
            rewardText.text = "UNLOCK";
            rewardImage.sprite = PuckSkinManager.Instance.ColorIDtoPuckSprite(PlinkoReward);
            rewardImageAnimation.SetActive(PlinkoReward == 40);
        }
        rewardImage.enabled = PlinkoReward < 100; // reward image should be disabled if the reward is XP
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
    }

    private void AssignNewPlinkoReward()
    {
        PlayerPrefs.SetString("LastPlinkoRewardDate", DateTime.Today.ToString("yyyy-MM-dd"));

        int currentReward = PlayerPrefs.GetInt("PlinkoReward", 100);

        int totalWeight = 0;

        // Shallow copy so we don't override real weights
        int[,] plinkoUnlockableIDsCopy = (int[,])plinkoUnlockableIDs.Clone();

        for (int i = 0; i < plinkoUnlockableIDsCopy.GetLength(0); i++)
        {
            int puckID = plinkoUnlockableIDsCopy[i, 0];
            // if a puck has been unlocked already or was the previous reward, set it's weight (probability of being the next reward) to be zero
            if (PlayerPrefs.GetInt("puck" + puckID + "unlocked") == 1 || puckID == currentReward)
            {
                plinkoUnlockableIDsCopy[i, 1] = 0;
            }
            else
            {
                totalWeight += plinkoUnlockableIDsCopy[i, 1];
            }
        }

        // if we have no pucks remaining
        if (totalWeight == 0 && currentReward < 100)
        {
            // no pucks are left to unlock
            if (PlayerPrefs.GetInt("puck" + currentReward + "unlocked") == 1)
            {
                Debug.Log("No puck skin rewards remain.");
                PlayerPrefs.SetInt("PlinkoReward", 999);
                rewardImage.enabled = false;
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
                Debug.Log("New Plinko Reward: " + PlayerPrefs.GetInt("PlinkoReward"));
                return;
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
            UIManagerScript.Instance.SetErrorMessage("Error: Plinko Reward not given.");
            return;
        }

        // give the reward to the player
        if (plinkoreward >= 100) // XP reward
        {
            LevelManager.Instance.AddXP(plinkoreward);
        }
        else // skin reward
        {
            // if the puck has not been unlocked already, unlock it. otherwise it is a duplicate and we should grant XP
            if (PlayerPrefs.GetInt("puck" + plinkoreward.ToString() + "unlocked", 0) == 0)
            {
                PuckSkinManager.Instance.UnlockPuckID(plinkoreward);
                PlayerPrefs.SetInt("puck" + plinkoreward.ToString() + "unlocked", 1);
                UIManagerScript.Instance.SetErrorMessage("New puck unlocked!");
            }
            else
            {
                plinkoreward = 100;
                LevelManager.Instance.AddXP(plinkoreward);
                UIManagerScript.Instance.SetErrorMessage("Duplicate reward. Adding +100XP instead.");
            }
        }

        // SFX for auditory feedback
        SoundManagerScript.Instance.PlayWinSFX();

        // floating text for visual feedback
        var floatingText = Instantiate(floatingTextPrefab, self.position, Quaternion.identity, transform);
        floatingText.GetComponent<FloatingTextScript>().Initialize((plinkoreward >= 100) ? "+" + plinkoreward + "XP" : "UNLOCKED!", 0.5f, 15);
    }

    public void SideReward( Transform self )
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