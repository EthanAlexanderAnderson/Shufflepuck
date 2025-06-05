using System.Collections.Generic;
using UnityEngine;

public class ChallengeObject
{
    public List<Challenge> easyDailyChallenges = new List<Challenge>();
    public List<Challenge> hardDailyChallenges = new List<Challenge>();
    public List<Challenge> ongoingChallenges = new List<Challenge>();

    public int GetNumberOfEasyDailyChallenges() { return easyDailyChallenges.Count; }
    public int GetNumberOfHardDailyChallenges() { return hardDailyChallenges.Count; }
    public int GetNumberOfOngoingChallenges() { return ongoingChallenges.Count; }
}

// TODO: add weights for dailt challenges maybe
public class Challenge
{
    public string challengeText; // Challenge description to display to the user
    public ChallengeCondition condition; // The condition to complete the challenge
    public List<Reward> rewards; // List of rewards (XP, drops, packs, credits, etc.)

    public (float, float) GetVariables(params object[] args)
    {
        return condition.GetConditionVariables();
    }

    public float GetProgress(params object[] args)
    {
        return condition.GetConditionProgress();
    }

    public bool CheckCompletion(params object[] args)
    {
        return condition.IsConditionMet(args);
    }

    public void ClaimRewards()
    {
        foreach (var reward in rewards)
        {
            if (reward.amount <= 0) { continue; }
            // TODO: puck reward
            if (reward.type == RewardType.XP)
            {
                LevelManager.Instance.AddXP(reward.amount);
            }
            else if (reward.type == RewardType.Drops)
            {
                PlayerPrefs.SetInt("PlinkoPegsDropped", PlayerPrefs.GetInt("PlinkoPegsDropped") - reward.amount);
            }
            else if (reward.type == RewardType.StandardPacks)
            {
                PackManager.Instance.RewardPacks(false, reward.amount);
            }
            else if (reward.type == RewardType.PlusPacks)
            {
                PackManager.Instance.RewardPacks(true, reward.amount);
            }
            else if (reward.type == RewardType.CraftingCredits)
            {
                PackManager.Instance.RewardCraftingCredits(reward.amount);
            }
        }
    }

    public List<string> GetRewardStrings()
    {
        List<string> rewardStrings = new();

        foreach (var reward in rewards)
        {
            if (reward.amount <= 0) { continue; }
            // TODO puck icon
            if (reward.type == RewardType.XP)
            {
                rewardStrings.Add(reward.amount.ToString() + " XP");
            }
            else if (reward.type == RewardType.Drops)
            {
                rewardStrings.Add(reward.amount.ToString() + (reward.amount > 1 ? " drops" : " drop"));
            }
            else if (reward.type == RewardType.StandardPacks)
            {
                rewardStrings.Add(reward.amount.ToString() + (reward.amount > 1 ? " packs" : " pack"));
            }
            else if (reward.type == RewardType.PlusPacks)
            {
                rewardStrings.Add(reward.amount.ToString() + (reward.amount > 1 ? " +packs" : " +pack"));
            }
            else if (reward.type == RewardType.CraftingCredits)
            {
                rewardStrings.Add(reward.amount.ToString() + " credits"); // TODO: make the icon instead of text
            }
        }
        return rewardStrings;
    }
}

// Abstract Base Class for Conditions
public abstract class ChallengeCondition
{
    public abstract (float, float) GetConditionVariables();
    public virtual float GetConditionProgress()
    {
        var (num, dem) = GetConditionVariables();
        return num / dem;
    }
    public virtual bool IsConditionMet(params object[] args)
    {
        return GetConditionProgress() >= 1;
    }
    public abstract bool IsAssignable();
}

// Beat By Condition, examples: "Beat the hard CPU", "Win by 3 points", "Win an online match", "Beat the medium CPU by 5 points"
public class BeatByCondition : ChallengeCondition
{
    public int winByTargetPoints = 1; // Example: win by 3+ points
    public int difficultyLevel = -2; // Example: Any Difficulty = -3, Online = -1, Easy = 0, Medium = 1, Hard = 2

    // TODO: investigate if there is a better way to do this
    public override (float, float) GetConditionVariables()
    {
        return (0, 0);
    }

    public override bool IsConditionMet(params object[] args)
    {
        int pointsWonBy = (int)args[0];
        int difficulty = (int)args[1];

        return pointsWonBy >= winByTargetPoints && (difficulty == difficultyLevel || difficultyLevel == -3) ;
    }

    public override bool IsAssignable()
    {
        string[] diffKeys = { "easyHighscore", "mediumHighscore", "hardHighscore" };
        // don't assign a daily challenge that is too hard or too easy
        if (difficultyLevel >= 0 && (PlayerPrefs.GetInt(diffKeys[difficultyLevel]) < (winByTargetPoints + 2) || PlayerPrefs.GetInt(diffKeys[difficultyLevel]) > (winByTargetPoints + 8)))
        {
            return false;
        }
        // don't assign "win online" challenge if the player hasn't beat the hard CPU 5 times
        else if (difficultyLevel == -1 && PlayerPrefs.GetInt("hardWin") < 5)
        {
            return false;
        }

        return true;
    }
}

// Matches Condition, examples: "Win 3 matches", "Win an online match"
public class MatchesCondition : ChallengeCondition
{
    public int targetMacthes;
    public string matchResult;
    public int difficultyLevel; // Example: Any Difficulty = -3, Combined = -2, Online = -1, Easy = 0, Medium = 1, Hard = 2

    public override (float, float) GetConditionVariables()
    {
        if (matchResult != "Win" && matchResult != "Tie" && matchResult != "Loss")
        {
            Debug.LogError("ChallengeObjectError: MatchesCondition of invalid type.");
            return (0, 0);
        }

        string[] difficultyStrings = { "easy", "medium", "hard" };

        var currentValue = 0;

        // Easy, Medium, Hard
        if (difficultyLevel == 0 || difficultyLevel == 1 || difficultyLevel == 2)
        {
            currentValue = PlayerPrefs.GetInt(difficultyStrings[difficultyLevel] + matchResult);
        }
        // Online
        if (difficultyLevel == -1)
        {
            currentValue = PlayerPrefs.GetInt("online" + matchResult);
        }
        // Combined
        else if (difficultyLevel == -2)
        {
            currentValue += PlayerPrefs.GetInt(matchResult.ToLower());
        }
        // Any Difficulty
        else if (difficultyLevel == -3)
        {
            int temp = 0;
            foreach (var dstring in difficultyStrings)
            {
                temp = PlayerPrefs.GetInt(dstring + matchResult);
                if (temp > currentValue)
                {
                    currentValue = temp;
                }
            }
            temp = PlayerPrefs.GetInt("online" + matchResult);
            if (temp > currentValue)
            {
                currentValue = temp;
            }
        }

        return (currentValue, targetMacthes);
    }

    public override bool IsAssignable() { return true; }
}

// Highscore Condition
public class HighscoreCondition : ChallengeCondition
{
    public int targetHighscore;
    public int difficultyLevel; // Example: Any Difficulty = -2, Combined = -1, Easy = 0, Medium = 1, Hard = 2

    public override (float, float) GetConditionVariables()
    {
        int easyHighscore = PlayerPrefs.GetInt("easyHighscore");
        int mediumHighscore = PlayerPrefs.GetInt("mediumHighscore");
        int hardHighscore = PlayerPrefs.GetInt("hardHighscore");

        int[] highscores = { easyHighscore, mediumHighscore, hardHighscore };

        var currentValue = 0;

        // Easy, Medium, Hard
        if (difficultyLevel == 0 || difficultyLevel == 1 || difficultyLevel == 2)
        {
            currentValue = highscores[difficultyLevel];
        }
        // Combined
        else if (difficultyLevel == -1)
        {
            currentValue = easyHighscore + mediumHighscore + hardHighscore;
        }
        //Any Difficulty
        else if (difficultyLevel == -2)
        {
            foreach (var highscore in highscores)
            {
                if (highscore > currentValue)
                {
                    currentValue = highscore;
                }
            }
        }

        return (currentValue, targetHighscore);
    }

    public override bool IsAssignable() { return true; }
}

// Card Condition, example: "Win a match using 'plus one'", "Win a match using any holo card", "Win matches using 'growth' 3 times", "Win a match using any common card"
public class WinUsingCondition : ChallengeCondition
{
    public int ID; // -5 = celestial, -4 = diamond, -3 = gold, -2 = bronze, -1 = holo, 0 - 4 rarity, 5+ index - 5
    public int targetNumberUsed;

    public override (float, float) GetConditionVariables()
    {
        int currentNumberUsed = 0;

        // Get the current WonUsing string
        string wonUsingString = PlayerPrefs.GetString("WonUsing", "");

        // Convert the wonUsingString string into a list of encoded card strings
        List<string> wonUsingCardsEncoded = string.IsNullOrEmpty(wonUsingString)
            ? new List<string>()
            : new List<string>(wonUsingString.Split(','));


        // Search for the card in the list
        for (int i = 0; i < wonUsingCardsEncoded.Count; i++)
        {
            if (int.TryParse(wonUsingCardsEncoded[i], out int encodedCard))
            {
                var decodedCard = PowerupCardData.DecodeCard(encodedCard);

                // card index
                if (ID >= 5 && decodedCard.cardIndex == (ID - 5))
                {
                    currentNumberUsed += decodedCard.quantity;
                }
                // card rarity
                else if (ID >= 0 && PowerupCardData.GetCardRarity(decodedCard.cardIndex) == ID)
                {
                    currentNumberUsed += decodedCard.quantity;
                }
                // holo
                else if (ID == -1 && decodedCard.holo)
                {
                    currentNumberUsed += decodedCard.quantity;
                }
                // rank
                else if (ID >= -5 && decodedCard.rank == (ID * -1) - 1)
                {
                    currentNumberUsed += decodedCard.quantity;
                }
            }
        }

        return (currentNumberUsed, targetNumberUsed);
    }

    // TODO: make sure player has the card
    public override bool IsAssignable() { return true; }
}

public class LevelCondition : ChallengeCondition
{
    public int targetLevel;

    public override (float, float) GetConditionVariables()
    {
        var (_, level) = LevelManager.Instance.GetXPAndLevel();

        return (level, targetLevel);
    }

    public override bool IsAssignable() { return true; }
}

public class StreakCondition : ChallengeCondition
{
    public int targetStreak;

    public override (float, float) GetConditionVariables()
    {
        int streak = PlayerPrefs.GetInt("Streak");

        return (streak, targetStreak);
    }

    public override bool IsAssignable() { return true; }
}

public class Reward
{
    public RewardType type;
    public int amount;
    public int itemID; // For packs, skins, etc.
}

public enum RewardType
{
    XP,
    Drops,
    Skin,
    StandardPacks,
    PlusPacks,
    CraftingCredits
}
