using UnityEngine;

public class RewardsScreenScript : MonoBehaviour
{
    public GameObject plinkoAlert;
    public GameObject packAlert;

    private void OnEnable()
    {
        UpdateAlerts();
    }

    public void UpdateAlerts()
    {
        // set plinko alert active if we have drops and haven't unlocked the current reward
        (_, int level) = LevelManager.Instance.GetXPAndLevel();
        int plinkoReward = PlayerPrefs.GetInt("PlinkoReward", 0);
        bool unlockedReward = PuckSkinManager.Instance.IsPlinkoSkinUnlocked(plinkoReward);
        plinkoAlert.SetActive(PlayerPrefs.GetInt("PlinkoPegsDropped", 0) < level && !unlockedReward);
        packAlert.SetActive(PlayerPrefs.GetInt("StandardPacks", 0) > 0 || PlayerPrefs.GetInt("PlusPacks", 0) > 0);
    }
}
