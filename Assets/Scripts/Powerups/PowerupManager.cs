using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Unity.Netcode;
using System.Collections.Generic;

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
            HydraPowerup
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
    }

    // used by CPU during gameplay to check what the player has left in their deck
    public List<int> GetDeck()
    {
        return deck;
    }

    public void ShufflePowerups()
    {
        if (deck == null) { LoadDeck(); }
        GetActiveCompetitor();
        Button[] powerupButtons = { powerupButton1, powerupButton2, powerupButton3 };
        Sprite[] powerupSprites = { plusOneImage, foresightImage, blockImage, boltImage, forceFieldImage, phaseImage, cullImage, growthImage, lockImage, explosionImage, fogImage, hydraImage };

        // generate 3 unique random powerups
        int[] randomPowerups = { 0, 1, 2 };
        int[] previouslyGeneratedIndexes = { -1, -1, -1 };
        for (int i = 0; i < 3; i++)
        {
            int randomCard;
            int randomIndex = Random.Range(0, deck.Count);
            // while empty in deck, reroll
            while (Array.Exists(previouslyGeneratedIndexes, element => element == randomIndex))
            {
                randomIndex = Random.Range(0, deck.Count);
            }
            previouslyGeneratedIndexes[i] = randomIndex;

            randomCard = deck[randomIndex];
            randomPowerups[i] = randomCard;
            powerupButtons[i].image.sprite = powerupSprites[randomCard];
            powerupButtons[i].onClick.RemoveAllListeners();
            powerupButtons[i].onClick.AddListener(() => methodArray[randomCard]());
            // add disable powerupmenu object function as listener
            powerupButtons[i].onClick.AddListener(() => powerupsMenu.SetActive(false));
        }
    }

    public void PlusOnePowerup() // give active puck +1 value
    {
        var index = Array.IndexOf(methodArray, PlusOnePowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPuckBonusValue(1);
        activeCompetitor.activePuckScript.SetPowerupText("plus one");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    public void ForesightPowerup() // enable the shot predicted location halo
    {
        var index = Array.IndexOf(methodArray, ForesightPowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        puckHalo.SetActive(true);
        puckHalo.GetComponent<HaloScript>().EnableFogMask(true);
        LineScript.Instance.HalfSpeed();
        activeCompetitor.activePuckScript.SetPowerupText("foresight");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    public void BlockPowerup() // create a valueless blocking puck
    {
        var index = Array.IndexOf(methodArray, BlockPowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            ServerLogicScript.Instance.BlockServerRpc();
            deck.Remove(index);
            return; // for online mode, this function always returns here, unlike other powerups, because it gets handed off to it's dedicated serverrpc function
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        int swap = activeCompetitor.isPlayer ? 1 : -1;
        GameObject blockPuckObject = Instantiate(puckPrefab, new Vector3(Random.Range(2f * swap, 4f * swap), Random.Range(2f, 4f), -1.0f), Quaternion.identity);
        PuckScript blockPuckScript = blockPuckObject.GetComponent<PuckScript>();
        blockPuckScript.InitPuck(activeCompetitor.isPlayer, activeCompetitor.puckSpriteID);
        blockPuckScript.SetPuckBaseValue(0);
        blockPuckScript.SetPowerupText("valueless");
        blockPuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.SetPowerupText("block");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    private PuckScript pucki;
    public void BoltPowerup() // destroy a random puck with value greater than or equal to 1
    {
        var index = Array.IndexOf(methodArray, BoltPowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

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
        activeCompetitor.activePuckScript.SetPowerupText("bolt");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    [SerializeField] private ForcefieldScript forcefieldScript;
    public void ForceFieldPowerup()
    {
        var index = Array.IndexOf(methodArray, ForceFieldPowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPowerupText("force field");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        forcefieldScript.EnableForcefield(activeCompetitor.isPlayer);
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    public void PhasePowerup()
    {
        var index = Array.IndexOf(methodArray, PhasePowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPowerupText("phase");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.SetPhase(true);
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    public void CullPowerup()
    {
        var index = Array.IndexOf(methodArray, CullPowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

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
        activeCompetitor.activePuckScript.SetPowerupText("cull");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    public void GrowthPowerup()
    {
        var index = Array.IndexOf(methodArray, GrowthPowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPowerupText("growth");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableGrowth();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    public void LockPowerup()
    {
        var index = Array.IndexOf(methodArray, LockPowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPowerupText("lock");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableLock();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    public void ExplosionPowerup()
    {
        var index = Array.IndexOf(methodArray, ExplosionPowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPowerupText("explosion");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableExplosion();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    public void FogPowerup()
    {
        var index = Array.IndexOf(methodArray, FogPowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPowerupText("fog");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        FogScript.Instance.StartListeners(activeCompetitor.isPlayer);
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
    }

    public void HydraPowerup()
    {
        var index = Array.IndexOf(methodArray, HydraPowerup);
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(index);
            deck.Remove(index);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPowerupText("hydra");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.EnableHydra();
        if (LogicScript.Instance.gameIsRunning && activeCompetitor.isPlayer) { deck.Remove(index); }
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

    public void HydraHelper(bool isPlayers, float x, float y)
    {
        Competitor hydraCompetitor = isPlayers ? LogicScript.Instance.player : LogicScript.Instance.player;
        Vector3 pos = Vector3.zero;

        // do twice (spawn 2 pucks)
        for (int i = 0; i < 2; i++)
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
}
