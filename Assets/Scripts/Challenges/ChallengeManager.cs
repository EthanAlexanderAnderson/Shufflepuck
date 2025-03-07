using UnityEngine;
using System.Collections.Generic;

public class ChallengeManager : MonoBehaviour
{
    // self
    public static ChallengeManager Instance;

    public ChallengeObject challengeData = new ChallengeObject();
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    void Start()
    {
        AddChallenges();
    }

    void AddChallenges()
    {
        // EASY DAILY CHALLENGES
        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Claimed",
            condition = new BeatByCondition { winByTargetPoints = 99999999, difficultyLevel = 3 },
            rewards = new List<Reward> { }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU",
            condition = new BeatByCondition { winByTargetPoints = 1, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 40 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 3 points",
            condition = new BeatByCondition { winByTargetPoints = 3, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 60 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 5 points",
            condition = new BeatByCondition { winByTargetPoints = 5, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 80 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 7 points",
            condition = new BeatByCondition { winByTargetPoints = 7, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 120 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 9 points",
            condition = new BeatByCondition { winByTargetPoints = 9, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 160 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 11 points",
            condition = new BeatByCondition { winByTargetPoints = 11, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 13 points",
            condition = new BeatByCondition { winByTargetPoints = 13, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 320 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 15 points",
            condition = new BeatByCondition { winByTargetPoints = 15, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 480 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 17 points",
            condition = new BeatByCondition { winByTargetPoints = 17, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 640 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU",
            condition = new BeatByCondition { winByTargetPoints = 1, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 60 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 3 points",
            condition = new BeatByCondition { winByTargetPoints = 3, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 90 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 5 points",
            condition = new BeatByCondition { winByTargetPoints = 5, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 120 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 7 points",
            condition = new BeatByCondition { winByTargetPoints = 7, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 180 }
            }
        });



        // HARD DAILY CHALLENGES
        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Claimed",
            condition = new BeatByCondition { winByTargetPoints = 99999999, difficultyLevel = 3 },
            rewards = new List<Reward> { }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU",
            condition = new BeatByCondition { winByTargetPoints = 1, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 80 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 3 points",
            condition = new BeatByCondition { winByTargetPoints = 3, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 120 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 5 points",
            condition = new BeatByCondition { winByTargetPoints = 5, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 160 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 7 points",
            condition = new BeatByCondition { winByTargetPoints = 7, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 9 points",
            condition = new BeatByCondition { winByTargetPoints = 9, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 320 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 11 points",
            condition = new BeatByCondition { winByTargetPoints = 11, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 480 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 13 points",
            condition = new BeatByCondition { winByTargetPoints = 13, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 640 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 15 points",
            condition = new BeatByCondition { winByTargetPoints = 15, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 960 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 9 points",
            condition = new BeatByCondition { winByTargetPoints = 9, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 11 points",
            condition = new BeatByCondition { winByTargetPoints = 11, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 360 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 13 points",
            condition = new BeatByCondition { winByTargetPoints = 13, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 480 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 15 points",
            condition = new BeatByCondition { winByTargetPoints = 15, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 720 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 17 points",
            condition = new BeatByCondition { winByTargetPoints = 17, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 960 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Win an online game",
            condition = new BeatByCondition { winByTargetPoints = 1, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 }
            }
        });



        // ONGOING CHALLENGES
        // 0 index can't exist because to complete a challenge means it is saved as a negative value
        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "ERROR",
            condition = new BeatByCondition { winByTargetPoints = 99999999, difficultyLevel = 3 },
            rewards = new List<Reward> { }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU",
            condition = new BeatByCondition { winByTargetPoints = 1, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU",
            condition = new BeatByCondition { winByTargetPoints = 1, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU",
            condition = new BeatByCondition { winByTargetPoints = 1, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 }
            }
        });

        // last challenge is uncompletable
        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "No ongoing quests remaining",
            condition = new BeatByCondition { winByTargetPoints = 99999999, difficultyLevel = 3 },
            rewards = new List<Reward> { }
        });

        // load challenges
        DailyChallengeManagerScript.Instance.SetText();
        OngoingChallengeManagerScript.Instance.SetText();
    }
}
