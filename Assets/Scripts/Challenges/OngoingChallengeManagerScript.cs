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
    [SerializeField] private TMP_Text allQuestsProgressText; // how many ongoing quests have been completed & how many are left
    [SerializeField] private Button claim;
    [SerializeField] private GameObject glow;
    // our progress into completing the current quest
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;


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

        // if uncompleted
        if (OC > 0)
        {
            // retroactively complete the challenge
            if (ongoingChallenges[OC].CheckCompletion())
            {
                PlayerPrefs.SetInt("OngoingChallenge", -OC);
                OC *= -1;
            }
        }

        // if the challenge is completed, the value is negative
        claim.interactable = OC < 0;
        glow.SetActive(OC < 0);

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

        // set description
        challengeText.text = ongoingChallenges[Mathf.Abs(OC)].challengeText;
        allQuestsProgressText.text = Mathf.Abs(OC) + "/" + (numberOfOngoingChallenges - 1).ToString();

        // set rewards
        List<string> rewardStrings = ongoingChallenges[Mathf.Abs(OC)].GetRewardStrings();
        TMP_Text[] rewardTexts = { rewardText1, rewardText2, rewardText3};
        for (int i = 0; i < rewardStrings.Count; i++)
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
        else // this is just for the final "no challenges remaing" challenge
        {
            progressBar.value = 100;
            progressText.text = "";
        }

        LevelManager.Instance.SetText();
    }

    private void AdvanceOngoingChallenge()
    {
        int OC = PlayerPrefs.GetInt("OngoingChallenge", 1);

        // assign next challenge & check its completion
        OC = Mathf.Abs(OC) + 1;
        if (ongoingChallenges[OC].CheckCompletion())
        {
            PlayerPrefs.SetInt("OngoingChallenge", -OC);
        }
        else
        {
            PlayerPrefs.SetInt("OngoingChallenge", OC);
            Debug.Log($"New Ongoing Challenge assigned. " + PlayerPrefs.GetInt("OngoingChallenge", 1));
        }

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

        // don't need to evaluate already-completed ongoing challenges
        if (OC < 0) return;

        // Evalute condition is met
        if (ongoingChallenges[OC].CheckCompletion())
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
            ongoingChallenges[Mathf.Abs(OC)].ClaimRewards();

            Debug.Log("Claimed ongoing reward!");
            AdvanceOngoingChallenge();
            SetText();
            sound.PlayWinSFX();
        }
        titleScreen.GetComponent<TitleScreenScript>().UpdateAlerts();
    }
}
