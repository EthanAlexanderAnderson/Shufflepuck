using UnityEngine;

public class TitleScreenScript : MonoBehaviour
{
    // self
    public static TitleScreenScript Instance;

    public GameObject puckAlert;
    public GameObject rewardsAlert;
    public GameObject questsAlert;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void OnEnable()
    {
        UpdateAlerts();
    }

    public void UpdateAlerts()
    {
        puckAlert.SetActive(PlayerPrefs.GetInt("ShowNewSkinAlert", 0) == 1);
        questsAlert.SetActive(PlayerPrefs.GetInt("DailyChallenge1", 0) < 0 || PlayerPrefs.GetInt("DailyChallenge2", 0) < 0 || PlayerPrefs.GetInt("OngoingChallenge", 0) < 0);

        if (LevelManager.Instance == null || PuckSkinManager.Instance == null)
        {
            Debug.LogWarning("LevelManager or PuckSkinManager not initialized yet.");
            return;
        }

        // set plinko alert active if we have drops and haven't unlocked the current reward
        (_, int level) = LevelManager.Instance.GetXPAndLevel();
        int plinkoReward = PlayerPrefs.GetInt("PlinkoReward", 0);
        bool unlockedReward = PuckSkinManager.Instance.IsPlinkoSkinUnlocked(plinkoReward);
        rewardsAlert.SetActive((PlayerPrefs.GetInt("PlinkoPegsDropped", 0) < level && !unlockedReward) ||
                               (PlayerPrefs.GetInt("StandardPacks", 0) > 0 || PlayerPrefs.GetInt("PlusPacks", 0) > 0));
    }
}
