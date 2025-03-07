using System.Collections.Generic;
using UnityEngine;

public class ChallengeObject
{
    public List<Challenge> easyDailyChallenges = new List<Challenge>();
    public List<Challenge> hardDailyChallenges = new List<Challenge>();
    public List<Challenge> ongoingChallenges = new List<Challenge>();
}

// TODO: add weights for dailt challenges maybe
public class Challenge
{
    public string challengeText; // Challenge description to display to the user
    public ChallengeCondition condition; // The condition to complete the challenge
    public List<Reward> rewards; // List of rewards (XP, drops, packs, credits, etc.)

    public bool CheckCompletion(params object[] args)
    {
        return condition.IsConditionMet(args);
    }

    // TODO: figure out if I want to use Gets for each reward or have other scripts iterate through the rewards list
    public int GetXPReward()
    {
        foreach (var reward in rewards)
        {
            if (reward.type == RewardType.XP)
            {
                return reward.amount;
            }
        }
        return 0;
    }
}

// Abstract Base Class for Conditions
public abstract class ChallengeCondition
{
    public abstract bool IsConditionMet(params object[] args);
    public abstract bool IsAssignable();
}

// Beat By Condition (
public class BeatByCondition : ChallengeCondition
{
    public int winByTargetPoints = 1; // Example: win by 3+ points
    public int difficultyLevel = -2;   // Example: Any Difficulty = -3, Online = -1, Easy = 0, Medium = 1, Hard = 2

    public override bool IsConditionMet(params object[] args)
    {
        int pointsWonBy = (int)args[0];
        int difficulty = (int)args[1];

        return pointsWonBy >= winByTargetPoints && (difficulty == difficultyLevel || difficultyLevel == -3) ;
    }

    public override bool IsAssignable()
    {
        if (
            (difficultyLevel == 0 && PlayerPrefs.GetInt("easyHighscore") < (winByTargetPoints + 2)) ||
            (difficultyLevel == 1 && PlayerPrefs.GetInt("mediumHighscore") < (winByTargetPoints + 2)) ||
            (difficultyLevel == 2 && PlayerPrefs.GetInt("hardHighscore") < (winByTargetPoints + 2))
            )
        {
            return false;
        }

        return true;
    }
}
public class MatchesCondition : ChallengeCondition
{
    public int targetMacthes;
    public string matchType;
    public int difficultyLevel; // Example: Any Difficulty = -3, Combined = -2, Online = -1, Easy = 0, Medium = 1, Hard = 2

    public override bool IsConditionMet(params object[] args)
    {
        if (matchType != "Win" && matchType != "Tie" && matchType != "Loss")
        {
            Debug.LogError("ChallengeObjectError: MatchesCondition of invalid type.");
            return false;
        }

        string[] difficultyStrings = { "easy", "medium", "hard" };

        var currentValue = 0;

        // Easy, Medium, Hard
        if (difficultyLevel == 0 || difficultyLevel == 1 || difficultyLevel == 2)
        {
            currentValue = PlayerPrefs.GetInt(difficultyStrings[difficultyLevel] + matchType);
        }
        // Combined
        else if (difficultyLevel == -1)
        {
            currentValue += PlayerPrefs.GetInt(matchType.ToLower());
        }
        //Any Difficulty
        else if (difficultyLevel == -2)
        {
            int temp = 0;
            foreach (var dstring in difficultyStrings)
            {
                temp = PlayerPrefs.GetInt(dstring + matchType);
                if (temp > currentValue)
                {
                    currentValue = temp;
                }
            }
            temp = PlayerPrefs.GetInt("online" + matchType);
            if (temp > currentValue)
            {
                currentValue = temp;
            }
        }

        return currentValue >= targetMacthes;
    }

    public override bool IsAssignable() { return true; }
}

// Highscore Condition
public class HighscoreCondition : ChallengeCondition
{
    public int targetHighscore;
    public int difficultyLevel; // Example: Any Difficulty = -2, Combined = -1, Easy = 0, Medium = 1, Hard = 2

    public override bool IsConditionMet(params object[] args)
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

        return currentValue >= targetHighscore;
    }

    public override bool IsAssignable() { return true; }
}

// Card Condition (Example: Play a specific card X times)
// TODO: this is just a template for now, eventually i'll make this a proper challange
public class CardCondition : ChallengeCondition
{
    public int cardID;
    public int targetNumberPlayed;

    public override bool IsConditionMet(params object[] args)
    {
        int playedCardID = (int)args[0];
        int timesPlayed = (int)args[1];

        return playedCardID == cardID && timesPlayed >= targetNumberPlayed;
    }

    // TODO: make sure player has the card
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
    Packs,
    Credits // crafting credits, will be added later in developement with packs
}
