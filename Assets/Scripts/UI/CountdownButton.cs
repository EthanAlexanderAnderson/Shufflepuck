using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownButton : MonoBehaviour
{
    [SerializeField] private Image tintImage;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Button button;

    [SerializeField] private float countdownCurrentSeconds;
    [SerializeField] private float countdownTargetSeconds = 5;

    private void OnEnable()
    {
        countdownCurrentSeconds = 0;
        button.interactable = false;
    }

    void Update()
    {
        if (countdownTargetSeconds <= 0) return;

        tintImage.transform.localScale = new(1f - (countdownCurrentSeconds/countdownTargetSeconds), 1f);
        countdownText.text = ((int)(countdownTargetSeconds - countdownCurrentSeconds)).ToString();

        if (countdownCurrentSeconds >= countdownTargetSeconds)
        {
            if (!button.interactable)
            {
                button.interactable = true;
            }
            tintImage.transform.localScale = new(0f, 1f);
            countdownText.text = "";
        }
        else
        {
            countdownCurrentSeconds += Time.deltaTime;
        }
    }
}
