using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void OnEnable()
    {
        UpdatePackSreenUI();
    }

    private void UpdatePackSreenUI()
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
            bottomText.text = dupeCreditReward > 0 ? ("+" + dupeCreditReward.ToString() + " credits") : "";
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
        pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(RollRarity(), RollRankStandard(), Random.Range(0, 100) < 5);
        targetOpened = 1;
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

        // god pack chance
        bool godpack = false;
        if (Random.Range(0, 1000) < 1) // 1 in 1000 : 0.1% chance
        {
            godpack = true;
        }

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

            pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(RollRarity(), RollRankStandard(), (Random.Range(0, 100) < 5) || godpack);
        }
        targetOpened = 10;
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
        pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(RollRarity(), RollRankPlus(), Random.Range(0, 100) < 25);
        targetOpened = 1;
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

        // god pack chance
        bool godpack = false;
        if (Random.Range(0, 100) < 1) // 1 in 100 : 1% chance
        {
            godpack = true;
        }

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

            pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(RollRarity(), RollRankPlus(), (Random.Range(0, 100) < 25) || godpack);
        }
        targetOpened = 10;
    }

    private int RollRarity()
    {
        // get the player level for overrides. Players should not get rare cards at low levels (before they understand the value).
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        int rand = Random.Range(0, 1000);
        if (rand < 30 && level >= 10)           // legendary : 0 - 29 : 30 in 1000 : 3%
        {
            return PowerupCardData.GetRandomCardOfRarity(4);
        }
        else if (rand < 100 && level >= 5)      // epic : 30 - 99 : 70 in 1000 : 7%
        {
            return PowerupCardData.GetRandomCardOfRarity(3);
        }
        else if (rand < 250)                    // rare : 100 - 249 : 150 in 1000 : 15%
        {
            return PowerupCardData.GetRandomCardOfRarity(2);
        }
        else if (rand < 500)                    // uncommon : 250 - 499 : 250 in 1000 : 25%
        {
            return PowerupCardData.GetRandomCardOfRarity(1);
        }
        else                                    // common : 500 - 999 : 500 in 1000 : 50%
        {
            return PowerupCardData.GetRandomCardOfRarity(0);
        }
    }

    private int RollRankStandard()
    {
        // get the player level for overrides. Players should not get rare ranks at low levels (before they understand the value).
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        int rand = Random.Range(0, 1000);
        if (rand < 1 && level >= 25)         // Celestial : 0 only : 1 in 1000 : 0.1%
        {
            return 4;
        }
        else if (rand < 11 && level >= 15)   // Diamond : 1 - 10 : 10 in 1000 : 1%
        {
            return 3;
        }
        else if (rand < 31 && level >= 10)   // Gold : 11 - 30 : 20 in 1000 : 2%
        {
            return 2;
        }
        else if (rand < 71 && level >= 5)    // Bronze : 31 - 70 : 40 in 1000 : 4%
        {
            return 1;
        }
        else                                // Standard : 71 - 1000 : 929 in 1000 : 92.9%
        {
            return 0;
        }
    }

    private int RollRankPlus()
    {
        // get the player level for overrides. Players should not get rare ranks at low levels (before they understand the value).
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        int rand = Random.Range(0, 1000);
        if (rand < 10 && level >= 25)       // Celestial: 0 - 10 : 10 in 1000 : 1%
        {
            return 4;
        }
        else if (rand < 110)                // Diamond : 10 - 110 : 100 in 1000 : 10%
        {
            return 3;
        }
        else if (rand < 360)                // Gold : 110 - 360 : 250 in 1000 : 25%
        {
            return 2;
        }
        else                                // Bronze : 360 - 1000 : 650 in 1000 : 64%
        {
            return 1;
        }
    }

    public void ClearPackUIs()
    {
        foreach (Transform child in packParent.transform)
        {
            Destroy(child.gameObject);
        }
    }
    /*
    public void ResetBottomText()
    {
        bottomText.text = "tap to open!";
    }*/
}
