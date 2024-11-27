using TMPro;
using UnityEngine;

public class HighscoreTextScript : MonoBehaviour
{
    [SerializeField] private int difficulty;
    [SerializeField] private TMP_Text highscoreText;
    private string[] highscoresPlayerPrefsKeys = { "easyHighscore", "mediumHighscore", "hardHighscore" };

    private void OnEnable()
    {
        int highscore = PlayerPrefs.GetInt(highscoresPlayerPrefsKeys[difficulty]);
        highscoreText.text = highscore > 0 ? highscore.ToString() : "";
        highscoreText.fontWeight = FontWeight.Black;
        highscoreText.outlineWidth = 0.3f;
        highscoreText.outlineColor = new Color32(0, 0, 0, 255);
    }
}
