using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class PowerupManager : NetworkBehaviour
{
    // self
    public static PowerupManager Instance;

    // dependancies
    [SerializeField] private GameObject puckPrefab; 
    [SerializeField] private GameObject puckHalo;
    [SerializeField] private GameObject powerupsMenu;

    [SerializeField] private Button powerupButton1;
    [SerializeField] private Button powerupButton2;
    [SerializeField] private Button powerupButton3;

    [SerializeField] private GameObject popupEffectIconObject;
    [SerializeField] private Image popupEffectIcon;
    [SerializeField] private GameObject popupEffectTextObject;
    [SerializeField] private TMP_Text popupEffectText;

    // images
    [SerializeField] private Sprite plusOneImage;
    [SerializeField] private Sprite foresightImage;
    [SerializeField] private Sprite blockImage;
    [SerializeField] private Sprite boltImage;
    [SerializeField] private Sprite forceFieldImage;
    [SerializeField] private Sprite phaseImage;
    [SerializeField] private Sprite cullImage;
    [SerializeField] private Sprite growthImage;
    [SerializeField] private Sprite lockImage;
    [SerializeField] private Sprite explosionImage;
    [SerializeField] private Sprite fogImage;
    [SerializeField] private Sprite hydraImage;
    [SerializeField] private Sprite factoryImage;
    [SerializeField] private Sprite shieldImage;
    [SerializeField] private Sprite shuffleImage;
    [SerializeField] private Sprite chaosImage;
    [SerializeField] private Sprite timesTwoImage;
    [SerializeField] private Sprite resurrectImage;
    [SerializeField] private Sprite millImage;
    [SerializeField] private Sprite researchImage;
    [SerializeField] private Sprite insanityImage;
    [SerializeField] private Sprite tripleImage;
    [SerializeField] private Sprite exponentImage;
    [SerializeField] private Sprite laserImage;

    [SerializeField] private Sprite plusOneIcon;
    [SerializeField] private Sprite foresightIcon;
    [SerializeField] private Sprite blockIcon;
    [SerializeField] private Sprite boltIcon;
    [SerializeField] private Sprite forceFieldIcon;
    [SerializeField] private Sprite phaseIcon;
    [SerializeField] private Sprite cullIcon;
    [SerializeField] private Sprite growthIcon;
    [SerializeField] private Sprite lockIcon;
    [SerializeField] private Sprite explosionIcon;
    [SerializeField] private Sprite fogIcon;
    [SerializeField] private Sprite hydraIcon;
    [SerializeField] private Sprite factoryIcon;
    [SerializeField] private Sprite shieldIcon;
    [SerializeField] private Sprite shuffleIcon;
    [SerializeField] private Sprite chaosIcon;
    [SerializeField] private Sprite timesTwoIcon;
    [SerializeField] private Sprite resurrectIcon;
    [SerializeField] private Sprite millIcon;
    [SerializeField] private Sprite researchIcon;
    [SerializeField] private Sprite insanityIcon;
    [SerializeField] private Sprite tripleIcon;
    [SerializeField] private Sprite exponentIcon;
    [SerializeField] private Sprite laserIcon;

    // additional costs indexes
    private int[] cost2Discard = { 15, 16, 17}; // TODO: use indexOf in start method
    private int[] cost2Points = { 18, 19, 20 };
    private int[] cost1Puck = { 21, 22, 23 };

    public int[] GetCost2Discard() { return cost2Discard; }
    private List<int> deck;
    int[] hand = { -1, -1, -1 };

    private Competitor activeCompetitor;
    Action[] methodArray;
    private bool fromClientRpc;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        methodArray = new Action[]
        {
            PlusOnePowerup, // 0
            ForesightPowerup, // 1
            BlockPowerup, // 2
            BoltPowerup, // 3
            ForceFieldPowerup, // 4
            PhasePowerup, // 5
            CullPowerup, // 6
            GrowthPowerup, // 7
            LockPowerup, // 8
            ExplosionPowerup, // 9
            FogPowerup, // 10
            HydraPowerup, // 11
            FactoryPowerup, // 12
            ShieldPowerup, // 13
            ShufflePowerup, // 14
            ChaosPowerup, // 15
            TimesTwoPowerup, // 16
            ResurrectPowerup, // 17
            MillPowerup, // 18
            ResearchPowerup, // 19
            InsanityPowerup, // 20
            TriplePowerup, // 21
            ExponentPowerup, // 22
            LaserPowerup //23
        };
    }

    public int GetMethodArrayLength()
    {
        return methodArray.Length;
    }

    private void GetActiveCompetitor()
    {
        activeCompetitor = LogicScript.Instance.activeCompetitor;
        return;
    }

    public void LoadDeck()
    {
        deck = DeckManager.Instance.GetDeck();
        lastPlayedCard = -1;
    }

    // used by CPU during gameplay to check what the player has left in their deck
    public List<int> GetDeck()
    {
        return deck;
    }

    public void ShuffleDeck()
    {
        chaosEnsuing = false;
        if (deck == null) { LoadDeck(); }
        var deckCount = deck.Count;
        var pay2DiscardPossible = deck.Count >= 3;
        GetActiveCompetitor();
        Button[] powerupButtons = { powerupButton1, powerupButton2, powerupButton3 };
        Sprite[] powerupSprites = { plusOneImage, foresightImage, blockImage, boltImage, forceFieldImage, phaseImage, cullImage, growthImage, lockImage, explosionImage, fogImage, hydraImage, factoryImage, shieldImage, shuffleImage, chaosImage, timesTwoImage, resurrectImage, millImage, researchImage, insanityImage, tripleImage, exponentImage, laserImage };

        // generate 3 unique random powerups
        int[] previouslyGeneratedIndexes = { -1, -1, -1 };
        for (int i = 0; i < 3; i++)
        {
            powerupButtons[i].gameObject.SetActive(false);
            if (deckCount <= 0) { continue; }
            deckCount--;
            int randomCard;
            int randomIndex = Random.Range(0, deck.Count);
            // while empty in deck, reroll
            while (Array.Exists(previouslyGeneratedIndexes, element => element == randomIndex))
            {
                randomIndex = Random.Range(0, deck.Count);
            }
            previouslyGeneratedIndexes[i] = randomIndex;

            randomCard = deck[randomIndex];
            hand[i] = randomCard;
            powerupButtons[i].gameObject.SetActive(true);
            powerupButtons[i].gameObject.GetComponent<Button>().interactable = pay2DiscardPossible || !Array.Exists(cost2Discard, x => x == randomCard); // disable cost2Discard cards if hand size < 3
            DisableCost1PuckCardsIfNeeded();
            if (lastPlayedCard < 0 && randomCard == Array.IndexOf(methodArray, InsanityPowerup)) { powerupButtons[i].gameObject.GetComponent<Button>().interactable = false; } // special case for insanity in hand #1
            powerupButtons[i].image.sprite = powerupSprites[randomCard];
            powerupButtons[i].onClick.RemoveAllListeners();
            // add disable powerupmenu object function as listener
            int index = i; // this has to be here for some technical closure reason idk
            powerupButtons[i].onClick.AddListener(() => PowerupsHUDUIManager.Instance.UsePowerup(index, randomCard));
            powerupButtons[i].onClick.AddListener(() => methodArray[randomCard]());
            powerupButtons[i].onClick.AddListener(() => hand[index] = -1); // this is needed to not double remove from deck with research
            powerupButtons[i].onClick.AddListener(() => SoundManagerScript.Instance.PlayClickSFX(2));
        }
    }

    private void DisableCost2DiscardCards()
    {
        Button[] powerupButtons = { powerupButton1, powerupButton2, powerupButton3 };
        for (int i = 0; i < 3; i++)
        {
            if (Array.Exists(cost2Discard, x => x == hand[i]))
            {
                powerupButtons[i].gameObject.GetComponent<Button>().interactable = false;
            }
        }
    }

    private void DisableCost1PuckCardsIfNeeded()
    {
        // only do this if the active competitor has <= 1 puck remaining
        if (LogicScript.Instance.activeCompetitor.puckCount > 1) { return; }
        Button[] powerupButtons = { powerupButton1, powerupButton2, powerupButton3 };
        for (int i = 0; i < 3; i++)
        {
            if (Array.Exists(cost1Puck, x => x == hand[i]))
            {
                powerupButtons[i].gameObject.GetComponent<Button>().interactable = false;
            }
        }
    }

    private void DiscardHand()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            deck.Remove(hand[i]);
        }
    }

    public void PlusOnePowerup() // index 0 : give active puck +1 value
    {
        var index = Array.IndexOf(methodArray, PlusOnePowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.IncrementPuckBonusValue(1);

        activeCompetitor.activePuckScript.SetPowerupText("plus one");
    }

    public void ForesightPowerup() // index 1 : enable the shot predicted location halo
    {
        var index = Array.IndexOf(methodArray, ForesightPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        // user only
        if (activeCompetitor.isPlayer)
        {
            puckHalo.SetActive(true);
            puckHalo.GetComponent<HaloScript>().EnableFogMask(true);
            LineScript.Instance.HalfSpeed();
        }
    }

    public void BlockPowerup() // index 2 : create a valueless blocking puck
    {
        var index = Array.IndexOf(methodArray, BlockPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        if (ClientLogicScript.Instance.isRunning)
        {
            if (activeCompetitor.isPlayer)
            {
                ServerLogicScript.Instance.BlockServerRpc();
            }
            return;
        }

        int swap = activeCompetitor.isPlayer ? 1 : -1;
        GameObject blockPuckObject = Instantiate(puckPrefab, new Vector3(Random.Range(2f * swap, 4f * swap), Random.Range(2f, 4f), -1.0f), Quaternion.identity);
        PuckScript blockPuckScript = blockPuckObject.GetComponent<PuckScript>();
        blockPuckScript.InitPuck(activeCompetitor.isPlayer, activeCompetitor.puckSpriteID);
        blockPuckScript.InitBlockPuck();
    }

    private PuckScript pucki;
    public void BoltPowerup() // index 3 : destroy a random puck with value greater than or equal to 1
    {
        var index = Array.IndexOf(methodArray, BoltPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        int randomPuckIndex = Random.Range(0, allPucks.Length);
        pucki = allPucks[randomPuckIndex].GetComponent<PuckScript>();
        var iterationLimit = 0;
        while (pucki.ComputeValue() == 0 && iterationLimit < 1000)
        {
            randomPuckIndex = Random.Range(0, allPucks.Length);
            pucki = allPucks[randomPuckIndex].GetComponent<PuckScript>();
            iterationLimit++;
        }
        if (pucki != null && pucki.ComputeValue() > 0)
        {
            if (ClientLogicScript.Instance.isRunning && activeCompetitor.isPlayer) // Online
            {
                pucki.DestroyPuckServerRpc();
            }
            else if (!ClientLogicScript.Instance.isRunning) // vs CPU
            {
                pucki.DestroyPuck();
            }
        }
    }

    [SerializeField] private ForcefieldScript forcefieldScript;
    public void ForceFieldPowerup() // index 4 : forcefield pushes your pucks back from the off zone
    {
        var index = Array.IndexOf(methodArray, ForceFieldPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        forcefieldScript.EnableForcefield(activeCompetitor.isPlayer);
    }

    public void PhasePowerup() // index 5 : phases through (doesn't collide with) other pucks until stops
    {
        var index = Array.IndexOf(methodArray, PhasePowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.SetPowerupText("phase");
        activeCompetitor.activePuckScript.SetPhase(true);
    }

    public void CullPowerup() // index 6 : destroy all pucks valued <= 0
    {
        var index = Array.IndexOf(methodArray, CullPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            var pucki = puck.GetComponent<PuckScript>();
            if (pucki.ComputeValue() <= 0 && pucki.IsShot())
            {
                if (ClientLogicScript.Instance.isRunning && activeCompetitor.isPlayer)
                {
                    pucki.DestroyPuckServerRpc();
                }
                else
                {
                    pucki.DestroyPuck();
                }
            }
        }
    }

    public void GrowthPowerup() // index 7 : +1 value every shot
    {
        var index = Array.IndexOf(methodArray, GrowthPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.EnableGrowth();

        activeCompetitor.activePuckScript.SetPowerupText("growth");
    }

    public void LockPowerup() // index 8 : locked in place (can't be moved) after stopping
    {
        var index = Array.IndexOf(methodArray, LockPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.EnableLock();

        activeCompetitor.activePuckScript.SetPowerupText("lock");
    }

    public void ExplosionPowerup() // index 9 : destroys itself and first touched puck
    {
        var index = Array.IndexOf(methodArray, ExplosionPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.EnableExplosion();

        activeCompetitor.activePuckScript.SetPowerupText("explosion");
    }

    public void FogPowerup() // index 10 : fog blocks your opponent's vision
    {
        var index = Array.IndexOf(methodArray, FogPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        FogScript.Instance.StartListeners(activeCompetitor.isPlayer);
    }

    public void HydraPowerup() // index 11 : when destroyed, spawns 2 pucks
    {
        var index = Array.IndexOf(methodArray, HydraPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.EnableHydra();

        activeCompetitor.activePuckScript.SetPowerupText("hydra");
    }

    public void FactoryPowerup() // index 12 : active puck is valueless, but spawns a valued puck every shot
    {
        var index = Array.IndexOf(methodArray, FactoryPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.EnableFactory();

        activeCompetitor.activePuckScript.SetPowerupText("factory");
    }

    public void ShieldPowerup() // index 13 : prevent being destroyed once
    {
        var index = Array.IndexOf(methodArray, ShieldPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.EnableShield();

        activeCompetitor.activePuckScript.SetPowerupText("shield");
    }

    public void ShufflePowerup() // index 14 : randomize positions of all pucks
    {
        var index = Array.IndexOf(methodArray, ShufflePowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        if (!IsServer && ClientLogicScript.Instance.isRunning)
        {
            return;
        }

        // sort by valid / non-valid
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        int numOfValidPucks = 0;
        for (int i = 0; i < allPucks.Length; i++)
        {
            if (allPucks[i].transform.position.y > 0 && allPucks[i].transform.position.y < 20 && allPucks[i].transform.position.x > -12 && allPucks[i].transform.position.x < 12)
            {
                numOfValidPucks++;
            }
        }
        GameObject[] validPucks = new GameObject[numOfValidPucks];
        int j = 0;
        for (int i = 0; i < allPucks.Length; i++)
        {
            if (allPucks[i].transform.position.y > 0 && allPucks[i].transform.position.y < 20 && allPucks[i].transform.position.x > -12 && allPucks[i].transform.position.x < 12)
            {
                validPucks[j] = allPucks[i];
                j++;
            }
        }

        // get old positions of all pucks
        Vector3[] oldPuckPositions = new Vector3[numOfValidPucks];
        Vector3[] newPuckPositions = new Vector3[numOfValidPucks];
        for (int i = 0; i < numOfValidPucks; i++)
        {
            oldPuckPositions[i] = validPucks[i].transform.position;
            validPucks[i].transform.position = new Vector3(0, 20f * (i+1), validPucks[i].transform.position.z); // move it far away temporarily
        }
        // randomize their position (every puck must switch, that's why we don't do a built-in randomize)
        for (int i = 0; i < oldPuckPositions.Length; i++)
        {
            int iterations = 0;
            var newIndex = Random.Range(0, newPuckPositions.Length);
            // while the new position has already been chosen (not null & not zero) OR the new position is the same as the old one, reroll
            while (((newPuckPositions[newIndex] != null && newPuckPositions[newIndex] != Vector3.zero) || newIndex == i) && iterations < 1000)
            {
                newIndex = Random.Range(0, newPuckPositions.Length);
                iterations++;
            }
            if (iterations >= 1000) // fail case
            {
                newIndex = i;
            }
            newPuckPositions[newIndex] = oldPuckPositions[i];
        }
        // move the pucks
        for (int i = 0; i < numOfValidPucks; i++)
        {
            validPucks[i].transform.position = newPuckPositions[i]; // move the puck
        }
    }

    private bool chaosEnsuing = false;
    private void StopEnsuingChaos() { chaosEnsuing = false; }
    public void ChaosPowerup() // index 15 : activate 3 random powerup effects
    {
        var index = Array.IndexOf(methodArray, ChaosPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        chaosEnsuing = true;
        LogicScript.OnPlayerShot += StopEnsuingChaos;
        LogicScript.OnOpponentShot += StopEnsuingChaos;
        ClientLogicScript.OnPlayerShot += StopEnsuingChaos;
        ClientLogicScript.OnOpponentShot += StopEnsuingChaos;
        if (activeCompetitor.isPlayer)
        {
            int[] blacklist = { Array.IndexOf(methodArray, ResearchPowerup) };
            // call 3 different random methods from methodArray
            List<int> indexes = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, methodArray.Length);
                while (indexes.Contains(randomIndex) || Array.Exists(blacklist, x => x == randomIndex))
                {
                    randomIndex = Random.Range(0, methodArray.Length);
                }
                indexes.Add(randomIndex);
                methodArray[randomIndex].Invoke();
            }
        }
    }

    public void TimesTwoPowerup() // index 16 : give active puck x2 base value
    {
        var index = Array.IndexOf(methodArray, TimesTwoPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.DoublePuckBaseValue();

        activeCompetitor.activePuckScript.SetPowerupText("times two");
    }

    public void ResurrectPowerup() // index 17 : +1 puck count when destroyed
    {
        var index = Array.IndexOf(methodArray, ResurrectPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.EnableResurrect();

        activeCompetitor.activePuckScript.SetPowerupText("resurrect");
    }

    public void MillPowerup() // index 18 : discard half your opponent's deck rounded up
    {
        var index = Array.IndexOf(methodArray, MillPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        if (!activeCompetitor.isPlayer)
        {
            // Remove half the opponents deck, rounded up
            System.Random rand = new System.Random();
            if (deck == null) { LoadDeck(); }
            int countToRemove = (deck.Count + 1) / 2; // Round up
            HashSet<int> indexesToRemove = new HashSet<int>();

            // Select random unique indices
            while (indexesToRemove.Count < countToRemove)
            {
                int randomIndex = rand.Next(deck.Count);
                indexesToRemove.Add(randomIndex);
            }

            // Remove elements in reverse order to avoid shifting issues
            List<int> toRemove = new List<int>(indexesToRemove);
            toRemove.Sort((a, b) => b.CompareTo(a)); // Sort in descending order

            foreach (int indexToRemove in toRemove)
            {
                deck.RemoveAt(indexToRemove);
            }
        }
        else if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) // mill against CPU
        {
            LogicScript.Instance.MillPowerupHelper();
        }
    }

    public void ResearchPowerup() // index 19 : discard hand and draw 3
    {
        var index = Array.IndexOf(methodArray, ResearchPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        if (activeCompetitor.isPlayer)
        {
            DiscardHand();
            powerupsMenu.SetActive(false);
            ShuffleDeck();
            powerupsMenu.SetActive(true);
        }
    }

    private int lastPlayedCard;
    public void InsanityPowerup() // index 20 : add 3 copies of your last used card to deck
    {
        var index = Array.IndexOf(methodArray, InsanityPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        if (activeCompetitor.isPlayer && lastPlayedCard >= 0)
        {
            // add the last played card 3 times
            for (int i = 0; i < 3; i++)
            {
                deck.Add(lastPlayedCard);
            }
        }
        if (activeCompetitor.isPlayer && !chaosEnsuing) { lastPlayedCard = index; }
    }

    public void TriplePowerup() // index 21 : shoots 3 pucks instead of 1. (two additional pucks per instance of triple).
    {
        var index = Array.IndexOf(methodArray, TriplePowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { LogicScript.Instance.triplePowerup += 2; }
        else if (ClientLogicScript.Instance.isRunning && activeCompetitor.isPlayer) { ServerLogicScript.Instance.IncrementTriplePowerupServerRpc(); }
    }

    public void ExponentPowerup() // index 22 : double base value every shot
    {
        var index = Array.IndexOf(methodArray, ExponentPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        activeCompetitor.activePuckScript.EnableExponent();
        activeCompetitor.activePuckScript.SetPowerupText("exponent");
    }

    public GameObject laser;
    public void LaserPowerup() // index 23 : destroy all pucks in a line
    {
        var index = Array.IndexOf(methodArray, LaserPowerup);
        if (!CanPayCosts(index)) { return; }
        if (NeedsToBeSentToServer(index)) { return; }
        PayCosts(index);

        LaserScript.Instance.StartListeners(activeCompetitor.isPlayer);
    }

    private bool CanPayCosts(int index)
    {
        GetActiveCompetitor();

        // make sure the powerup costs are payable
        if (activeCompetitor.isPlayer && !chaosEnsuing)
        {
            // 2 discard (can't pay if two or more cards from our have already been played. must be two to allow card currently being used)
            if (cost2Discard.Contains(index) && hand.Count(x => x == -1) >= 2)
            {
                Debug.Log("Cannot pay 2 discard.");
                return false;
            }

            // 1 puck
            if (Array.Exists(cost1Puck, x => x == index) && activeCompetitor.puckCount <= 1)
            {
                Debug.Log("Cannot pay 1 puck.");
                return false;
            }
        }

        return true;
    }

    private void PayCosts(int index)
    {
        GetActiveCompetitor();

        // discard the played card from the deck and set the last played card for insanity (for just the person who played the card)
        if (activeCompetitor.isPlayer && !chaosEnsuing)
        {
            // set this powerup as the last played card, with an exception for insanity because it does this at the end of its execution
            if (index != Array.IndexOf(methodArray, InsanityPowerup))
            {
                lastPlayedCard = index;
            }
            // if the played card costs 2 discards, discard the whole hand
            if (Array.Exists(cost2Discard, x => x == index))
            {
                DiscardHand();
            }
            // otherwise only discard the played card
            else
            {
                deck.Remove(index);
            }
        }
        // if the played card costs 2 points, pay the cost
        if (Array.Exists(cost2Points, x => x == index) && !chaosEnsuing)
        {
            LogicScript.Instance.ModifyScoreBonus(activeCompetitor.isPlayer, -2);
        }
        // if the played card costs 1 puck, pay the cost
        if (Array.Exists(cost1Puck, x => x == index) && !chaosEnsuing)
        {
            LogicScript.Instance.IncrementPuckCount(activeCompetitor.isPlayer, -1);

            if (ClientLogicScript.Instance.isRunning && activeCompetitor.isPlayer)
            {
                ServerLogicScript.Instance.AdjustPuckCountServerRpc(true, -1);
            }
        }

        // disable any cards that cost 2 discards from hand
        DisableCost2DiscardCards();
        // disable any cards that cost 1 puck if the player only has 1 puck left
        DisableCost1PuckCardsIfNeeded();
        // add the powerup animation to the animation queue
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    private bool NeedsToBeSentToServer(int index)
    {
        // If used by the client before going to the server, instead send it to the server and stop the powerup functionality early
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            return true;
        }
        fromClientRpc = false;
        return false;
    }

    public void DisableForceFieldIfNecessary()
    {
        GetActiveCompetitor();
        if (forcefieldScript.IsPlayers() != activeCompetitor.isPlayer)
        {
            forcefieldScript.DisableForcefield();
        }
    }

    // Sending chosen powerup to server
    [ServerRpc(RequireOwnership = false)]
    public void PowerupServerRpc(int powerupActionIndex)
    {
        if (!IsServer) return;
        PowerupClientRpc(powerupActionIndex);
    }

    // receive chosen powerup from server
    [ClientRpc]
    public void PowerupClientRpc(int powerupActionIndex)
    {
        if (!IsClient) return;
        fromClientRpc = true;
        methodArray[powerupActionIndex].Invoke();
    }

    public void PuckSpawnHelper(bool isPlayers, float x, float y, int spwanCount)
    {
        Competitor hydraCompetitor = isPlayers ? LogicScript.Instance.player : LogicScript.Instance.opponent;
        Vector3 pos = Vector3.zero;

        // do X times (X is count)
        for (int i = 0; i < spwanCount; i++)
        {
            float randRange = 2.0f;
            // generate coordinates for potenial spawn, then see if it's too close to another puck
            bool tooClose = true;
            while (tooClose)
            {
                pos = new Vector3(x + Random.Range(-randRange, randRange), y + Random.Range(-randRange, randRange), -1.0f);

                tooClose = false;
                var pucks = GameObject.FindGameObjectsWithTag("puck");
                foreach (var puck in pucks)
                {
                    if (Vector2.Distance(puck.transform.position, pos) < 2)
                    {
                        tooClose = true;
                        break;
                    }
                }
                // expand possible range until we find a valid postion
                randRange += 0.1f;
            }

            GameObject puckObject = Instantiate(puckPrefab, pos, Quaternion.identity);
            PuckScript puckScript = puckObject.GetComponent<PuckScript>();
            puckScript.InitPuck(hydraCompetitor.isPlayer, hydraCompetitor.puckSpriteID);
        }
    }

    Queue<int> PowerupPopupEffectAnimationQueue = new Queue<int>();
    public void ClearPowerupPopupEffectAnimationQueue() { PowerupPopupEffectAnimationQueue.Clear(); }

    public void AddPowerupPopupEffectAnimationToQueue(int index)
    {
        PowerupPopupEffectAnimationQueue.Enqueue(index);
        if (PowerupPopupEffectAnimationQueue.Count == 1)
        {
            PlayNextPowerupPopupEffectAnimationInQueue();
        }
    }

    public void PlayNextPowerupPopupEffectAnimationInQueue()
    {
        if (PowerupPopupEffectAnimationQueue.Count <= 0) { return; }
        PlayPowerupPopupEffectAnimation(PowerupPopupEffectAnimationQueue.Peek());
    }

    public void FinishCurrentPowerupPopupEffectAnimationInQueue()
    {
        if (PowerupPopupEffectAnimationQueue.Count <= 0) { return; }
        PowerupPopupEffectAnimationQueue.Dequeue();
        if (PowerupPopupEffectAnimationQueue.Count >= 1)
        {
            PlayNextPowerupPopupEffectAnimationInQueue();
        }
    }

    public void PlayPowerupPopupEffectAnimation(int index)
    {
        Sprite[] powerupIcon = { plusOneIcon, foresightIcon, blockIcon, boltIcon, forceFieldIcon, phaseIcon, cullIcon, growthIcon, lockIcon, explosionIcon, fogIcon, hydraIcon, factoryIcon, shieldIcon, shuffleIcon, chaosIcon, timesTwoIcon, resurrectIcon, millIcon, researchIcon, insanityIcon, tripleIcon, exponentIcon, laserIcon };
        String[] powerupText = { "plus one", "foresight", "block", "bolt", "force field", "phase", "cull", "growth", "lock", "explosion", "fog", "hydra", "factory", "shield", "shuffle", "chaos", "times two", "resurrect", "mill", "research", "insanity", "triple", "exponent", "laser" };

        popupEffectIcon.sprite = powerupIcon[index];
        popupEffectText.text = powerupText[index];

        LeanTween.cancel(popupEffectIconObject);
        LeanTween.cancel(popupEffectTextObject);

        popupEffectIconObject.transform.localScale = new Vector3(0f, 0f, 0f);
        popupEffectTextObject.transform.localScale = new Vector3(0f, 0f, 0f);

        LeanTween.scale(popupEffectIconObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
        LeanTween.scale(popupEffectTextObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.2f);

        LeanTween.scale(popupEffectIconObject, new Vector3(0f, 0f, 0f), 0.5f).setEase(LeanTweenType.easeInElastic).setDelay(1.51f);
        LeanTween.scale(popupEffectTextObject, new Vector3(0f, 0f, 0f), 0.5f).setEase(LeanTweenType.easeInElastic).setDelay(1.7f).setOnComplete(FinishCurrentPowerupPopupEffectAnimationInQueue);
    }
}
