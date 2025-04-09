using UnityEngine;

public class AndroidFrameRateFix : MonoBehaviour
{
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Debug.Log("App regained focus. Reapplying target framerate.");
            ReapplyFramerate();
        }
        else
        {
            QualitySettings.vSyncCount = 1;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            Debug.Log("App resumed. Reapplying target framerate.");
            ReapplyFramerate();
        }
        else
        {
            QualitySettings.vSyncCount = 1;
        }
    }

    private void ReapplyFramerate()
    {
        QualitySettings.vSyncCount = 0;
        int targetFramerate = Mathf.Clamp(PlayerPrefs.GetInt("FPS", 90), 30, 120);
        Application.targetFrameRate = targetFramerate;
    }
}
