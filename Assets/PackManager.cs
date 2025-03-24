using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackManager : MonoBehaviour
{
    [SerializeField] private GameObject cardOpenPrefab;

    [SerializeField] private GameObject packParent;

    [SerializeField] private TMP_Text standardPackCountText;
    [SerializeField] private TMP_Text plusPackCountText;

    [SerializeField] private Button openOneStandardButton;
    [SerializeField] private Button openTenStandardButton;
    [SerializeField] private Button openOnePlusButton;
    [SerializeField] private Button openTenPlusButton;


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

        // open the pack
        GameObject pack = Instantiate(cardOpenPrefab, packParent.transform);
        pack.transform.localScale = new Vector3(3f, 3f, 3f);
        pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(Random.Range(0, 30), RollRankStandard(), Random.Range(0, 100) < 5);

        UpdatePackSreenUI();
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

            pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(Random.Range(0, 30), RollRankStandard(), (Random.Range(0, 100) < 5) || godpack);
        }

        UpdatePackSreenUI();
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

        GameObject pack = Instantiate(cardOpenPrefab, packParent.transform);
        pack.transform.localScale = new Vector3(3f, 3f, 3f);
        pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(Random.Range(0, 30), RollRankPlus(), Random.Range(0, 100) < 25);

        UpdatePackSreenUI();
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

            pack.GetComponent<PackOpenPrefabScript>().InitializePackOpen(Random.Range(0, 30), RollRankPlus(), (Random.Range(0, 100) < 25) || godpack);
        }

        UpdatePackSreenUI();
    }

    private int RollRankStandard()
    {
        int rand = Random.Range(0, 1000);
        if (rand < 1)           // 0 only : 0.1%
        {
            return 4;           // Celestial
        }
        else if (rand < 11)     // 1 - 10 : 10 in 1000 : 1%
        {
            return 3;           // Diamond
        }
        else if (rand < 31)     // 11 - 30 : 20 in 1000 : 2%
        {
            return 2;           // Gold
        }
        else if (rand < 71)     // 31 - 70 : 40 in 1000 : 4%
        {
            return 1;           // Bronze
        }
        else
        {
            return 0;           // Standard
        }
    }

    private int RollRankPlus()
    {
        int rand = Random.Range(0, 1000);
        if (rand < 10)           // 0 - 10 : 10 in 1000 : 1%
        {
            return 4;           // Celestial
        }
        else if (rand < 110)    // 10 - 110 : 100 in 1000 : 10%
        {
            return 3;           // Diamond
        }
        else if (rand < 360)    // 110 - 360 : 250 in 1000 : 25%
        {
            return 2;           // Gold
        }
        else                    // 360 - 1000 : 650 in 1000 : 64%
        {
            return 1;           // Bronze
        }
    }

    public void ClearPackUIs()
    {
        foreach (Transform child in packParent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
