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

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    void OnEnable()
    {
        XP = PlayerPrefs.GetInt("XP", 0);
    }

    public void AddXP(int xp)
    {
        Debug.Log("AddXP called " + xp);
        XP += xp;
        PlayerPrefs.SetInt("XP", XP);
        SetText();
    }

    public void SetText()
    {
        int XPiterator;
        (XPiterator, level) = GetXPAndLevel();

        currentLevelText.text = level.ToString();
        nextLevelText.text = (level + 1).ToString();
        xpText.text = (XPiterator + 100 + 10 * level).ToString() + "/" + (100 + 10 * level).ToString() + " XP";
        levelProgressBarValue = (((float)XPiterator + 100 + 10 * (float)level) / (100 + 10 * (float)level) * 100);
    }

    // gets current XP progress (not total XP) and current level
    public (int, int) GetXPAndLevel()
    {
        int XPiterator = XP;
        int leveliterator = -1;
        while (XPiterator >= 0)
        {
            leveliterator++;
            XPiterator -= 100 + 10 * leveliterator;
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
