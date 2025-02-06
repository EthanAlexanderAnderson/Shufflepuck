using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

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
    [SerializeField] private Sprite insanityImage;
    [SerializeField] private Sprite timesTwoImage;
    [SerializeField] private Sprite resurrectImage;
    [SerializeField] private Sprite millImage;
    [SerializeField] private Sprite researchImage;
    [SerializeField] private Sprite chaosImage;

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
    [SerializeField] private Sprite insanityIcon;
    [SerializeField] private Sprite timesTwoIcon;
    [SerializeField] private Sprite resurrectIcon;
    [SerializeField] private Sprite millIcon;
    [SerializeField] private Sprite researchIcon;
    [SerializeField] private Sprite chaosIcon;

    // additional costs indexes
    public int[] cost2Discard = { 15, 16, 17};
    public int[] cost2Points = { 18, 19, 20 };

    private List<int> deck;

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
            PlusOnePowerup,
            ForesightPowerup,
            BlockPowerup,
            BoltPowerup,
            ForceFieldPowerup,
            PhasePowerup,
            CullPowerup,
            GrowthPowerup,
            LockPowerup,
            ExplosionPowerup,
            FogPowerup,
            HydraPowerup,
            FactoryPowerup,
            ShieldPowerup,
            ShufflePowerup,
            InsanityPowerup,
            TimesTwoPowerup,
            ResurrectPowerup,
            MillPowerup,
            ResearchPowerup,
            ChaosPowerup
        };
    }

    public int GetMethodArrayLength()
    {
        return methodArray.Length;
    }

    private void GetActiveCompetitor()
    {
        if (!ClientLogicScript.Instance.isRunning) // offline
        {
            activeCompetitor = LogicScript.Instance.activeCompetitor;
        }
        else // online
        {
            activeCompetitor = ClientLogicScript.Instance.client;
        }
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

    int[] hand = { -1, -1, -1 };
    public void ShuffleDeck()
    {
        if (deck == null) { LoadDeck(); }
        var deckCount = deck.Count;
        var pay2DiscardPossible = deck.Count >= 3;
        GetActiveCompetitor();
        Button[] powerupButtons = { powerupButton1, powerupButton2, powerupButton3 };
        Sprite[] powerupSprites = { plusOneImage, foresightImage, blockImage, boltImage, forceFieldImage, phaseImage, cullImage, growthImage, lockImage, explosionImage, fogImage, hydraImage, factoryImage, shieldImage, shuffleImage, insanityImage, timesTwoImage, resurrectImage, millImage, researchImage, chaosImage };

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
            if (lastPlayedCard < 0 && randomCard == 15) { powerupButtons[i].gameObject.GetComponent<Button>().interactable = false; } // special case for insanity in hand #1
            powerupButtons[i].image.sprite = powerupSprites[randomCard];
            powerupButtons[i].onClick.RemoveAllListeners();
            // add disable powerupmenu object function as listener
            int index = i; // this has to be here for some technical closure reason idk
            powerupButtons[i].onClick.AddListener(() => PowerupsHUDUIManager.Instance.UsePowerup(index, randomCard));
            powerupButtons[i].onClick.AddListener(() => methodArray[randomCard]());
            powerupButtons[i].onClick.AddListener(() => hand[index] = -1);
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

    private void DiscardHand()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            deck.Remove(hand[i]);
        }
    }

    public void PlusOnePowerup() // give active puck +1 value
    {
        var index = Array.IndexOf(methodArray, PlusOnePowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        activeCompetitor.activePuckScript.IncrementPuckBonusValue(1);
        activeCompetitor.activePuckScript.SetPowerupText("plus one");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void ForesightPowerup() // enable the shot predicted location halo
    {
        var index = Array.IndexOf(methodArray, ForesightPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        puckHalo.SetActive(true);
        puckHalo.GetComponent<HaloScript>().EnableFogMask(true);
        LineScript.Instance.HalfSpeed();
        //activeCompetitor.activePuckScript.SetPowerupText("foresight");
        //activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void BlockPowerup() // create a valueless blocking puck
    {
        var index = Array.IndexOf(methodArray, BlockPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            ServerLogicScript.Instance.BlockServerRpc();
            deck.Remove(index);
            return; // for online mode, this function always returns here, unlike other powerups, because it gets handed off to it's dedicated serverrpc function
        }
        fromClientRpc = false;

        int swap = activeCompetitor.isPlayer ? 1 : -1;
        GameObject blockPuckObject = Instantiate(puckPrefab, new Vector3(Random.Range(2f * swap, 4f * swap), Random.Range(2f, 4f), -1.0f), Quaternion.identity);
        PuckScript blockPuckScript = blockPuckObject.GetComponent<PuckScript>();
        blockPuckScript.InitPuck(activeCompetitor.isPlayer, activeCompetitor.puckSpriteID);
        blockPuckScript.SetPuckBaseValue(0);
        blockPuckScript.SetPowerupText("valueless");
        blockPuckScript.CreatePowerupFloatingText();
        //activeCompetitor.activePuckScript.SetPowerupText("block");
        //activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    private PuckScript pucki;
    public void BoltPowerup() // destroy a random puck with value greater than or equal to 1
    {
        var index = Array.IndexOf(methodArray, BoltPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

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
            if (ClientLogicScript.Instance.isRunning)
            {
                pucki.DestroyPuckServerRpc(); // for online. This only triggers once, because calling this requires ownership of the puck
            }
            else
            {
                pucki.DestroyPuck();
            }
        }
        //activeCompetitor.activePuckScript.SetPowerupText("bolt");
        //activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    [SerializeField] private ForcefieldScript forcefieldScript;
    public void ForceFieldPowerup() // forcefield pushes your pucks back from the off zone
    {
        var index = Array.IndexOf(methodArray, ForceFieldPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        //activeCompetitor.activePuckScript.SetPowerupText("force field");
        //activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        forcefieldScript.EnableForcefield(activeCompetitor.isPlayer);
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void PhasePowerup() // phases through (doesn't collide with) other pucks until stops
    {
        var index = Array.IndexOf(methodArray, PhasePowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        activeCompetitor.activePuckScript.SetPowerupText("phase");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.SetPhase(true);
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void CullPowerup() // destroy all pucks valued <= 0
    {
        var index = Array.IndexOf(methodArray, CullPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            var pucki = puck.GetComponent<PuckScript>();
            if (pucki.ComputeValue() <= 0 && pucki.IsShot())
            {
                if (ClientLogicScript.Instance.isRunning)
                {
                    pucki.DestroyPuckServerRpc(); // for online. This only triggers once, because calling this requires ownership of the puck
                }
                else
                {
                    pucki.DestroyPuck();
                }
            }
        }
        //activeCompetitor.activePuckScript.SetPowerupText("cull");
        //activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void GrowthPowerup() // +1 value every shot
    {
        var index = Array.IndexOf(methodArray, GrowthPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        activeCompetitor.activePuckScript.SetPowerupText("growth");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableGrowth();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void LockPowerup() // locked in place (can't be moved) after stopping
    {
        var index = Array.IndexOf(methodArray, LockPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        activeCompetitor.activePuckScript.SetPowerupText("lock");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableLock();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void ExplosionPowerup() // destroys itself and first touched puck
    {
        var index = Array.IndexOf(methodArray, ExplosionPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        activeCompetitor.activePuckScript.SetPowerupText("explosion");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableExplosion();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void FogPowerup() // fog blocks your opponent's vision
    {
        var index = Array.IndexOf(methodArray, FogPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        //activeCompetitor.activePuckScript.SetPowerupText("fog");
        //activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        FogScript.Instance.StartListeners(activeCompetitor.isPlayer);
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void HydraPowerup() // when destroyed, spawns 2 pucks
    {
        var index = Array.IndexOf(methodArray, HydraPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        activeCompetitor.activePuckScript.SetPowerupText("hydra");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableHydra();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void FactoryPowerup() // active puck is valueless, but spawns a valued puck every shot
    {
        var index = Array.IndexOf(methodArray, FactoryPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        activeCompetitor.activePuckScript.SetPowerupText("factory");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableFactory();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void ShieldPowerup() // prevent being destroyed once
    {
        var index = Array.IndexOf(methodArray, ShieldPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        activeCompetitor.activePuckScript.SetPowerupText("shield");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableShield();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void ShufflePowerup() // randomize positions of all pucks
    {
        var index = Array.IndexOf(methodArray, ShufflePowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
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
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    private int lastPlayedCard;
    public void InsanityPowerup() // add 3 copies of your last used card to deck
    {
        var index = Array.IndexOf(methodArray, InsanityPowerup);
        GetActiveCompetitor();
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            DiscardHand();
            return;
        }
        fromClientRpc = false;

        if (activeCompetitor.isPlayer && lastPlayedCard >= 0)
        {
            // add the last played card 3 times
            for (int i = 0; i < 3; i++)
            {
                deck.Add(lastPlayedCard);
            }
        }
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { if (Array.Exists(cost2Discard, x => x == index) && !chaosEnsuing) { DiscardHand(); } else { deck.Remove(index); } }
        AddPowerupPopupEffectAnimationToQueue(index);
    }
    public void TimesTwoPowerup() // give active puck x2 base value
    {
        var index = Array.IndexOf(methodArray, TimesTwoPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        activeCompetitor.activePuckScript.DoublePuckBaseValue();
        activeCompetitor.activePuckScript.SetPowerupText("times two");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { if (Array.Exists(cost2Discard, x => x == index) && !chaosEnsuing) { DiscardHand(); } else { deck.Remove(index); } }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void ResurrectPowerup() // +1 puck count when destroyed
    {
        var index = Array.IndexOf(methodArray, ResurrectPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            DiscardHand();
            return;
        }
        fromClientRpc = false;

        activeCompetitor.activePuckScript.SetPowerupText("resurrect");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableResurrect();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { if (Array.Exists(cost2Discard, x => x == index) && !chaosEnsuing) { DiscardHand(); } else { deck.Remove(index); } }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void MillPowerup() // discard half your opponent�s deck rounded up
    {
        var index = Array.IndexOf(methodArray, MillPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            DiscardHand();
            return;
        }
        fromClientRpc = false;

        if (!activeCompetitor.isPlayer)
        {
            // Remove half the opponents deck, rounded up
            System.Random rand = new System.Random();
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

        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { if (Array.Exists(cost2Discard, x => x == index) && !chaosEnsuing) { DiscardHand(); } else { deck.Remove(index); } }
        if (Array.Exists(cost2Points, x => x == index) && !chaosEnsuing) { LogicScript.Instance.ModifyScoreBonus(activeCompetitor.isPlayer, -2); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    public void ResearchPowerup() // discard hand and draw 3
    {
        var index = Array.IndexOf(methodArray, ResearchPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            DiscardHand();
            return;
        }
        fromClientRpc = false;

        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer)
        {
            DiscardHand();
            powerupsMenu.SetActive(false);
            ShuffleDeck();
            powerupsMenu.SetActive(true);
        }

        if (Array.Exists(cost2Points, x => x == index) && !chaosEnsuing) { LogicScript.Instance.ModifyScoreBonus(activeCompetitor.isPlayer, -2); }
        AddPowerupPopupEffectAnimationToQueue(index);
    }

    private bool chaosEnsuing = false;
    public void ChaosPowerup() // activate 3 random powerup effects
    {
        var index = Array.IndexOf(methodArray, ChaosPowerup);
        GetActiveCompetitor();
        if (activeCompetitor.isPlayer) { lastPlayedCard = index; }
        DisableCost2DiscardCards();
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;

        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { if (Array.Exists(cost2Discard, x => x == index) && !chaosEnsuing) { DiscardHand(); } else { deck.Remove(index); } }
        if (Array.Exists(cost2Points, x => x == index) && !chaosEnsuing) { LogicScript.Instance.ModifyScoreBonus(activeCompetitor.isPlayer, -2); }
        AddPowerupPopupEffectAnimationToQueue(index);

        if (activeCompetitor.isPlayer)
        {
            chaosEnsuing = true;
            // call 3 different random methods from methodArray
            List<int> indexes = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, methodArray.Length);
                while (indexes.Contains(randomIndex))
                {
                    randomIndex = Random.Range(0, methodArray.Length);
                }
                indexes.Add(randomIndex);
                methodArray[randomIndex].Invoke();
            }
            chaosEnsuing = false;
        }
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
        Competitor hydraCompetitor = isPlayers ? LogicScript.Instance.player : LogicScript.Instance.player;
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
        Sprite[] powerupIcon = { plusOneIcon, foresightIcon, blockIcon, boltIcon, forceFieldIcon, phaseIcon, cullIcon, growthIcon, lockIcon, explosionIcon, fogIcon, hydraIcon, factoryIcon, shieldIcon, shuffleIcon, insanityIcon, timesTwoIcon, resurrectIcon, millIcon, researchIcon, chaosIcon };
        String[] powerupText = { "plus one", "foresight", "block", "bolt", "force field", "phase", "cull", "growth", "lock", "explosion", "fog", "hydra", "factory", "shield", "shuffle", "insanity", "times two", "resurrect", "mill", "research", "chaos" };

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
