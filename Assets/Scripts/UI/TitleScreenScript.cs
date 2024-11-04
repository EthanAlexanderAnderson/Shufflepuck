using UnityEngine;

public class TitleScreenScript : MonoBehaviour
{
    public GameObject puckAlert;
    public GameObject profileAlert;

    private void OnEnable()
    {
        puckAlert.SetActive(PlayerPrefs.GetInt("ShowNewSkinAlert", 0) == 1);
        profileAlert.SetActive(PlayerPrefs.GetInt("DailyChallenge1", 0) < 0 || PlayerPrefs.GetInt("DailyChallenge2", 0) < 0);
    }
}
