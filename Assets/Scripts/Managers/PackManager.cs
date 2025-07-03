using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PackManager : MonoBehaviour
{
    // self
    public static PackManager Instance;

    [SerializeField] private GameObject cardOpenPrefab;

    [SerializeField] private GameObject packParent;

    [SerializeField] private TMP_Text standardPackCountText;
    [SerializeField] private TMP_Text plusPackCountText;

    [SerializeField] private Button openOneStandardButton;
    [SerializeField] private Button openTenStandardButton;
    [SerializeField] private Button openOnePlusButton;
    [SerializeField] private Button openTenPlusButton;

    [SerializeField] private TMP_Text bottomText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button openAnotherButton;

    [SerializeField] private GameObject PowerupPopupPrefab;
    [SerializeField] private GameObject boosterPopupParent;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text chanceMultText;
    private GameObject powerupPopupObject;
    // pack booster Ids & probability weights & required level
    List<(int id, int weight, int level)> packBoosterIDs = new()
    {
        (-5, 1, 50),
        (-4, 10, 25),
        (-3, 20, 15),
        (-2, 40, 10),
        (-1, 50, 0)
    };
    float[] boosterRankMult = { 0.1f, 0.08f, 0.04f, 0.02f, 0.002f };


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void Start()
    {
        powerupPopupObject = Instantiate(PowerupPopupPrefab, boosterPopupParent.transform);
        UpdatePackSreenUI();
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
            AssignNewPackBooster();
        }
    }

    private void CheckForNewDailyPackBooster()
    {
        // Try to read the DateTime from the "LastPackBoosterDate" PlayerPref
        if (DateTime.TryParseExact(PlayerPrefs.GetString("LastPackBoosterDate", string.Empty), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastPackBoosterDate))
        {
            // Compare the last saved date to today's date & make sure current date is NEWER than lastChallengeDate to prevent device time tampering
            if (DateTime.Today.Subtract(lastPackBoosterDate).Days >= 1)
            {
                AssignNewPackBooster();
            }
        }
        else // no date ever written, default plus one
        {
            PlayerPrefs.SetString("LastPackBoosterDate", DateTime.Today.ToString("yyyy-MM-dd"));
            PlayerPrefs.SetInt("PackBooster", 0);
            Debug.Log("New Pack Booster: 0");
        }
    }

    private void AssignNewPackBooster()
    {
        PlayerPrefs.SetString("LastPackBoosterDate", DateTime.Today.ToString("yyyy-MM-dd"));
        int currentPackBooster = PlayerPrefs.GetInt("PackBooster");

        // add unowned cards to weights list
        var sums = PowerupCardData.GetAllCardsOwnedSums();
        for (int i = 0; i < sums.Length; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            // large chance at boosting a specific card if you own zero
            if (sums[i] == 0)
            {
                packBoosterIDs.Add((i, (5 - PowerupCardData.GetCardRarity(i)) * 100, 0));
            }
            // small chance at boosting a specific card if you own less than a full set
            else if (sums[i] < (5 - PowerupCardData.GetCardRarity(i)))
            {
                packBoosterIDs.Add((i, (5 - PowerupCardData.GetCardRarity(i)) * 10, 0));
            }
        }

        // get total weight
        int totalWeight = 0;
        for (int i = 0; i < packBoosterIDs.Count; i++)
        {
            totalWeight += packBoosterIDs[i].weight;
        }

        // generate random
        int rand = Random.Range(0, (totalWeight - 1));
        int threshold = 0;
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        for (int i = 0; i < packBoosterIDs.Count; i++)
        {
            threshold += packBoosterIDs[i].weight;

            if (rand < threshold && packBoosterIDs[i].id != currentPackBooster && level >= packBoosterIDs[i].level)
            {
                PlayerPrefs.SetInt("PackBooster", packBoosterIDs[i].id);
                Debug.Log("New Pack Booster: " + PlayerPrefs.GetInt("PackBooster"));
                break;
            }
        }

        // if we have same boost as last time still, upgrade it if it's a holo/rank boost
        int newBoost = PlayerPrefs.GetInt("PackBooster");
        if (newBoost == currentPackBooster && newBoost < 0 && newBoost > -5)
        {
            PlayerPrefs.SetInt("PackBooster", newBoost - 1);
        }
    }

    public void UpdatePackSreenUI()
    {
        int standardPackCount = PlayerPrefs.GetInt("StandardPacks");
        int plusPackCount = PlayerPrefs.GetInt("PlusPacks");

        openOneStandardButton.interactable = standardPackCount >= 1;
        openTenStandardButton.interactable = standardPackCount >= 10;
        openOnePlusButton.interactable = plusPackCount >= 1;
        openTenPlusButton.interactable = plusPackCount >= 10;

        standardPackCountText.text = standardPackCount.ToString();
        plusPackCountText.text = plusPackCount.ToString();

        bottomText.text = "tap to open!";
        dupeCreditReward = 0;
        opened = 0;

        // pack booster
        CheckForNewDailyPackBooster();
        int currentReward = PlayerPrefs.GetInt("PackBooster");
        PowerupPopupPrefabScript powerupPopupScript = powerupPopupObject.GetComponent<PowerupPopupPrefabScript>();
        if (currentReward < PowerupCardData.GetCardCount())
        {
            powerupPopupScript.InitializePowerupPopup(currentReward, (currentReward * -1) - 1, currentReward == -1);
        }

        var (popupEffectIconObject, popupEffectIconOutlineObject, popupEffectTextObject, popupEffectRarityObject) = powerupPopupScript.GetObjects();
        popupEffectIconObject.transform.localScale = new Vector3(1f, 1f, 1f);
        popupEffectIconOutlineObject.transform.localScale = new Vector3(1f, 1f, 1f);
        popupEffectTextObject.transform.localScale = new Vector3(1f, 1f, 1f);
        popupEffectRarityObject.transform.localScale = new Vector3(1f, 1f, 1f);

        var (_, level) = LevelManager.Instance.GetXPAndLevel();
        int packBooster = PlayerPrefs.GetInt("PackBooster");
        float mult;
        if (packBooster >= 0)
        {
            mult = 1.5f + (level * 0.1f);
        }
        else
        {
            int index = (packBooster * -1) - 1;
            mult = 1f + (level * boosterRankMult[index]);
        }
        chanceMultText.text = $"x{Math.Truncate(100 * mult) / 100}";
    }

    public void RewardPacks(bool isPlus, int count)
    {
        string key = isPlus ? "PlusPacks" : "StandardPacks";
        PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key) + count);
        UpdatePackSreenUI();
    }

    private int dupeCreditReward;
    public void RewardCraftingCredits(int credits)
    {
        PlayerPrefs.SetInt("CraftingCredits", PlayerPrefs.GetInt("CraftingCredits") + credits);
        dupeCreditReward += credits;
    }

    private int opened;
    private int targetOpened;
    public void ShowBottomText()
    {
        opened++;
        if (opened >= targetOpened)
        {
            bottomText.text = dupeCreditReward > 0 ? ("+" + dupeCreditReward.ToString() + " credits") : "NEW";
            backButton.interactable = true;
            openAnotherButton.interactable = PlayerPrefs.GetInt(!openAnotherPlus ? "StandardPacks" : "PlusPacks") > 0;
        }
    }

    public void OpenStandardOne()
    {
        // validate we own a pack
        int standardPackCount = PlayerPrefs.GetInt("StandardPacks");
        if (standardPackCount < 1)
        {
            UIManagerScript.Instance.SetErrorMessage("not enough packs");
            return;
        }

        // consume pack
        PlayerPrefs.SetInt("StandardPacks", standardPackCount - 1);
        UpdatePackSreenUI();

        // open the pack
        GameObject pack = Instantiate(cardOpenPrefab, packParent.transform);
        pack.transform.localScale = new Vector3(3f, 3f, 3f);
        int cardIndex = RollRarity();
        pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(cardIndex, RollRankStandard(cardIndex), RollHolo(false));
        targetOpened = 1;
        openAnotherPlus = false;
        openAnotherx10 = false;
        openAnotherButtonObject.sprite = !openAnotherPlus ? openAnotherStandardSprite : openAnotherPlusSprite;
        DailyChallengeManagerScript.Instance.PackOpenDailyChallengeHelper(0);
        PlayerPrefs.SetInt("StandardPacksOpened", PlayerPrefs.GetInt("StandardPacksOpened") + 1);

        // disable buttons
        backButton.interactable = false;
        openAnotherButton.interactable = false;
    }

    public void OpenStandardTen()
    {
        // validate we own 10 packs
        int standardPackCount = PlayerPrefs.GetInt("StandardPacks");
        if (standardPackCount < 10)
        {
            UIManagerScript.Instance.SetErrorMessage("not enough packs");
            return;
        }

        // consume pack
        PlayerPrefs.SetInt("StandardPacks", standardPackCount - 10);
        UpdatePackSreenUI();

        // god pack chance (1 in 1000 : 0.1%)
        bool godpack = Random.Range(0, 1000) < 1;

        // open the packs
        for (int i = 0; i < 10; i++)
        {
            GameObject pack = Instantiate(cardOpenPrefab, packParent.transform);
            pack.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

            if (i < 3)
            {
                pack.transform.localPosition = new Vector3(-600f, 350f - 500f * i, 0f);
            }
            else if (i < 7)
            {
                pack.transform.localPosition = new Vector3(0f, 600f - 500f * (i - 3), 0f);
            }
            else if (i < 10)
            {
                pack.transform.localPosition = new Vector3(600f, 350f - 500f * (i-7), 0f);
            }

            int cardIndex = RollRarity();
            pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(cardIndex, RollRankStandard(cardIndex), RollHolo(false) || godpack);
        }
        targetOpened = 10;
        openAnotherPlus = false;
        openAnotherx10 = true;
        openAnotherButtonObject.sprite = !openAnotherPlus ? openAnotherStandardSprite : openAnotherPlusSprite;
        DailyChallengeManagerScript.Instance.PackOpenDailyChallengeHelper(0);
        PlayerPrefs.SetInt("StandardPacksOpened", PlayerPrefs.GetInt("StandardPacksOpened") + 10);

        // disable buttons
        backButton.interactable = false;
        openAnotherButton.interactable = false;
    }

    public void OpenPlusOne()
    {
        // validate we own a plus pack
        int plusPackCount = PlayerPrefs.GetInt("PlusPacks");
        if (plusPackCount < 1)
        {
            UIManagerScript.Instance.SetErrorMessage("not enough packs");
            return;
        }

        // consume pack
        PlayerPrefs.SetInt("PlusPacks", plusPackCount - 1);
        UpdatePackSreenUI();

        GameObject pack = Instantiate(cardOpenPrefab, packParent.transform);
        pack.transform.localScale = new Vector3(3f, 3f, 3f);
        int cardIndex = RollRarity();
        pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(cardIndex, RollRankPlus(cardIndex), RollHolo(true));
        targetOpened = 1;
        openAnotherPlus = true;
        openAnotherx10 = false;
        openAnotherButtonObject.sprite = !openAnotherPlus ? openAnotherStandardSprite : openAnotherPlusSprite;
        DailyChallengeManagerScript.Instance.PackOpenDailyChallengeHelper(1);
        PlayerPrefs.SetInt("PlusPacksOpened", PlayerPrefs.GetInt("PlusPacksOpened") + 1);

        // disable buttons
        backButton.interactable = false;
        openAnotherButton.interactable = false;
    }

    public void OpenPlusTen()
    {
        // validate we own 10 plus packs
        int plusPackCount = PlayerPrefs.GetInt("PlusPacks");
        if (plusPackCount < 10)
        {
            UIManagerScript.Instance.SetErrorMessage("not enough packs");
            return;
        }

        // consume pack
        PlayerPrefs.SetInt("PlusPacks", plusPackCount - 10);
        UpdatePackSreenUI();

        // god pack chance (1 in 100 : 1%)
        bool godpack = Random.Range(0, 100) < 1;

        // open packs
        for (int i = 0; i < 10; i++)
        {
            GameObject pack = Instantiate(cardOpenPrefab, packParent.transform);
            pack.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

            if (i < 3)
            {
                pack.transform.localPosition = new Vector3(-600f, 350f - 500f * i, 0f);
            }
            else if (i < 7)
            {
                pack.transform.localPosition = new Vector3(0f, 600f - 500f * (i - 3), 0f);
            }
            else if (i < 10)
            {
                pack.transform.localPosition = new Vector3(600f, 350f - 500f * (i - 7), 0f);
            }

            int cardIndex = RollRarity();
            pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(cardIndex, RollRankPlus(cardIndex), RollHolo(true) || godpack);
        }
        targetOpened = 10;
        openAnotherPlus = true;
        openAnotherx10 = true;
        openAnotherButtonObject.sprite = !openAnotherPlus ? openAnotherStandardSprite : openAnotherPlusSprite;
        DailyChallengeManagerScript.Instance.PackOpenDailyChallengeHelper(1);
        PlayerPrefs.SetInt("PlusPacksOpened", PlayerPrefs.GetInt("PlusPacksOpened") + 10);

        // disable buttons
        backButton.interactable = false;
        openAnotherButton.interactable = false;
    }

    private int RollRarity()
    {
        int returnValue = int.MaxValue;

        // get the player level for overrides. Players should not get rare cards at low levels (before they understand the value).
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        int packBooster = PlayerPrefs.GetInt("PackBooster");
        int boosterPoints = packBooster >= 0 ? 50 + level * 10 : 0;

        if (packBooster >= 0) Debug.Log("card booster points: " + boosterPoints);

        while (boosterPoints >= 0 && returnValue != packBooster)
        {
            int rand = Random.Range(0, 1000);
            if (rand < 10 && level >= 10)           // legendary : 0 - 9 : 10 in 1000 : 1%
            {
                returnValue = PowerupCardData.GetRandomCardOfRarity(4);
            }
            else if (rand < 60 && level >= 5)      // epic : 10 - 59 : 50 in 1000 : 5%
            {
                returnValue = PowerupCardData.GetRandomCardOfRarity(3);
            }
            else if (rand < 180)                    // rare : 60 - 179 : 120 in 1000 : 12%
            {
                returnValue = PowerupCardData.GetRandomCardOfRarity(2);
            }
            else if (rand < 460)                    // uncommon : 180 - 459 : 280 in 1000 : 28%
            {
                returnValue = PowerupCardData.GetRandomCardOfRarity(1);
            }
            else                                    // common : 460 - 999 : 550 in 1000 : 54%
            {
                returnValue = PowerupCardData.GetRandomCardOfRarity(0);
            }

            // if boosterPoints >= 100 (additional +x1 chance), reroll
            if (boosterPoints >= 100)
            {
                boosterPoints -= 100;
            }
            else
            {
                // if boosterPoints < 100, reroll with that percentage chance (15 booster points means 15% percent chance to reroll)
                int boosterRand = Random.Range(0, 100);
                if (boosterPoints > boosterRand)
                {
                    boosterPoints = 0; // reroll once
                }
                else
                {
                    boosterPoints = -1; // don't reroll
                }
            }
        }

        return returnValue;
    }

    private int RollRankStandard(int cardIndex)
    {
        int returnValue = 9999999;

        // get the player level for overrides. Players should not get rare ranks at low levels (before they understand the value).
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        int packBooster = (PlayerPrefs.GetInt("PackBooster") * -1) - 1;
        int boosterPoints = 0;
        if (packBooster > 0 && packBooster < 5)
        {
            boosterPoints = (int)(level * (float)(boosterRankMult[packBooster] * 100));
            Debug.Log("rank booster points: " + boosterPoints);
        }

        while (boosterPoints >= 0 && returnValue != packBooster)
        {
            int rand = Random.Range(0, 1000);
            if (rand < 1 && level >= 25)         // Celestial : 0 only : 1 in 1000 : 0.1%
            {
                returnValue = 4;
            }
            else if (rand < 11 && level >= 15)   // Diamond : 1 - 10 : 10 in 1000 : 1%
            {
                returnValue = 3;
            }
            else if (rand < 31 && level >= 10)   // Gold : 11 - 30 : 20 in 1000 : 2%
            {
                returnValue = 2;
            }
            else if (rand < 71 && level >= 5)    // Bronze : 31 - 70 : 40 in 1000 : 4%
            {
                returnValue = 1;
            }
            else                                // Standard : 71 - 1000 : 929 in 1000 : 92.9%
            {
                returnValue = 0;
            }

            // if boosterPoints >= 100 (additional +x1 chance), reroll
            if (boosterPoints >= 100)
            {
                boosterPoints -= 100;
            }
            else
            {
                // if boosterPoints < 100, reroll with that percentage chance (15 booster points means 15% percent chance to reroll)
                int boosterRand = Random.Range(0, 100);
                if (boosterPoints > boosterRand)
                {
                    boosterPoints = 0; // reroll once
                }
                else
                {
                    boosterPoints = -1; // don't reroll
                }
            }
        }

        // Downgrade to best undiscovered rank
        returnValue = DowngradeRankIfNeeded(returnValue, cardIndex);

        return returnValue;
    }

    private int RollRankPlus(int cardIndex)
    {
        int returnValue = 9999999;

        // get the player level for overrides. Players should not get rare ranks at low levels (before they understand the value).
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        int packBooster = (PlayerPrefs.GetInt("PackBooster") * -1) - 1;
        int boosterPoints = 0;
        if (packBooster > 0 && packBooster < 5)
        {
            boosterPoints = (int)(level * (float)(boosterRankMult[packBooster] * 100));
            Debug.Log("rank booster points: " + boosterPoints);
        }

        while (boosterPoints >= 0 && returnValue != packBooster)
        {
            int rand = Random.Range(0, 1000);
            if (rand < 10 && level >= 25)       // Celestial: 0 - 10 : 10 in 1000 : 1%
            {
                returnValue = 4;
            }
            else if (rand < 110)                // Diamond : 10 - 110 : 100 in 1000 : 10%
            {
                returnValue = 3;
            }
            else if (rand < 360)                // Gold : 110 - 360 : 250 in 1000 : 25%
            {
                returnValue = 2;
            }
            else                                // Bronze : 360 - 1000 : 650 in 1000 : 64%
            {
                returnValue = 1;
            }

            // if boosterPoints >= 100 (additional +x1 chance), reroll
            if (boosterPoints >= 100)
            {
                boosterPoints -= 100;
            }
            else
            {
                // if boosterPoints < 100, reroll with that percentage chance (15 booster points means 15% percent chance to reroll)
                int boosterRand = Random.Range(0, 100);
                if (boosterPoints > boosterRand)
                {
                    boosterPoints = 0; // reroll once
                }
                else
                {
                    boosterPoints = -1; // don't reroll
                }
            }
        }

        // Downgrade to worst undiscovered rank
        returnValue = DowngradeRankIfNeeded(returnValue, cardIndex);
        if (returnValue <= 0) returnValue = 1;

        return returnValue;
    }

    // this makes sure you unlock the ranks somewhat in order, for crafting purposes
    private int DowngradeRankIfNeeded(int returnValue, int cardIndex)
    {
        if (returnValue <= 0) return 0;

        // Downgrade to worst undiscovered rank
        try
        {
            // figure out which ranks we already own of this cardIndex
            bool[] owns = new bool[4];
            foreach (var v in PowerupCardData.GetAllVariations(cardIndex))
                if (v.rank >= 0 && v.rank < 4 && !v.holo)
                    owns[v.rank] = true;

                // downgrade by one rank if we don't own it
                while (returnValue > 0 && !owns[returnValue - 1])
                    returnValue--;
        }
        catch (Exception)
        {
            Debug.LogWarning("Failed to check for downgrade.");
        }

        return returnValue;
    }

    private bool RollHolo(bool isPlus)
    {
        bool returnValue = false;

        // get the player level for overrides. Players should not get rare ranks at low levels (before they understand the value).
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        int packBooster = (PlayerPrefs.GetInt("PackBooster"));
        int boosterPoints = (packBooster == -1) ? level * 10 : 0;

        if (packBooster == -1) Debug.Log("holo booster points: " + boosterPoints);

        while (boosterPoints >= 0 && !returnValue)
        {
            returnValue = Random.Range(0, 100) < (isPlus ? 25 : 5);

            // if boosterPoints >= 100 (additional +x1 chance), reroll
            if (boosterPoints >= 100)
            {
                boosterPoints -= 100;
            }
            else
            {
                // if boosterPoints < 100, reroll with that percentage chance (15 booster points means 15% percent chance to reroll)
                int boosterRand = Random.Range(0, 100);
                if (boosterPoints > boosterRand)
                {
                    boosterPoints = 0; // reroll once
                }
                else
                {
                    boosterPoints = -1; // don't reroll
                }
            }
        }

        return returnValue && level >= 5;
    }

    public void ClearPackUIs()
    {
        foreach (Transform child in packParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    bool openAnotherPlus = false;
    bool openAnotherx10 = false;
    [SerializeField] private Image openAnotherButtonObject;
    [SerializeField] private Sprite openAnotherStandardSprite;
    [SerializeField] private Sprite openAnotherPlusSprite;

    public void OpenAnother()
    {
        if (!openAnotherPlus && !openAnotherx10)
        {
            OpenStandardOne();
        }
        else if (!openAnotherPlus && openAnotherx10)
        {
            OpenStandardTen();
        }
        else if (openAnotherPlus && !openAnotherx10)
        {
            OpenPlusOne();
        }
        else if (openAnotherPlus && openAnotherx10)
        {
            OpenPlusTen();
        }
        openAnotherButtonObject.sprite = !openAnotherPlus ? openAnotherStandardSprite : openAnotherPlusSprite;
        bottomText.text = "tap to open!";
    }
}
