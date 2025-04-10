using System;
using System.Collections;
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

        for (int i = 0; i < modifiedDifficulty; i++)
        {
            hand[i] = -1;
        }
    }

    // TODO: make sure this is called after PowerupManager.Instance.LoadDeck to accurately determine player deck power level
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

        // modify the power level based on CPU difficulty
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

        // any cards in deck -> increases modified difficulty by 1
        // every additional 15 deck power -> increases modified difficulty by 1
        modifiedDifficulty = difficulty + (playerDeckPowerLevel + 14) / 15;
        Debug.Log("CPU modifiedDifficulty set to: " + modifiedDifficulty);
    }

    private static void DrawHand()
    {
        int deckCount = deck.Count;

        int[] previouslyGeneratedIndexes = new int[modifiedDifficulty];

        for (int i = 0; i < modifiedDifficulty; i++)
        {
            previouslyGeneratedIndexes[i] = -1;
        }

        for (int i = 0; i < hand.Length; i++)
        {
            if (deckCount <= 0) { continue; }
            deckCount--;

            if (denyPowerup > 1 || denyPowerup == 1 && i < 2)
            {
                hand[i] = -1; 
                continue;
            }

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
                    waitingTime += 1.5f;
                    LogicScript.Instance.powerupWaitTime += 1f;
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

    private static bool UseNextCard()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i] == -1) continue;
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
        // initialize CPU paths
        CPUPaths = GameObject.FindGameObjectsWithTag("cpu_path");

        // if Fog is active and foresight isn't, shoot random
        if ((FogScript.Instance.FogEnabled() && !HaloScript.Instance.HaloEnabled())) { return (Random.Range(20.0f, 60.0f), Random.Range(60.0f, 80.0f), Random.Range(45.0f, 55.0f)); }

        CPUPathInterface best = null;
        int highestValue = 0;
        // check all paths to see which are unblocked
        foreach (var path in CPUPaths)
        {
            pathi = path.GetComponent<CPUPathScript>();
            if (pathi == null) { pathi = path.GetComponent<SmartScanCPUPathScript>(); }

            pathi.DisablePathVisualization();

            var pathiShotValue = pathi.CalculateValue(modifiedDifficulty);

            if (pathi.GetPath().spin != 50 && difficulty < 2) { continue; } // skip path if it needs spin and we're not playing against hard CPU

            float randomValue = Random.Range(0f, 1f);
            if (pathiShotValue > highestValue || (pathiShotValue == highestValue && randomValue < 0.25))
            {
                best = pathi;
                highestValue = pathiShotValue;
            }
        }
        // chose the highest-value unblocked path
        if (highestValue > 0)
        {
            Debug.Log("Found path with highest value " + highestValue);
            best.EnablePathVisualization();

            // try to phase lol
            usePhase = best.DoesPathRequirePhasePowerup();
            if (best.DoesPathRequirePhasePowerup())
            {
                for (int i = 0; i < hand.Count(); i++)
                {
                    if (hand.Any(n => n != -1) && PowerupCountUsedThisTurn() < 3)
                    {
                        UseNextCard();
                    }
                }
            }

            return best.GetPath();
        }
        // otherwise, Shoot random
        else
        {
            Debug.Log("No path :(");
            float variance = Random.Range(0f, (5 - modifiedDifficulty) * 3);
            return (Random.Range(20.0f - variance, 60.0f + variance), Random.Range(65.0f - variance, 80.0f + variance), Random.Range(45.0f - variance, 55.0f + variance));
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
            13 => true,
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
            28 => !PowerupManager.Instance.DeckContains(18),
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
            6 => !powerupsUsedThisTurn.Contains(cardIndex) && EvaluateCull(),
            7 => LogicScript.Instance.opponent.puckCount > 1,
            8 => LogicScript.Instance.player.puckCount > 0,
            // TODO: explostion path
            9 => false,
            10 => !powerupsUsedThisTurn.Contains(cardIndex) && LogicScript.Instance.player.puckCount > 0,
            11 => !powerupsUsedThisTurn.Contains(13) && LogicScript.Instance.player.puckCount > 0,
            12 => LogicScript.Instance.opponent.puckCount > 2,
            13 => !powerupsUsedThisTurn.Contains(11) && !powerupsUsedThisTurn.Contains(17) && LogicScript.Instance.player.puckCount > 0,
            14 => LogicScript.Instance.player.score > LogicScript.Instance.opponent.score && PuckManager.Instance.GetPucksInPlayCount() >= 2,
            // TODO: 2discardcosts
            15 => false,
            16 => true, // TODO: times two based on path value
            17 => LogicScript.Instance.opponent.puckCount >= 3 && (deck.Contains(3) || deck.Contains(6)),
            18 => ((PowerupManager.Instance.GetDeck().Count / 2) < LogicScript.Instance.player.puckCount * 3 && PowerupManager.Instance.GetDeck().Count > 2) || PowerupManager.Instance.DeckContains(30),
            19 => false,
            20 => false,
            21 => false, // TODO: use triple if CPU already used good stacking stuff
            22 => LogicScript.Instance.opponent.puckCount >= 5,
            23 => false,
            24 => DeckInExcess(),
            25 => LogicScript.Instance.player.score > LogicScript.Instance.opponent.score,
            26 => !powerupsUsedThisTurn.Contains(cardIndex) && LogicScript.Instance.player.puckCount > 0,
            27 => !powerupsUsedThisTurn.Contains(cardIndex) && LogicScript.Instance.player.puckCount > 0,
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

            // check normal pucks
            if (puckScript.IsPlayersPuck() && puckScript.ComputeValue() > 0)
            {
                playerPucks++;
                // extra weight for high value pucks
                if (puckScript.ComputeValue() > 10)
                {
                    playerPucks++;
                }
                // small negative weight for hydra
                if (puckScript.IsHydra())
                {
                    opponentPucks += 2;
                }
                // large negative weight for resurrect
                if (puckScript.IsResurrect())
                {
                    opponentPucks += 5;
                }
            }
            else if (!puckScript.IsPlayersPuck() && puckScript.ComputeValue() > 0)
            {
                opponentPucks++;
                // extra weight for high value pucks
                if (puckScript.ComputeValue() > 10)
                {
                    opponentPucks++;
                }
                // small negative weight for hydra
                if (puckScript.IsHydra())
                {
                    playerPucks += 2;
                }
                // large negative weight for resurrect
                if (puckScript.IsResurrect())
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

        Debug.Log("CPUPucksInForcefield: " + CPUPucksInForcefield);
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
            // don't use cull if CPU has factory that would be destroyed
            if (!puckScript.IsPlayersPuck() && puckScript.IsFactory())
            {
                useCull = false;
                break;
            }

            if (puckScript.IsPlayersPuck()) { validPucks++; }
            if (puckScript.IsPlayersPuck() && puckScript.IsFactory()) { validPucks++; } // a single player factory will trigger cull
            if (!puckScript.IsPlayersPuck() && puckScript.IsResurrect()) { validPucks++; } // a single CPU resurrect will trigger cull
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
