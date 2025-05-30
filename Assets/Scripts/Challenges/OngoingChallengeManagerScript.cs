using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OngoingChallengeManagerScript : MonoBehaviour
{
    // self
    public static OngoingChallengeManagerScript Instance;

    [SerializeField] private GameObject titleScreen;

    [SerializeField] private TMP_Text challengeText;
    [SerializeField] private TMP_Text rewardText1;
    [SerializeField] private TMP_Text rewardText2;
    [SerializeField] private TMP_Text rewardText3;
    [SerializeField] private TMP_Text allQuestsProgressText; // how many ongoing quests have been completed & how many are left
    [SerializeField] private Button claim;
    [SerializeField] private GameObject glow;
    // our progress into completing the current quest
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    // Called at start after the challenges are instantiated by ChallengeManager, whenever a match is won, and when the ongoing challenge is advanced
    public void EvaluateChallengeAndSetText()
    {
        int OC = PlayerPrefs.GetInt("OngoingChallenge", 1);

        List<Challenge> ongoingChallenges = ChallengeManager.Instance.challengeData.ongoingChallenges;
        int numberOfOngoingChallenges = ongoingChallenges.Count;

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

        // if uncompleted,
        if (OC > 0)
        {
            // retroactively complete the challenge
            if (ongoingChallenges[OC].CheckCompletion())
            {
                PlayerPrefs.SetInt("OngoingChallenge", -OC);
                OC *= -1;
            }
        }

        // set description
        challengeText.text = ongoingChallenges[Mathf.Abs(OC)].challengeText;
        allQuestsProgressText.text = Mathf.Abs(OC) + "/" + (numberOfOngoingChallenges - 1).ToString();

        // clear rewards text boxes
        TMP_Text[] rewardTexts = { rewardText1, rewardText2, rewardText3 };
        for (int i = 0; i < rewardTexts.Length; i++)
        {
            rewardTexts[i].text = "";
        }

        // set rewards text boxes
        List<string> rewardStrings = ongoingChallenges[Mathf.Abs(OC)].GetRewardStrings();
        for (int i = 0; (i < rewardStrings.Count && i < rewardTexts.Length); i++)
        {
            rewardTexts[i].text = rewardStrings[i];
        }

        // progress bar
        var (num, dem) = ongoingChallenges[Mathf.Abs(OC)].GetVariables();
        if (dem != int.MaxValue)
        {
            progressBar.value = ongoingChallenges[Mathf.Abs(OC)].GetProgress() * 100f;
            progressText.text = $"{num}/{dem}";
        }
        else // this is just for the final "no challenges remaining" challenge
        {
            progressBar.value = 100;
            progressText.text = "";
        }

        // if the challenge is completed (the value is negative), enable the claim button
        claim.interactable = OC < 0;
        glow.SetActive(OC < 0);
    }

    private void AdvanceOngoingChallenge()
    {
        int OC = PlayerPrefs.GetInt("OngoingChallenge", 1);

        // assign next challenge & check its completion
        PlayerPrefs.SetInt("OngoingChallenge", Mathf.Abs(OC) + 1);
        EvaluateChallengeAndSetText();
    }

    // called by claim button
    public void ClaimReward()
    {
        int OC = PlayerPrefs.GetInt("OngoingChallenge", 1);

        // Check if the reward is complete (negative value)
        if (OC < 0)
        {
            List<Challenge> ongoingChallenges = ChallengeManager.Instance.challengeData.ongoingChallenges;
            // Add rewards
            ongoingChallenges[Mathf.Abs(OC)].ClaimRewards();

            Debug.Log("Claimed ongoing reward!");
            AdvanceOngoingChallenge();
            SoundManagerScript.Instance.PlayWinSFX();
        }
        titleScreen.GetComponent<TitleScreenScript>().UpdateAlerts();
    }
}
