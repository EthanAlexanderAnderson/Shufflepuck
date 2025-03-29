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
                new Reward { type = RewardType.XP, amount = 320 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 15 points",
            condition = new BeatByCondition { winByTargetPoints = 15, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 480 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 17 points",
            condition = new BeatByCondition { winByTargetPoints = 17, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 640 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
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
                new Reward { type = RewardType.XP, amount = 120 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 5 points",
            condition = new BeatByCondition { winByTargetPoints = 5, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 160 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 7 points",
            condition = new BeatByCondition { winByTargetPoints = 7, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 9 points",
            condition = new BeatByCondition { winByTargetPoints = 9, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 320 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 11 points",
            condition = new BeatByCondition { winByTargetPoints = 11, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 480 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 13 points",
            condition = new BeatByCondition { winByTargetPoints = 13, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 640 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 15 points",
            condition = new BeatByCondition { winByTargetPoints = 15, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 960 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 9 points",
            condition = new BeatByCondition { winByTargetPoints = 9, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 11 points",
            condition = new BeatByCondition { winByTargetPoints = 11, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 360 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 13 points",
            condition = new BeatByCondition { winByTargetPoints = 13, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 480 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 15 points",
            condition = new BeatByCondition { winByTargetPoints = 15, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 720 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU by 17 points",
            condition = new BeatByCondition { winByTargetPoints = 17, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 960 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Win an online game",
            condition = new BeatByCondition { winByTargetPoints = 1, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 250 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
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
            condition = new HighscoreCondition { targetHighscore = 1, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 60 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an easy highscore of 3",
            condition = new HighscoreCondition { targetHighscore = 3, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 80 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU",
            condition = new HighscoreCondition { targetHighscore = 1, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 90 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a medium highscore of 3",
            condition = new HighscoreCondition { targetHighscore = 3, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 120 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 3 matches",
            condition = new MatchesCondition { targetMacthes = 3, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 90 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 5",
            condition = new LevelCondition { targetLevel = 5 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 8",
            condition = new HighscoreCondition { targetHighscore = 8, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 130 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 5 matches",
            condition = new MatchesCondition { targetMacthes = 5, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 120 },
                new Reward { type = RewardType.CraftingCredits, amount = 50 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU",
            condition = new HighscoreCondition { targetHighscore = 1, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 150 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 10",
            condition = new HighscoreCondition { targetHighscore = 10, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 10",
            condition = new LevelCondition { targetLevel = 10 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a daily login streak of 2",
            condition = new StreakCondition { targetStreak = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an easy highscore of 7",
            condition = new HighscoreCondition { targetHighscore = 7, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 160 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a medium highscore of 5",
            condition = new HighscoreCondition { targetHighscore = 5, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a hard highscore of 3",
            condition = new HighscoreCondition { targetHighscore = 3, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 140 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 10 matches",
            condition = new MatchesCondition { targetMacthes = 10, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 150 },
                new Reward { type = RewardType.CraftingCredits, amount = 100 },
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 15",
            condition = new LevelCondition { targetLevel = 15 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 5 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a daily login streak of 3",
            condition = new StreakCondition { targetStreak = 3 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 250 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 15",
            condition = new HighscoreCondition { targetHighscore = 15, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 150 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win an online match",
            condition = new MatchesCondition { targetMacthes = 1, matchResult = "Win", difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.PlusPacks, amount = 1 }
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
