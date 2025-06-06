using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    // self
    public static LevelManager Instance;

    private int level;
    private int XP;
    private float levelProgressBarValue;

    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private TMP_Text nextLevelText;
    [SerializeField] private TMP_Text xpText;

    [SerializeField] private Slider levelProgressBar;

    private int baseXPRequirement = 100;
    private int incrementXPRequirement = 10;
    private int maximumXPRequirement = 1000;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    void OnEnable()
    {
        LoadXP();
        SetText();
    }

    public void LoadXP()
    {
        XP = PlayerPrefs.GetInt("XP");
    }

    public void AddXP(int xp)
    {
        Debug.Log("AddXP called " + xp);
        XP += xp;
        PlayerPrefs.SetInt("XP", XP);
        SetText();
        OngoingChallengeManagerScript.Instance.EvaluateChallengeAndSetText();
    }

    public void SetText()
    {
        int XPiterator;
        (XPiterator, level) = GetXPAndLevel();

        currentLevelText.text = level.ToString();
        nextLevelText.text = (level + 1).ToString();
        int levelUpXpRequirement = Mathf.Min(maximumXPRequirement, baseXPRequirement + incrementXPRequirement * level);
        xpText.text = (XPiterator + levelUpXpRequirement).ToString() + "/" + (levelUpXpRequirement).ToString() + " XP";
        levelProgressBarValue = (((float)XPiterator + (float)levelUpXpRequirement) / (float)levelUpXpRequirement) * 100;
    }

    // gets current XP progress (not total XP) and current level
    public (int, int) GetXPAndLevel()
    {
        int XPiterator = PlayerPrefs.GetInt("XP");
        int leveliterator = -1;
        while (XPiterator >= 0)
        {
            leveliterator++;
            XPiterator -= Mathf.Min(maximumXPRequirement, baseXPRequirement + incrementXPRequirement * leveliterator);
        }
        return (XPiterator, leveliterator);
    }

    private void Update()
    {
        if (levelProgressBar.value < levelProgressBarValue)
        {
            levelProgressBar.value += (levelProgressBarValue  - levelProgressBar.value)/50f;
        }
        else if (levelProgressBar.value > (levelProgressBarValue + 1)) 
        {
            levelProgressBar.value = 0;
        }
    }
}
