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
        GenerateDailyChallenges();
        GenerateOngoingChallenges();
    }

    void GenerateDailyChallenges()
    {
        // clear old challenges
        challengeData.easyDailyChallenges = new List<Challenge>();
        challengeData.hardDailyChallenges = new List<Challenge>();

        // ----- EASY DAILY CHALLENGES -----
        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Claimed",
            condition = new BeatByCondition { winByTargetPoints = 99999999, targetDifficultyLevel = 3 },
            rewards = new List<Reward> { }
        });

        // "BEAT THE EASY CPU BY X POINTS"

        int[] easyBeatByScores = { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 };
        int[] easyBeatByXPRewards = { 40, 80, 120, 160, 240, 320, 400, 560, 720, 880, 1200, 1520, 1840, 2480, 3120, 3760 };
        int[] easyBeatByPackRewards = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };

        for (int i = 0; i < Mathf.Min(easyBeatByScores.Length, easyBeatByXPRewards.Length, easyBeatByPackRewards.Length); i++)
        {
            challengeData.easyDailyChallenges.Add(new Challenge
            {
                challengeText = easyBeatByScores[i] > 1 ? $"Beat the easy CPU by {easyBeatByScores[i]} points" : "Beat the easy CPU",
                condition = new BeatByCondition { winByTargetPoints = easyBeatByScores[i], targetDifficultyLevel = 0 },
                rewards = new List<Reward>
                {
                    new Reward { type = RewardType.XP, amount = easyBeatByXPRewards[i] },
                    new Reward { type = RewardType.StandardPacks, amount = easyBeatByPackRewards[i] }
                }
            });
        }

        // "WIN USING X" // TODO: variable quantity, "win a hard match using 10 common cards"

        // "Win an easy match using {cardIndex}"
        int winUsingX = System.DateTime.Parse(PlayerPrefs.GetString("LastChallengeDate")).Second % PowerupCardData.GetCardCount();
        if (PowerupCardData.GetCardName(winUsingX) == null) { winUsingX = (winUsingX + 1) % PowerupCardData.GetCardCount(); }
        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = $"Win an easy match using '{PowerupCardData.GetCardName(winUsingX)}'",
            condition = new WinUsingCondition { ID = winUsingX + 5, targetDifficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 80 + (PowerupCardData.GetCardRarity(winUsingX) * 60) },
                new Reward { type = RewardType.StandardPacks, amount = 1 + PowerupCardData.GetCardRarity(winUsingX) }
            }
        });

        // "Win an easy match using any {Type} card" (ranks)
        string[] winUsingATexts = { "legendary", "epic", "rare", "uncommon", "common", "holo", "bronze", "gold", "diamond", "celestial" }; // TODO: any, different
        int winUsingA = (System.DateTime.Parse(PlayerPrefs.GetString("LastChallengeDate")).Second % winUsingATexts.Length);

        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = $"Win an easy match using any {winUsingATexts[winUsingA]} card",
            condition = new WinUsingCondition { ID = 4 - winUsingA, targetDifficultyLevel = 0 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 60 + (Mathf.Abs(4 - winUsingA) * 40) },
                new Reward { type = RewardType.StandardPacks, amount = Mathf.Abs(4 - winUsingA) }
            }
        });

        // TODO: "Win a match using Y and Z"

        // "Open a pack"
        challengeData.easyDailyChallenges.Add(new Challenge
        {
            challengeText = "Open a pack",
            condition = new OpenPacksCondition { targetNumberOpened = 1, targetPackType = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 100 }
            }
        });


        // ----- HARD DAILY CHALLENGES -----
        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Claimed",
            condition = new BeatByCondition { winByTargetPoints = 99999999, targetDifficultyLevel = 3 },
            rewards = new List<Reward> { }
        });

        // "BEAT THE HARD CPU BY X POINTS"

        int[] hardBeatByScores = { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 };
        int[] hardBeatByXPRewards = { 80, 160, 240, 320, 480, 640, 800, 1120, 1440, 1760, 2400, 3040, 3680, 4960, 6240, 7520 };
        int[] hardBeatByPackRewards = { 2, 2, 4, 4, 6, 6, 8, 8, 10, 10, 12, 12, 14, 14, 16, 16 };

        for (int i = 0; i < Mathf.Min(hardBeatByScores.Length, hardBeatByXPRewards.Length, hardBeatByPackRewards.Length); i++)
        {
            challengeData.hardDailyChallenges.Add(new Challenge
            {
                challengeText = hardBeatByScores[i] > 1 ? $"Beat the hard CPU by {hardBeatByScores[i]} points" : "Beat the hard CPU",
                condition = new BeatByCondition { winByTargetPoints = hardBeatByScores[i], targetDifficultyLevel = 2 },
                rewards = new List<Reward>
                {
                    new Reward { type = RewardType.XP, amount = hardBeatByXPRewards[i] },
                    new Reward { type = RewardType.StandardPacks, amount = hardBeatByPackRewards[i] }
                }
            });
        }

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = "Win an online match",
            condition = new BeatByCondition { winByTargetPoints = 1, targetDifficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 250 },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
            }
        });

        // "WIN USING X" // TODO: variable quantity, rarity "win a hard match using 10 common cards"

        // "Win a hard match using X" (card indexes)
        winUsingX = (winUsingX + (PowerupCardData.GetCardCount() / 2)) % PowerupCardData.GetCardCount();
        if (PowerupCardData.GetCardName(winUsingX) == null) { winUsingX = (winUsingX + 1) % PowerupCardData.GetCardCount(); }

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = $"Win a hard match using '{PowerupCardData.GetCardName(winUsingX)}'",
            condition = new WinUsingCondition { ID = winUsingX + 5, targetDifficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 160 + (PowerupCardData.GetCardRarity(winUsingX) * 80) },
                new Reward { type = RewardType.StandardPacks, amount = 3 + PowerupCardData.GetCardRarity(winUsingX) }
            }
        });

        // "Win a hard match using any {Type} card" (ranks)
        winUsingA = (winUsingA + (winUsingATexts.Length / 2)) % winUsingATexts.Length;

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = $"Win a hard match using any {winUsingATexts[winUsingA]} card",
            condition = new WinUsingCondition { ID = 4 - winUsingA, targetDifficultyLevel = 2 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 120 + (Mathf.Abs(4 - winUsingA) * 60) },
                new Reward { type = RewardType.StandardPacks, amount = Mathf.Abs(4 - winUsingA) * 2 }
            }
        });

        // "Win an online match using X" (card indexes)
        winUsingX = (winUsingX + (PowerupCardData.GetCardCount() / 3)) % PowerupCardData.GetCardCount();
        if (PowerupCardData.GetCardName(winUsingX) == null) { winUsingX = (winUsingX + 1) % PowerupCardData.GetCardCount(); }

        challengeData.hardDailyChallenges.Add(new Challenge
        {
            challengeText = $"Win an online match using '{PowerupCardData.GetCardName(winUsingX)}'",
            condition = new WinUsingCondition { ID = winUsingX + 5, targetDifficultyLevel = -1 },
            rewards = new List<Reward>
            {
                new Reward { type = RewardType.XP, amount = 250 + (PowerupCardData.GetCardRarity(winUsingX) * 100) },
                new Reward { type = RewardType.PlusPacks, amount = 1 }
            }
        });
    }

    void GenerateOngoingChallenges() {
        // ONGOING CHALLENGES
        // 0 index can't exist because to complete a challenge means it is saved as a negative value
        challengeData.ongoingChallenges.Add(new Challenge
        {
            challengeText = "ERROR",
            condition = new BeatByCondition { winByTargetPoints = 99999999, targetDifficultyLevel = 3 },
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

    }
#endif

    public void ReGenerateDailyChallenges()
    {
        GenerateDailyChallenges();
    }
}
