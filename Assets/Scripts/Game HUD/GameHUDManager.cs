using UnityEngine;
using UnityEngine.UI;

public class GameHUDManager : MonoBehaviour
{
    // self
    public static GameHUDManager Instance;

    private GameObject[] playerHUDElements;
    private GameObject[] playerHUDTexts;
    private GameObject[] playerHUDIcons;
    [SerializeField] private GameObject playerScore;
    [SerializeField] private GameObject playerScoreText;
    [SerializeField] private GameObject playerScoreIcon;
    [SerializeField] private GameObject playerPuckCount;
    [SerializeField] private GameObject playerPuckCountText;
    [SerializeField] private GameObject playerPuckCountIcon;
    [SerializeField] private GameObject playerWins;
    [SerializeField] private GameObject playerWinsText;
    [SerializeField] private GameObject playerWinsIcon;
    private float playerStartXLocalPos = -1340;
    private float playerEndXLocalPos = -840;

    private GameObject[] opponentHUDElements;
    private GameObject[] opponentHUDTexts;
    private GameObject[] opponentHUDIcons;
    [SerializeField] private GameObject opponentScore;
    [SerializeField] private GameObject opponentScoreText;
    [SerializeField] private GameObject opponentScoreIcon;
    [SerializeField] private GameObject opponentPuckCount;
    [SerializeField] private GameObject opponentPuckCountText;
    [SerializeField] private GameObject opponentPuckCountIcon;
    [SerializeField] private GameObject opponentWins;
    [SerializeField] private GameObject opponentWinsText;
    [SerializeField] private GameObject opponentWinsIcon;
    private float opponentStartXLocalPos = 1340;
    private float opponentEndXLocalPos = 840;

    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject restartButton;

    [SerializeField] private GameObject turnText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        if (-Screen.width < playerStartXLocalPos) { playerStartXLocalPos = -Screen.width; }
        if (Screen.width > opponentStartXLocalPos) { opponentStartXLocalPos = Screen.width; }

        playerHUDElements = new GameObject[] { playerScore, playerPuckCount, playerWins };
        playerHUDTexts = new GameObject[] { playerScoreText, playerPuckCountText, playerWinsText };
        playerHUDIcons = new GameObject[] { playerScoreIcon, playerPuckCountIcon, playerWinsIcon };

        opponentHUDElements = new GameObject[] { opponentScore, opponentPuckCount, opponentWins };
        opponentHUDTexts = new GameObject[] { opponentScoreText, opponentPuckCountText, opponentWinsText };
        opponentHUDIcons = new GameObject[] { opponentScoreIcon, opponentPuckCountIcon, opponentWinsIcon };
        Reset();
    }


    private void OnEnable()
    {
        Reset();
        for (int i = 0; i < playerHUDElements.Length; i++)
        {
            LeanTween.cancel(playerHUDElements[i]);
            LeanTween.moveLocalX(playerHUDElements[i], playerEndXLocalPos, 1f).setEase(LeanTweenType.easeOutElastic).setDelay((0.2f * i) + 0.1f);
        }

        for (int i = 0; i < playerHUDTexts.Length; i++)
        {
            LeanTween.cancel(playerHUDTexts[i]);
            LeanTween.scale(playerHUDTexts[i], new Vector3(1f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutElastic).setDelay((0.2f * i) + 0.3f);
        }

        for (int i = 0; i < playerHUDIcons.Length; i++)
        {
            LeanTween.cancel(playerHUDIcons[i]);
            LeanTween.scale(playerHUDIcons[i], new Vector3(1f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutBack).setDelay((0.2f * i) + 0.2f);
        }

        for (int i = 0; i < opponentHUDElements.Length; i++)
        {
            LeanTween.cancel(opponentHUDElements[i]);
            LeanTween.moveLocalX(opponentHUDElements[i], opponentEndXLocalPos, 1f).setEase(LeanTweenType.easeOutElastic).setDelay((0.2f * i) + 0.1f);
        }

        for (int i = 0; i < opponentHUDTexts.Length; i++)
        {
            LeanTween.cancel(opponentHUDTexts[i]);
            LeanTween.scale(opponentHUDTexts[i], new Vector3(1f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutElastic).setDelay((0.2f * i) + 0.3f);
        }

        for (int i = 0; i < opponentHUDIcons.Length; i++)
        {
            LeanTween.cancel(opponentHUDIcons[i]);
            LeanTween.scale(opponentHUDIcons[i], new Vector3(1f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutBack).setDelay((0.2f * i) + 0.2f);
        }

        backButton.transform.localScale = new Vector3(0f, 0f, 0f);
        restartButton.transform.localScale = new Vector3(0f, 0f, 0f);
        turnText.transform.localScale = new Vector3(0f, 0f, 0f);

        LeanTween.cancel(backButton);
        LeanTween.scale(backButton, new Vector3(1f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutBack).setDelay(0.4f);
        LeanTween.cancel(restartButton);
        LeanTween.scale(restartButton, new Vector3(1f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutBack).setDelay(0.4f); 
        LeanTween.cancel(turnText);
        LeanTween.scale(turnText, new Vector3(1f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutQuint).setDelay(0.6f);
    }

    public void Reset()
    {
        for (int i = 0; i < playerHUDElements.Length; i++)
        {
            playerHUDElements[i].transform.localPosition = new Vector3(playerStartXLocalPos, playerHUDElements[i].transform.localPosition.y, playerHUDElements[i].transform.localPosition.z);
        }
        for (int i = 0; i < playerHUDTexts.Length; i++)
        {
            playerHUDTexts[i].transform.localScale = new Vector3(0f, 0f, 0f);
        }
        for (int i = 0; i < playerHUDIcons.Length; i++)
        {
            playerHUDIcons[i].transform.localScale = new Vector3(0f, 0f, 0f);
        }
        for (int i = 0; i < opponentHUDElements.Length; i++)
        {
            opponentHUDElements[i].transform.localPosition = new Vector3(opponentStartXLocalPos, opponentHUDElements[i].transform.localPosition.y, opponentHUDElements[i].transform.localPosition.z);
        }
        for (int i = 0; i < opponentHUDTexts.Length; i++)
        {
            opponentHUDTexts[i].transform.localScale = new Vector3(0f, 0f, 0f);
        }
        for (int i = 0; i < opponentHUDIcons.Length; i++)
        {
            opponentHUDIcons[i].transform.localScale = new Vector3(0f, 0f, 0f);
        }

        backButton.transform.localScale = new Vector3(0f, 0f, 0f);
        restartButton.transform.localScale = new Vector3(0f, 0f, 0f);
        turnText.transform.localScale = new Vector3(0f, 0f, 0f);
    }

    public void ChangeScoreText(bool isPlayers, string score, bool playAnimation)
    {
        var textObject = isPlayers ? playerScoreText : opponentScoreText;
        var text = textObject.GetComponent<Text>();

        var iconObject = isPlayers ? playerScoreIcon : opponentScoreIcon;

        if (playAnimation)
        {
            // reset position
            LeanTween.cancel(textObject);
            textObject.transform.localRotation = new Quaternion(0f, 0f, Random.Range(-20f, 20f), 0f);
            textObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            iconObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }

        // set text
        text.text = score;

        if (playAnimation)
        {
            // animation
            LeanTween.rotateZ(textObject, 0, 0.7f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
            LeanTween.scale(textObject, new Vector3(1f, 1f, 1f), 0.7f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
            LeanTween.scale(iconObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
        }
    }
}