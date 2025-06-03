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
                new Reward { type = RewardType.XP, amount = 120 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 9 points",
            condition = new BeatByCondition { winByTargetPoints = 9, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 160 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 11 points",
            condition = new BeatByCondition { winByTargetPoints = 11, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 13 points",
            condition = new BeatByCondition { winByTargetPoints = 13, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 320 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 15 points",
            condition = new BeatByCondition { winByTargetPoints = 15, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 480 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 17 points",
            condition = new BeatByCondition { winByTargetPoints = 17, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 640 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 19 points",
            condition = new BeatByCondition { winByTargetPoints = 19, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 960 },
                new Reward { type = RewardType.StandardPacks, amount = 4 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 21 points",
            condition = new BeatByCondition { winByTargetPoints = 21, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 1280 },
                new Reward { type = RewardType.StandardPacks, amount = 4 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 23 points",
            condition = new BeatByCondition { winByTargetPoints = 23, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 1920 },
                new Reward { type = RewardType.StandardPacks, amount = 5 }
            }
        });

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the easy CPU by 25 points",
            condition = new BeatByCondition { winByTargetPoints = 25, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 2560 },
                new Reward { type = RewardType.StandardPacks, amount = 5 }
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
                new Reward { type = RewardType.StandardPacks, amount = 3 }
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
                new Reward { type = RewardType.StandardPacks, amount = 4 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 17 points",
            condition = new BeatByCondition { winByTargetPoints = 17, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 1280 },
                new Reward { type = RewardType.StandardPacks, amount = 4 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 19 points",
            condition = new BeatByCondition { winByTargetPoints = 19, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 1920 },
                new Reward { type = RewardType.StandardPacks, amount = 5 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU by 21 points",
            condition = new BeatByCondition { winByTargetPoints = 21, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 2560 },
                new Reward { type = RewardType.StandardPacks, amount = 5 }
            }
        });

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Win an online game",
            condition = new BeatByCondition { winByTargetPoints = 1, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 250 },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
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
            condition = new MatchesCondition { targetMacthes = 1, matchResult = "Win", difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 40 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an easy highscore of 3",
            condition = new HighscoreCondition { targetHighscore = 3, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 60 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Beat the medium CPU",
            condition = new MatchesCondition { targetMacthes = 1, matchResult = "Win", difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 60 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 5",
            condition = new HighscoreCondition { targetHighscore = 5, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 50 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 3 matches",
            condition = new MatchesCondition { targetMacthes = 3, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 30 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a medium highscore of 3",
            condition = new HighscoreCondition { targetHighscore = 3, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 90 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 7",
            condition = new HighscoreCondition { targetHighscore = 7, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 70 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
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
            challengeText = "Win 5 matches",
            condition = new MatchesCondition { targetMacthes = 5, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 50 }
            }
        });

        // 10 -
        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Beat the hard CPU",
            condition = new MatchesCondition { targetMacthes = 1, matchResult = "Win", difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 80 },
                new Reward { type = RewardType.CraftingCredits, amount = 15 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 3 matches on easy difficulty",
            condition = new MatchesCondition { targetMacthes = 3, matchResult = "Win", difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 40 },
                new Reward { type = RewardType.CraftingCredits, amount = 15 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 9",
            condition = new HighscoreCondition { targetHighscore = 9, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 70 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a daily login streak of 2",
            condition = new StreakCondition { targetStreak = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 150 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 10",
            condition = new LevelCondition { targetLevel = 10 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 4 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an easy highscore of 7",
            condition = new HighscoreCondition { targetHighscore = 7, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 70 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 3 matches on medium difficulty",
            condition = new MatchesCondition { targetMacthes = 3, matchResult = "Win", difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 50 },
                new Reward { type = RewardType.CraftingCredits, amount = 30 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a medium highscore of 5",
            condition = new HighscoreCondition { targetHighscore = 5, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 90 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using any common card",
            condition = new WinUsingCondition { ID = 0, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 40 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a hard highscore of 3",
            condition = new HighscoreCondition { targetHighscore = 3, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 80 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        // 20
        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 10 matches",
            condition = new MatchesCondition { targetMacthes = 10, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 50 },
                new Reward { type = RewardType.CraftingCredits, amount = 50 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win matches using common cards 5 times",
            condition = new WinUsingCondition { ID = 0, targetNumberUsed = 5 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 50 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 17",
            condition = new HighscoreCondition { targetHighscore = 17, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 85 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a daily login streak of 3",
            condition = new StreakCondition { targetStreak = 3 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 150 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 15",
            condition = new LevelCondition { targetLevel = 15 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 6 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 3 matches on hard difficulty",
            condition = new MatchesCondition { targetMacthes = 3, matchResult = "Win", difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 90 },
                new Reward { type = RewardType.CraftingCredits, amount = 45 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using any uncommon card",
            condition = new WinUsingCondition { ID = 1, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 90 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win an online match",
            condition = new MatchesCondition { targetMacthes = 1, matchResult = "Win", difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 400 },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win matches using uncommon cards 5 times",
            condition = new WinUsingCondition { ID = 1, targetNumberUsed = 5 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 110 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a daily login streak of 4",
            condition = new StreakCondition { targetStreak = 4 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 20",
            condition = new LevelCondition { targetLevel = 20 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 8 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 15 matches",
            condition = new MatchesCondition { targetMacthes = 15, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 150 },
                new Reward { type = RewardType.StandardPacks, amount = 2 },
                new Reward { type = RewardType.CraftingCredits, amount = 75 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a hard highscore of 5",
            condition = new HighscoreCondition { targetHighscore = 5, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 180 },
                new Reward { type = RewardType.StandardPacks, amount = 5 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 5 matches on easy difficulty",
            condition = new MatchesCondition { targetMacthes = 5, matchResult = "Win", difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 130 },
                new Reward { type = RewardType.StandardPacks, amount = 1 },
                new Reward { type = RewardType.CraftingCredits, amount = 25 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 3 online matches",
            condition = new MatchesCondition { targetMacthes = 3, matchResult = "Win", difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 300 },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 21",
            condition = new HighscoreCondition { targetHighscore = 21, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 220 },
                new Reward { type = RewardType.StandardPacks, amount = 4 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using any rare card",
            condition = new WinUsingCondition { ID = 2, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 140 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 25",
            condition = new LevelCondition { targetLevel = 25 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 10 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'plus one'",
            condition = new WinUsingCondition { ID = 0 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a medium highscore of 7",
            condition = new HighscoreCondition { targetHighscore = 7, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 260 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 20 matches",
            condition = new MatchesCondition { targetMacthes = 20, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 },
                new Reward { type = RewardType.StandardPacks, amount = 2 },
                new Reward { type = RewardType.CraftingCredits, amount = 100 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'foresight'",
            condition = new WinUsingCondition { ID = 1 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an easy highscore of 9",
            condition = new HighscoreCondition { targetHighscore = 9, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'block'",
            condition = new WinUsingCondition { ID = 2 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 30",
            condition = new LevelCondition { targetLevel = 30 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 12 },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'bolt'",
            condition = new WinUsingCondition { ID = 3 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 23",
            condition = new HighscoreCondition { targetHighscore = 23, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 350 },
                new Reward { type = RewardType.StandardPacks, amount = 5 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'phase'",
            condition = new WinUsingCondition { ID = 5 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 5 matches on medium difficulty",
            condition = new MatchesCondition { targetMacthes = 5, matchResult = "Win", difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 },
                new Reward { type = RewardType.StandardPacks, amount = 2 },
                new Reward { type = RewardType.CraftingCredits, amount = 50 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'lock'",
            condition = new WinUsingCondition { ID = 8 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 35",
            condition = new LevelCondition { targetLevel = 35 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 14 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'fog'",
            condition = new WinUsingCondition { ID = 10 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'shield'",
            condition = new WinUsingCondition { ID = 13 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 30 matches",
            condition = new MatchesCondition { targetMacthes = 30, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 300 },
                new Reward { type = RewardType.StandardPacks, amount = 3 },
                new Reward { type = RewardType.CraftingCredits, amount = 150 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'shuffle'",
            condition = new WinUsingCondition { ID = 14 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 5 online matches",
            condition = new MatchesCondition { targetMacthes = 5, matchResult = "Win", difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 500 },
                new Reward { type = RewardType.PlusPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 40",
            condition = new LevelCondition { targetLevel = 40 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 16 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'push'",
            condition = new WinUsingCondition { ID = 25 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 5 matches on hard difficulty",
            condition = new MatchesCondition { targetMacthes = 5, matchResult = "Win", difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 360 },
                new Reward { type = RewardType.StandardPacks, amount = 3 },
                new Reward { type = RewardType.CraftingCredits, amount = 75 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'erratic'",
            condition = new WinUsingCondition { ID = 26 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'deny'",
            condition = new WinUsingCondition { ID = 27 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using any bronze card",
            condition = new WinUsingCondition { ID = -2, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 500 },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 45",
            condition = new LevelCondition { targetLevel = 45 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 18 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'force field'",
            condition = new WinUsingCondition { ID = 4 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });


        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 25",
            condition = new HighscoreCondition { targetHighscore = 25, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 450 },
                new Reward { type = RewardType.StandardPacks, amount = 5 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'cull'",
            condition = new WinUsingCondition { ID = 6 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 40 matches",
            condition = new MatchesCondition { targetMacthes = 40, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 400 },
                new Reward { type = RewardType.StandardPacks, amount = 4 },
                new Reward { type = RewardType.CraftingCredits, amount = 200 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'growth'",
            condition = new WinUsingCondition { ID = 7 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 50",
            condition = new LevelCondition { targetLevel = 50 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 25 },
                new Reward { type = RewardType.PlusPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'explosion'",
            condition = new WinUsingCondition { ID = 9 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win matches using rare cards 5 times",
            condition = new WinUsingCondition { ID = 2, targetNumberUsed = 5 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 170 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'hydra'",
            condition = new WinUsingCondition { ID = 11 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an easy highscore of 12",
            condition = new HighscoreCondition { targetHighscore = 12, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 },
                new Reward { type = RewardType.StandardPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'factory'",
            condition = new WinUsingCondition { ID = 12 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 50 matches",
            condition = new MatchesCondition { targetMacthes = 50, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 500 },
                new Reward { type = RewardType.StandardPacks, amount = 5 },
                new Reward { type = RewardType.CraftingCredits, amount = 250 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'mill'",
            condition = new WinUsingCondition { ID = 18 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an medium highscore of 9",
            condition = new HighscoreCondition { targetHighscore = 9, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'aura'",
            condition = new WinUsingCondition { ID = 24 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using any epic card",
            condition = new WinUsingCondition { ID = 3, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 250 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'investment'",
            condition = new WinUsingCondition { ID = 28 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 200 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 10 online matches",
            condition = new MatchesCondition { targetMacthes = 10, matchResult = "Win", difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 1000 },
                new Reward { type = RewardType.PlusPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using any gold card",
            condition = new WinUsingCondition { ID = -3, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 500 },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'chaos'",
            condition = new WinUsingCondition { ID = 15 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 300 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an hard highscore of 7",
            condition = new HighscoreCondition { targetHighscore = 7, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 240 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'times two'",
            condition = new WinUsingCondition { ID = 16 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 300 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win matches using epic cards 5 times",
            condition = new WinUsingCondition { ID = 3, targetNumberUsed = 5 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 550 },
                new Reward { type = RewardType.StandardPacks, amount = 4 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'resurrect'",
            condition = new WinUsingCondition { ID = 16 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 300 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win matches using common cards 25 times",
            condition = new WinUsingCondition { ID = 0, targetNumberUsed = 25 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 500 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'research'",
            condition = new WinUsingCondition { ID = 19 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 300 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 10 matches on easy difficulty",
            condition = new MatchesCondition { targetMacthes = 10, matchResult = "Win", difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 250 },
                new Reward { type = RewardType.StandardPacks, amount = 2 },
                new Reward { type = RewardType.CraftingCredits, amount = 50 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'insanity'",
            condition = new WinUsingCondition { ID = 20 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 300 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 31",
            condition = new HighscoreCondition { targetHighscore = 31, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 650 },
                new Reward { type = RewardType.StandardPacks, amount = 5 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using any diamond card",
            condition = new WinUsingCondition { ID = -4, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 500 },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'triple'",
            condition = new WinUsingCondition { ID = 21 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 400 },
                new Reward { type = RewardType.StandardPacks, amount = 4 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an easy highscore of 15",
            condition = new HighscoreCondition { targetHighscore = 15, difficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 400 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'exponent'",
            condition = new WinUsingCondition { ID = 22 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 400 },
                new Reward { type = RewardType.StandardPacks, amount = 4 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 10 matches on hard difficulty",
            condition = new MatchesCondition { targetMacthes = 10, matchResult = "Win", difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 450 },
                new Reward { type = RewardType.StandardPacks, amount = 2 },
                new Reward { type = RewardType.CraftingCredits, amount = 150 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'laser'",
            condition = new WinUsingCondition { ID = 23 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 400 },
                new Reward { type = RewardType.StandardPacks, amount = 4 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an medium highscore of 12",
            condition = new HighscoreCondition { targetHighscore = 12, difficultyLevel = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 400 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win matches using uncommon cards 25 times",
            condition = new WinUsingCondition { ID = 1, targetNumberUsed = 25 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 750 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using any holo card",
            condition = new WinUsingCondition { ID = -1, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 500 },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach an hard highscore of 9",
            condition = new HighscoreCondition { targetHighscore = 9, difficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 750 },
                new Reward { type = RewardType.StandardPacks, amount = 2 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win a match using 'omniscience'",
            condition = new WinUsingCondition { ID = 29 + 5, targetNumberUsed = 1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 500 },
                new Reward { type = RewardType.StandardPacks, amount = 5 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win matches using rare cards 25 times",
            condition = new WinUsingCondition { ID = 2, targetNumberUsed = 25 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 850 },
                new Reward { type = RewardType.StandardPacks, amount = 3 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a combined highscore of 36",
            condition = new HighscoreCondition { targetHighscore = 36, difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 650 },
                new Reward { type = RewardType.StandardPacks, amount = 5 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach a daily login streak of 7",
            condition = new StreakCondition { targetStreak = 7 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 500 },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 100 matches",
            condition = new MatchesCondition { targetMacthes = 100, matchResult = "Win", difficultyLevel = -2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 1000 },
                new Reward { type = RewardType.StandardPacks, amount = 10 },
                new Reward { type = RewardType.CraftingCredits, amount = 1000 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Win 50 online matches",
            condition = new MatchesCondition { targetMacthes = 50, matchResult = "Win", difficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 5000 },
                new Reward { type = RewardType.PlusPacks, amount = 10 }
            }
        });

        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "Reach level 100",
            condition = new LevelCondition { targetLevel = 100 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.StandardPacks, amount = 50 },
                new Reward { type = RewardType.PlusPacks, amount = 10 }
            }
        });

        // last challenge is uncompletable
        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "No ongoing quests remaining",
            condition = new MatchesCondition { targetMacthes = int.MaxValue, matchResult = "Win", difficultyLevel = -1 },
            rewards = new List<Reward> { }
        });

        // load challenges
        DailyChallengeManagerScript.Instance.SetText();
        OngoingChallengeManagerScript.Instance.EvaluateChallengeAndSetText();

#if UNITY_EDITOR
        int totalXPRewardForAllOngoingChallenges = 0;
        int totalStandardPackRewardForAllOngoingChallenges = 0;
        int totalPlusPackRewardForAllOngoingChallenges = 0;
        int totalCraftingCreditRewardForAllOngoingChallenges = 0;
        for (int i = 0; i < challengeData.ongoingChallenges.Count; i++)
        {
            foreach (var reward in challengeData.ongoingChallenges[i].rewards)
            {
                if (reward.type == RewardType.XP)
                {
                    totalXPRewardForAllOngoingChallenges += reward.amount;
                }
                else if (reward.type == RewardType.StandardPacks)
                {
                    totalStandardPackRewardForAllOngoingChallenges += reward.amount;
                }
                else if (reward.type == RewardType.PlusPacks)
                {
                    totalPlusPackRewardForAllOngoingChallenges += reward.amount;
                }
                else if (reward.type == RewardType.CraftingCredits)
                {
                    totalCraftingCreditRewardForAllOngoingChallenges += reward.amount;
                }
            }
        }
        Debug.Log("totalXPRewardForAllOngoingChallenges: " + totalXPRewardForAllOngoingChallenges);
        Debug.Log("totalStandardPackRewardForAllOngoingChallenges: " + totalStandardPackRewardForAllOngoingChallenges);
        Debug.Log("totalPlusPackRewardForAllOngoingChallenges: " + totalPlusPackRewardForAllOngoingChallenges);
        Debug.Log("totalCraftingCreditRewardForAllOngoingChallenges: " + totalCraftingCreditRewardForAllOngoingChallenges);
#endif
    }
}
