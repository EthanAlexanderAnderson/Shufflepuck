using UnityEngine;

public class TitleScreenScript : MonoBehaviour
{
    private LevelManager levelManager;

    public GameObject plinkoAlert;
    public GameObject puckAlert;
    public GameObject profileAlert;

    private void OnEnable()
    {
        levelManager = LevelManager.Instance;
        UpdateAlerts();
    }

    public void UpdateAlerts()
    {
        puckAlert.SetActive(PlayerPrefs.GetInt("ShowNewSkinAlert", 0) == 1);
        profileAlert.SetActive(PlayerPrefs.GetInt("DailyChallenge1", 0) < 0 || PlayerPrefs.GetInt("DailyChallenge2", 0) < 0 || PlayerPrefs.GetInt("OngoingChallenge", 0) < 0);
        (int XP, int level) = levelManager.GetXPAndLevel();
        plinkoAlert.SetActive(PlayerPrefs.GetInt("PlinkoPegsDropped", 0) < level);
    }
}
