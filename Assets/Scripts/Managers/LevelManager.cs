using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    // self
    public static LevelManager Instance;

    private float levelProgressBarValue;

    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private TMP_Text nextLevelText;
    [SerializeField] private TMP_Text xpText;

    [SerializeField] private Slider levelProgressBar;

    private const int baseXPRequirement = 100;
    private const int incrementXPRequirement = 10;
    private const int maximumXPRequirement = 1000;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    void OnEnable()
    {
        SetText();
    }

    public void AddXP(int xp)
    {
        Debug.Log("AddXP called " + xp);
        PlayerPrefs.SetInt("XP", PlayerPrefs.GetInt("XP") + xp);
        SetText();
        OngoingChallengeManagerScript.Instance.EvaluateChallengeAndSetText();
    }

    public void SetText()
    {
        (int XPiterator, int level) = GetXPAndLevel();

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

    public int GetLevelUpXPRequirement()
    {
        (_, int level) = GetXPAndLevel();
        return Mathf.Min(maximumXPRequirement, baseXPRequirement + incrementXPRequirement * level);
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
