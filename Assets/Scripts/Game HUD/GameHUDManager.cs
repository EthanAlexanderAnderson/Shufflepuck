using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    [SerializeField] private GameObject playerScoreBackground;
    [SerializeField] private GameObject playerScoreParticleEffect;
    private ParticleSystem.MainModule playerScoreParticleSystemMainModule;
    [SerializeField] private GameObject playerPuckCount;
    [SerializeField] private GameObject playerPuckCountText;
    [SerializeField] private GameObject playerPuckCountIcon;
    [SerializeField] private GameObject playerPuckCountBackground;
    [SerializeField] private GameObject playerWins;
    [SerializeField] private GameObject playerWinsText;
    [SerializeField] private GameObject playerWinsIcon;
    [SerializeField] private GameObject playerWinsBackground;
    private float playerStartXLocalPos = -1340;
    private float playerEndXLocalPos = -840;

    private GameObject[] opponentHUDElements;
    private GameObject[] opponentHUDTexts;
    private GameObject[] opponentHUDIcons;
    [SerializeField] private GameObject opponentScore;
    [SerializeField] private GameObject opponentScoreText;
    [SerializeField] private GameObject opponentScoreIcon;
    [SerializeField] private GameObject opponentScoreBackground;
    [SerializeField] private GameObject opponentScoreParticleEffect;
    private ParticleSystem.MainModule opponentScoreParticleSystemMainModule;
    [SerializeField] private GameObject opponentPuckCount;
    [SerializeField] private GameObject opponentPuckCountText;
    [SerializeField] private GameObject opponentPuckCountIcon;
    [SerializeField] private GameObject opponentPuckCountBackground;
    [SerializeField] private GameObject opponentWins;
    [SerializeField] private GameObject opponentWinsText;
    [SerializeField] private GameObject opponentWinsIcon;
    [SerializeField] private GameObject opponentWinsBackground;
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

        // setup score particles
        playerScoreParticleSystemMainModule = playerScoreParticleEffect.GetComponent<ParticleSystem>().main;
        playerScoreParticleSystemMainModule.startColor = UIManagerScript.Instance.GetDarkMode() ? Color.white : Color.black;
        playerScoreParticleSystemMainModule.startSpeed = 0;

        opponentScoreParticleSystemMainModule = opponentScoreParticleEffect.GetComponent<ParticleSystem>().main;
        opponentScoreParticleSystemMainModule.startColor = UIManagerScript.Instance.GetDarkMode() ? Color.white : Color.black;
        opponentScoreParticleSystemMainModule.startSpeed = 0;
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

        playerScoreText.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        opponentScoreText.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        playerPuckCountText.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        opponentPuckCountText.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
    }

    public void ChangeScoreText(bool isPlayers, string score, bool playAnimation)
    {
        var textObject = isPlayers ? playerScoreText : opponentScoreText;
        var text = textObject.GetComponent<Text>();

        var iconObject = isPlayers ? playerScoreIcon : opponentScoreIcon;

        var backgroundObject = isPlayers ? playerScoreBackground : opponentScoreBackground;

        if (playAnimation)
        {
            // reset position
            LeanTween.cancel(textObject);
            LeanTween.cancel(iconObject);
            LeanTween.cancel(backgroundObject);
            textObject.transform.localRotation = new Quaternion(0f, 0f, Random.Range(-20f, 20f), 0f);
            textObject.transform.localScale = new Vector3(2f, 2f, 2f);
            iconObject.transform.localScale = new Vector3(2f, 2f, 2f);
            LeanTween.color(backgroundObject.GetComponent<Image>().rectTransform, Color.white, 0.01f);
        }

        // set text
        text.text = score;

        if (playAnimation)
        {
            // animation
            LeanTween.color(backgroundObject.GetComponent<Image>().rectTransform, Color.black, 0.1f).setDelay(0.01f);
            LeanTween.rotateZ(textObject, 0, 0.7f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
            LeanTween.scale(textObject, new Vector3(1f, 1f, 1f), 0.7f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
            LeanTween.scale(iconObject, new Vector3(1f, 1f, 1f), 0.6f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
        }
    }

    public void ChangePuckCountText(bool isPlayers, string puckCount, bool playAnimation)
    {
        var textObject = isPlayers ? playerPuckCountText : opponentPuckCountText;
        var text = textObject.GetComponent<Text>();

        var iconObject = isPlayers ? playerPuckCountIcon : opponentPuckCountIcon;

        var backgroundObject = isPlayers ? playerPuckCountBackground : opponentPuckCountBackground;

        if (playAnimation)
        {
            // reset position
            LeanTween.cancel(textObject);
            LeanTween.cancel(iconObject);
            LeanTween.cancel(backgroundObject);
            textObject.transform.localRotation = new Quaternion(0f, 0f, Random.Range(-20f, 20f), 0f);
            textObject.transform.localScale = new Vector3(2f, 2f, 2f);
            iconObject.transform.localScale = new Vector3(2f, 2f, 2f);
            LeanTween.color(backgroundObject.GetComponent<Image>().rectTransform, Color.white, 0.01f);
        }

        // set text
        text.text = puckCount;

        if (playAnimation)
        {
            // animation
            LeanTween.color(backgroundObject.GetComponent<Image>().rectTransform, Color.black, 0.1f).setDelay(0.01f);
            LeanTween.rotateZ(textObject, 0, 0.7f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
            LeanTween.scale(textObject, new Vector3(1f, 1f, 1f), 0.7f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
            LeanTween.scale(iconObject, new Vector3(1f, 1f, 1f), 0.6f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
        }
    }

    public void ChangeTurnText(string turnTextString, bool playAnimation = true)
    {
        if (playAnimation)
        {
            // reset position
            LeanTween.cancel(turnText);
            if (turnTextString == "" || turnTextString == string.Empty)
            {
                // shrink out, then set text to blank
                LeanTween.scale(turnText, new Vector3(0f, 0f, 0f), 1f).setEase(LeanTweenType.easeInQuint).setDelay(0.01f);
                return;
            }
            else
            {
                // text should already be 0 scale, but just in case
                turnText.transform.localScale = new Vector3(0f, 0f, 0f);
            }
        }

        if (playAnimation && !(turnTextString == "" || turnTextString == string.Empty))
        {
            // set text, then scale in
            turnText.GetComponent<Text>().text = turnTextString;
            LeanTween.cancel(this.turnText);
            LeanTween.scale(this.turnText, new Vector3(1f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutQuint).setDelay(0.5f);
        }
    }

    public void UpdateParticleEffects(int playerScore, int opponentScore)
    {
        if (playerScore == opponentScore) // tie, disable both
        {
            playerScoreParticleSystemMainModule.startSpeed = 0;
            opponentScoreParticleSystemMainModule.startSpeed = 0;
        }
        else if (playerScore > opponentScore)
        {
            playerScoreParticleSystemMainModule.startSpeed = Math.Min(0.5f + ((float)playerScore - (float)opponentScore) / 2, 10);
            opponentScoreParticleSystemMainModule.startSpeed = 0;
        }
        else
        {
            playerScoreParticleSystemMainModule.startSpeed = 0;
            opponentScoreParticleSystemMainModule.startSpeed = Math.Min(0.5f + ((float)opponentScore - (float)playerScore) / 2, 10);
        }
    }
}