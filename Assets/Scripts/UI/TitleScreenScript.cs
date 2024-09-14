using UnityEngine;

public class TitleScreenScript : MonoBehaviour
{
    public GameObject puckAlert;

    private void OnEnable()
    {
        puckAlert.SetActive(PlayerPrefs.GetInt("ShowNewSkinAlert", 0) == 1);
    }
}
