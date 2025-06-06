using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public static class CPUBehaviorScript
{
    private static int difficulty;
    private static int modifiedDifficulty;

    public static GameObject[] CPUPaths;

    static List<int> powerupsUsedThisTurn = new();
    static public int PowerupCountUsedThisTurn() { return powerupsUsedThisTurn.Count; }
    static private List<int> deck;
    static private int[] hand = { -1, -1, -1 };
    static private int playerDeckPowerLevel;

    static private int denyPowerup;

    public static void TurnReset()
    {
        powerupsUsedThisTurn = new();
        DrawHand();
        waitingTime = 1f;
        LogicScript.Instance.powerupWaitTime = 0;
        usePhase = false;
    }

    public static void FullReset(int diff)
    {
        difficulty = diff;
        GenerateDeck();

        hand = new int[modifiedDifficulty];
        powerupsUsedThisTurn = new();

        for (int i = 0; i < modifiedDifficulty; i++)
        {
            hand[i] = -1;
        }
    }

    private static void GenerateDeck()
    {
        deck = new();

        // get power level of the players deck
        List<int> playerDeck = PowerupManager.Instance.GetDeck();
        playerDeckPowerLevel = 0;
        for (int i = 0; i < playerDeck.Count; i++)
        {
            playerDeckPowerLevel += (int)Math.Pow(PowerupCardData.GetCardRarity(PowerupCardData.DecodeCard(playerDeck[i]).cardIndex) + 1, 2);
        }

        // any cards in deck -> increases modified difficulty by 1
        // every additional 15 deck power -> increases modified difficulty by 1
        modifiedDifficulty = difficulty + (playerDeckPowerLevel + 14) / 15;

        // modify the deck power level based on CPU difficulty, for the purpose of CPU deckbuilding
        playerDeckPowerLevel -= (2 - difficulty) * 5;

        Debug.Log("modified playerDeckPowerLevel: " + playerDeckPowerLevel);

        int CPUDeckPowerLevel = 0;
        int failSafe = 0;
        while (CPUDeckPowerLevel < playerDeckPowerLevel && failSafe < 1000)
        {
            int rarity = Random.Range(0, (int)Math.Pow(difficulty, 2) + 1);
            int randomCard = PowerupCardData.GetRandomCardOfRarity(rarity);
            if (deck.Count(n => n == randomCard) < (5 - rarity) && EvaluatePowerupEquipage(randomCard) && PowerupCardData.CheckIfCardIsOwned(randomCard))
            {
                deck.Add(randomCard);
                CPUDeckPowerLevel += (rarity + 1);
            }
            failSafe++;
        }

        // temporary force add bolt/cull if the player has exponent, because exponent erratic is too strong
        // TODO remove this or change it or something
        if (PowerupManager.Instance.DeckContains(22))
        {
            while (deck.Count(n => n == 3) < 5)
            {
                deck.Add(3);
            }

            if (difficulty > 0)
            {
                while (deck.Count(n => n == 6) < 4)
                {
                    deck.Add(6);
                }
            }
        }

        Debug.Log("CPU modifiedDifficulty set to: " + modifiedDifficulty);
    }

    private static void DrawHand()
    {
        int deckCount = deck.Count;

        // initialize array with proper length
        int[] previouslyGeneratedIndexes = new int[modifiedDifficulty];
        for (int i = 0; i < modifiedDifficulty; i++)
        {
            previouslyGeneratedIndexes[i] = -1;
        }

        // deny powerup
        for (int i = 0; i < denyPowerup; i++)
        {
            powerupsUsedThisTurn.Add(-1);
            powerupsUsedThisTurn.Add(-1);
        }

        for (int i = 0; i < hand.Length; i++)
        {
            if (deckCount <= 0) { continue; }
            deckCount--;

            denyPowerup = 0;

            int randomIndex = Random.Range(0, deck.Count);
            // while empty in deck, reroll
            while (Array.Exists(previouslyGeneratedIndexes, element => element == randomIndex))
            {
                randomIndex = Random.Range(0, deck.Count);
            }

            previouslyGeneratedIndexes[i] = randomIndex;
            hand[i] = deck[randomIndex];
            Debug.Log("CPU HAND CARD " + (i + 1) + " : " + PowerupManager.Instance.powerupTexts[hand[i]]);
        }
    }

    static float waitingTime = 1f;
    public static (float, float, float) Think(float timeElapsed)
    {
        // use cards
        if (timeElapsed > waitingTime)
        {
            if (hand.Any(n => n != -1) && PowerupCountUsedThisTurn() < 3)
            {
                if (UseNextCard())
                {
                    waitingTime += 1.75f;
                    LogicScript.Instance.powerupWaitTime += 1.25f;
                }
            }
        }
        // pick angle power spin
        if (timeElapsed > waitingTime)
        {
            waitingTime += 999999f;
            denyPowerup = 0;
            return FindPath();
        }

        return (-1f, -1f, -1f);
    }

    private static bool UseNextCard(int cardIndex = -1)
    {
        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i] == -1 || (cardIndex >= 0 && hand[i] != cardIndex)) continue; // if card in hand is null, or is not the desired card, skip

            if (EvaluatePowerupUsage(hand[i]))
            {
                PowerupManager.Instance.CallMethodArray(PowerupCardData.EncodeCard(hand[i], 0, false, 1));
                powerupsUsedThisTurn.Add(hand[i]);
                deck.Remove(hand[i]);

                // cost 2 discard
                if (PowerupManager.Instance.GetCost2Discard().Contains(hand[i]))
                {
                    powerupsUsedThisTurn.Add(hand[i]);
                    powerupsUsedThisTurn.Add(hand[i]);
                    for (int j = 0; j < Math.Max(hand.Count(), 3); j++)
                    {
                        if (j != i)
                        {
                            deck.Remove(hand[j]);
                        }
                    }
                }

                // wait longer to use the next card if we just used forcefield or shuffle, to allow for pucks to move
                if (hand[i] == 4 || hand[i] == 14)
                {
                    waitingTime += 1.0f;
                    LogicScript.Instance.powerupWaitTime += 0.5f;
                }

                hand[i] = -1;
                return true;
            }
        }
        return false;
    }

    // TODO: save selected path info somewhere so we can query it to see if we need to use explosion or phase in the second pass of card usage
    static CPUPathInterface pathi;
    static bool usePhase = false;
    public static (float, float, float) FindPath()
    {
        // if Fog is active and foresight isn't, shoot random
        if ((FogScript.Instance.FogEnabled() && !HaloScript.Instance.HaloEnabled())) { return (Random.Range(20.0f, 60.0f), Random.Range(60.0f, 80.0f), Random.Range(45.0f, 55.0f)); }

        // initialize CPU paths
        CPUPaths = GameObject.FindGameObjectsWithTag("cpu_path");

        // random path is worth 2
        CPUPathInterface best = null;
        float highestValue = 2;

        // check all paths to see which are unblocked
        foreach (var path in CPUPaths)
        {
            pathi = path.GetComponent<CPUPathScript>();
            if (pathi == null) { pathi = path.GetComponent<SmartScanCPUPathScript>(); }

            pathi.DisablePathVisualization();

            var pathiShotValue = pathi.CalculateValue(modifiedDifficulty);

            if (pathi.GetPath().spin != 50 && difficulty < 2) { continue; } // skip path if it needs spin and we're not playing against hard CPU

            // pick the highest path. if the paths have the same value, there is a 25% chance of swapping.
            float randomValue = Random.Range(0f, 1f);
            if (pathiShotValue > highestValue || (pathiShotValue == highestValue && randomValue < 0.25))
            {
                best = pathi;
                highestValue = pathiShotValue;
            }
        }
        // chose the highest-value unblocked path
        if (highestValue > 2 && best != null)
        {
            Debug.Log("Found path with highest value " + highestValue);
            best.EnablePathVisualization();

            // try to phase lol
            usePhase = best.DoesPathRequirePhasePowerup();
            if (best.DoesPathRequirePhasePowerup() && hand.Any(n => n != -1) && PowerupCountUsedThisTurn() < 3)
            {
                UseNextCard(5);
            }

            return best.GetPath();
        }
        // otherwise, Shoot random
        else
        {
            Debug.Log("No path :(");
            float variance = Random.Range(0f, (5 - modifiedDifficulty) * 3);
            return (Random.Range(20.0f - variance, 60.0f + variance), Random.Range(65.0f - variance, 85.0f + variance - (LogicScript.Instance.opponent.puckCount * 3)), Random.Range(45.0f - variance, 55.0f + variance));
        }
    }

    public static void CPUPreShotPowerups()
    {

    }

    public static void CPUPostShotPowerups()
    {

    }

    static int CPUCallPowerupHelper(string name)
    {
        int index = Array.IndexOf(PowerupManager.Instance.powerupTexts, name);
        PowerupManager.Instance.CallMethodArray(PowerupCardData.EncodeCard(index, 0, false, 1));
        return index;
    }

    public static void MillPowerupHelper()
    {
        if (deck == null || deck.Count == 0) return;

        int cardsToRemove = Mathf.CeilToInt(deck.Count / 2f);

        for (int i = 0; i < cardsToRemove && deck.Count > 0; i++)
        {
            int indexToRemove = Random.Range(0, deck.Count);
            deck.RemoveAt(indexToRemove);
        }

        Debug.Log($"MillPowerupHelper: Removed {cardsToRemove} cards. {deck.Count} remain.");
    }


    public static void DenyPowerupHelper()
    {
        denyPowerup++;
    }

    public static void InvestmentPowerupHelper()
    {
        deck.Add(30);
    }

    // if the CPU should add a card to its deck
    public static bool EvaluatePowerupEquipage(int cardIndex)
    {
        return cardIndex switch
        {
            0 => true,
            1 => PowerupManager.Instance.DeckContains(10),
            2 => true,
            3 => true,
            4 => true,
            5 => true,
            6 => true,
            7 => true,
            8 => true,
            9 => false, // TODO
            10 => !PowerupManager.Instance.DeckContains(1),
            11 => true,
            12 => true,
            13 => deck.Contains(0) || deck.Contains(7) || deck.Contains(9) || deck.Contains(12) || deck.Contains(16) || deck.Contains(22) || deck.Contains(24),
            14 => true,
            15 => false, // TODO
            16 => deck.Count() > 2,
            17 => deck.Count() > 2 && deck.Contains(3) && deck.Contains(6),
            18 => true,
            19 => false, // TODO
            20 => false, // TODO: equip insanity only if cpu deck contains investment
            21 => false, // TODO
            22 => deck.Contains(13) && !deck.Contains(3),
            23 => false, // TODO
            24 => true,
            25 => true,
            26 => true,
            27 => true,
            28 => !PowerupManager.Instance.DeckContains(18), // TODO: add investment only based on the CPU deck size (or playerdeckpowerlevel) to make sure it will be usable at somepoint
            29 => false, // TODO: equip omniscience only if cpu deck contains over 2/3 cards with a cost
            30 => false, // don't change this one from false, this is Plus Three
            _ => false,
        };
    }

    // if the CPU should play a card in a match
    public static bool EvaluatePowerupUsage(int cardIndex)
    {
        return cardIndex switch
        {
            0 => DeckInExcess(),
            1 => !powerupsUsedThisTurn.Contains(cardIndex) && FogScript.Instance.FogEnabled(),
            2 => LogicScript.Instance.player.puckCount > 0,
            3 => EvaluateBolt(),
            4 => !powerupsUsedThisTurn.Contains(cardIndex) && EvaluateForcefield(),
            5 => !powerupsUsedThisTurn.Contains(cardIndex) && usePhase,
            6 => !powerupsUsedThisTurn.Contains(cardIndex) && EvaluateCull(), // TODO: somehow... CPU should wait to use cull until AFTER forcefield is used
            7 => LogicScript.Instance.opponent.puckCount > 2,
            8 => LogicScript.Instance.player.puckCount > 0,
            9 => false, // TODO: explostion path
            10 => !powerupsUsedThisTurn.Contains(cardIndex) && LogicScript.Instance.player.puckCount > 0,
            11 => !powerupsUsedThisTurn.Contains(13) && LogicScript.Instance.player.puckCount > 0,
            12 => LogicScript.Instance.opponent.puckCount > 2 && !powerupsUsedThisTurn.Contains(22) && !powerupsUsedThisTurn.Contains(16),
            13 => !powerupsUsedThisTurn.Contains(11) && !powerupsUsedThisTurn.Contains(17) && LogicScript.Instance.player.puckCount > 0 && (powerupsUsedThisTurn.Contains(0) || powerupsUsedThisTurn.Contains(7) || powerupsUsedThisTurn.Contains(9) || powerupsUsedThisTurn.Contains(12) || powerupsUsedThisTurn.Contains(16) || powerupsUsedThisTurn.Contains(22) || powerupsUsedThisTurn.Contains(24) || powerupsUsedThisTurn.Contains(30)),
            14 => LogicScript.Instance.player.score > LogicScript.Instance.opponent.score && PuckManager.Instance.GetPucksInPlayCount() >= 2 && PuckManager.Instance.GetPucksInPlayCount(false, -1) >= 1 && PuckManager.Instance.GetPucksInPlayCount(false, 1) >= 1,
            // TODO: 2discardcosts
            15 => PowerupCountUsedThisTurn() == 0 && !hand.Contains(30) && false,
            16 => PowerupCountUsedThisTurn() == 0 && !hand.Contains(30) && LogicScript.Instance.opponent.puckCount <= 3 && !powerupsUsedThisTurn.Contains(12), // TODO: times two based on path value
            17 => PowerupCountUsedThisTurn() == 0 && !hand.Contains(30) && LogicScript.Instance.opponent.puckCount >= 3 && (deck.Contains(3) || deck.Contains(6)),
            18 => ((PowerupManager.Instance.GetDeck().Count / 2) < LogicScript.Instance.player.puckCount * 3 && PowerupManager.Instance.GetDeck().Count > 2) || (LogicScript.Instance.player.puckCount > 0 && PowerupManager.Instance.DeckContains(30)),
            19 => false,
            20 => false,
            21 => false, // TODO: use triple if CPU already used good stacking stuff
            22 => LogicScript.Instance.opponent.puckCount >= 5 && !powerupsUsedThisTurn.Contains(12),
            23 => false,
            24 => !powerupsUsedThisTurn.Contains(25) && DeckInExcess() && (PuckManager.Instance.GetPucksInPlayCount(true, -1) >= 3 || LogicScript.Instance.opponent.puckCount >= 3),
            25 => !powerupsUsedThisTurn.Contains(cardIndex) && !powerupsUsedThisTurn.Contains(24) && LogicScript.Instance.player.score > LogicScript.Instance.opponent.score && PuckManager.Instance.GetPucksInPlayCount(true) >= 3, // TODO: push based on path proximity to player pucks (this will be a nightmare to code) AT LEAST make sure angle is middle-ish so it's likely to do SOMETHING
            26 => !powerupsUsedThisTurn.Contains(cardIndex) && LogicScript.Instance.player.puckCount > 0,
            27 => !powerupsUsedThisTurn.Contains(cardIndex) && LogicScript.Instance.player.puckCount > 0 && PowerupManager.Instance.GetDeck().Count > 0,
            28 => (deck.Count) < (LogicScript.Instance.opponent.puckCount - 1) * 3,
            29 => false,
            30 => DeckInExcess(),
            _ => false,
        };
    }

    private static bool DeckInExcess()
    {
        return deck.Count > 3 * (LogicScript.Instance.opponent.puckCount - 1);
    }

    private static bool EvaluateBolt()
    {
        // if the ratio of player pucks to opponent pucks is greater than 2, use bolt
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        float playerPucks = 0;
        float opponentPucks = 0.001f; // so we don't divide by zero
        foreach (var puck in allPucks)
        {
            var puckScript = puck.GetComponent<PuckScript>();

            // players pucks
            if (puckScript.IsPlayersPuck() && puckScript.ComputeValue() > 0)
            {
                playerPucks++;
                // small postive weight relative to player puck total future value, and ignore shield to avoid exponent+shield being OP
                playerPucks += puckScript.ComputeTotalFutureValue() / 10;
                // small negative weight for player hydra
                if (puckScript.IsHydra() && !puckScript.IsShield())
                {
                    opponentPucks += 2;
                }
                // large negative weight for player resurrect
                if (puckScript.IsResurrect() && !puckScript.IsShield())
                {
                    opponentPucks += 5;
                }
            }
            // CPUs pucks
            else if (!puckScript.IsPlayersPuck() && puckScript.ComputeValue() > 0)
            {
                opponentPucks++;
                // small negative weight relative to CPU puck total future value if it has no shield
                opponentPucks += !puckScript.IsShield() ? puckScript.ComputeTotalFutureValue() / 10 : 0;
                // small postive weight for CPU hydra
                if (puckScript.IsHydra() && !puckScript.IsShield())
                {
                    playerPucks += 2;
                }
                // large postive weight for CPU resurrect
                if (puckScript.IsResurrect() && !puckScript.IsShield())
                {
                    playerPucks += 5;
                }
            }
        }
        if (playerPucks / opponentPucks > 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool EvaluateForcefield()
    {
        List<PuckScript> pucksInForceField = ForcefieldScript.Instance.GetPucksInForcefield();

        int CPUPucksInForcefield = 0;
        foreach (var puck in pucksInForceField)
        {
            if (!puck.IsPlayersPuck())
            {
                CPUPucksInForcefield++;

                // auto-trigger forcefield if CPU has a single highvalue puck
                if (puck.GetPuckBaseValue() > 1 || puck.GetPuckBonusValue() > 2)
                {
                    CPUPucksInForcefield++;
                }
            }
        }

        return CPUPucksInForcefield >= 2;
    }

    private static bool EvaluateCull()
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        bool useCull = true;
        int validPucks = 0;
        foreach (var puck in allPucks)
        {
            var puckScript = puck.GetComponent<PuckScript>();

            if (puckScript.ComputeValue() > 0)
            {
                continue;
            }

            // don't use cull if player has resurrect that would trigger
            if (puckScript.IsPlayersPuck() && puckScript.IsResurrect())
            {
                useCull = false;
                break;
            }
            // don't use cull if CPU has factory in a scoring zone that would be destroyed
            if (!puckScript.IsPlayersPuck() && puckScript.IsFactory() && puckScript.GetZoneMultiplier() > 0)
            {
                useCull = false;
                break;
            }

            if (puckScript.IsPlayersPuck() && !puckScript.IsHydra()) { validPucks++; }
            if (puckScript.IsPlayersPuck() && puckScript.IsFactory() && LogicScript.Instance.player.puckCount > 0) { validPucks++; } // a single player factory will trigger cull
            if (!puckScript.IsPlayersPuck() && puckScript.IsResurrect()) { validPucks++; } // a single CPU resurrect will trigger cull
            if (puckScript.IsPlayersPuck() && puckScript.IsExponent() && (puckScript.IsErratic() || PowerupManager.Instance.GetDeck().Contains(4))) { validPucks++; } // a single exponent+erratic or exponent+forcefield will trigger cull
        }

        return useCull && validPucks > 1;
    }

    public static bool HasExplosion()
    {
        return hand.Contains(9) && PowerupCountUsedThisTurn() < 3;
    }

    public static bool HasPhase()
    {
        return hand.Contains(5) && PowerupCountUsedThisTurn() < 3;
    }
}
