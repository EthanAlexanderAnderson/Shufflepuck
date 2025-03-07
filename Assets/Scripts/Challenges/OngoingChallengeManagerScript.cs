using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OngoingChallengeManagerScript : MonoBehaviour
{
    // self
    public static OngoingChallengeManagerScript Instance;
    // dependencies
    private LevelManager levelManager;
    private SoundManagerScript sound;

    [SerializeField] private GameObject titleScreen;

    [SerializeField] private TMP_Text challengeText;
    [SerializeField] private TMP_Text rewardText1;
    [SerializeField] private TMP_Text rewardText2;
    [SerializeField] private TMP_Text rewardText3;
    [SerializeField] private TMP_Text rewardText4;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button claim;
    [SerializeField] private GameObject glow;

    List<Challenge> ongoingChallenges;
    int numberOfOngoingChallenges;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    void Start()
    {
        levelManager = LevelManager.Instance;
        sound = SoundManagerScript.Instance;
    }

    public void SetText()
    {
        ongoingChallenges = ChallengeManager.Instance.challengeData.ongoingChallenges;
        numberOfOngoingChallenges = ongoingChallenges.Count;

        int OC = PlayerPrefs.GetInt("OngoingChallenge", 1);

        // if the challenge is completed, the value is negative
        claim.interactable = OC < 0;
        glow.SetActive(claim.interactable);

        // assert the challenge ID is within range, prevent index error
        if (OC >= numberOfOngoingChallenges || OC <= (numberOfOngoingChallenges * -1))
        {
            if (numberOfOngoingChallenges == 0)
            {
                Debug.LogError("Failed to load ongoing challenges");
                challengeText.text = "ERROR";
                claim.interactable = false;
                glow.SetActive(false);
                return;
            }
            else
            {
                OC = numberOfOngoingChallenges - 1; // all challenges complete
                PlayerPrefs.SetInt("OngoingChallenge", OC);
            }
        }

        challengeText.text = ongoingChallenges[Mathf.Abs(OC)].challengeText;
        progressText.text = Mathf.Abs(OC) + "/" + (numberOfOngoingChallenges - 1).ToString();
        // TODO: make this dynamic to support non-XP rewards on challenges
        rewardText1.text = ongoingChallenges[Mathf.Abs(OC)].GetXPReward().ToString() + " XP";

        LevelManager.Instance.SetText();
    }

    private void AdvanceOngoingChallenge()
    {
        int OC = PlayerPrefs.GetInt("OngoingChallenge", 1);

        PlayerPrefs.SetInt("OngoingChallenge", (Mathf.Abs(OC) + 1));

        Debug.Log($"New Ongoing Challenge assigned. " + PlayerPrefs.GetInt("OngoingChallenge", 1));

        SetText();
    }

    public void EvaluateChallenge(int difficulty, int scoreDifference, bool isOnline)
    {
        int OC = PlayerPrefs.GetInt("OngoingChallenge", 1);

        // assert the challenges IDs are within range, prevent index error
        if (OC >= numberOfOngoingChallenges || OC <= (numberOfOngoingChallenges * -1))
        {
            OC = numberOfOngoingChallenges - 1; // all challenges complete
            PlayerPrefs.SetInt("OngoingChallenge", OC);
        }

        // Evalute condition is met
        if (ongoingChallenges[OC].CheckCompletion(scoreDifference, (isOnline ? -1 : difficulty)))
        {
            PlayerPrefs.SetInt("OngoingChallenge", -OC);
        }
        else
        {
            Debug.Log($"Challenge not complete. {OC} {scoreDifference} {(isOnline ? -1 : difficulty)}");
        }
    }

    // called by claim button
    public void ClaimReward()
    {
        int OC = PlayerPrefs.GetInt("OngoingChallenge", 1);

        // Check if the reward is complete (negative value)
        if (OC < 0)
        {
            // Add rewards
            LevelManager.Instance.AddXP(ongoingChallenges[Mathf.Abs(OC)].GetXPReward());

            Debug.Log("Claimed ongoing reward!");
            AdvanceOngoingChallenge();
            // TODO: evalute if challenge is complete here (combined highscore, win a match on easy, etc.)
            SetText();
            sound.PlayWinSFX();
        }
        titleScreen.GetComponent<TitleScreenScript>().UpdateAlerts();
    }
}
