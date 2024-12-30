using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Unity.Netcode;

public class PowerupManager : NetworkBehaviour
{
    // self
    public static PowerupManager Instance;

    // dependancies
    [SerializeField] private GameObject puckPrefab; 
    [SerializeField] private GameObject puckHalo;
    [SerializeField] private GameObject powerupsMenu;

    // power up stuff, eventually should be put into it's own script
    [SerializeField] private Button powerupButton1;
    [SerializeField] private Button powerupButton2;
    [SerializeField] private Button powerupButton3;

    [SerializeField] private Sprite plusOneImage;
    [SerializeField] private Sprite foresightImage;
    [SerializeField] private Sprite blockImage;
    [SerializeField] private Sprite boltImage;
    [SerializeField] private Sprite forceFieldImage;
    [SerializeField] private Sprite phaseImage;

    private Competitor activeCompetitor;
    Action[] methodArray;
    private bool fromClientRpc;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void Start()
    {
        methodArray = new Action[]
        {
            PlusOnePowerup,
            ForesightPowerup,
            BlockPowerup,
            BoltPowerup,
            ForceFieldPowerup,
            PhasePowerup
        };
    }

    private void Update()
    {
        activeCompetitor = LogicScript.Instance.activeCompetitor;
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

    public void ShufflePowerups()
    {
        Button[] powerupButtons = { powerupButton1, powerupButton2, powerupButton3 };
        Sprite[] powerupSprites = { plusOneImage, foresightImage, blockImage, boltImage, forceFieldImage, phaseImage };

        // generate 3 unique random powerups
        int[] randomPowerups = { 0, 1, 2 };
        for (int i = 0; i < 3; i++)
        {
            int randomPowerup = i;
            if (activeCompetitor.puckCount != 5) // first hand is predetermined
            {
                randomPowerup = Random.Range(0, methodArray.Length);
                while (Array.Exists(randomPowerups, element => element == randomPowerup))
                {
                    randomPowerup = Random.Range(0, methodArray.Length);
                }
            }
            randomPowerups[i] = randomPowerup;
            powerupButtons[i].image.sprite = powerupSprites[randomPowerup];
            powerupButtons[i].onClick.RemoveAllListeners();
            powerupButtons[i].onClick.AddListener(() => methodArray[randomPowerup]());
            // add disable powerupmenu object function as listener
            powerupButtons[i].onClick.AddListener(() => powerupsMenu.SetActive(false));
        }
    }

    public void PlusOnePowerup() // give active puck +1 value
    {
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(0);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPuckBonusValue(1);
        activeCompetitor.activePuckScript.SetPowerupText("plus one");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
    }

    public void ForesightPowerup() // enable the shot predicted location halo
    {
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(1);
            return;
        }
        fromClientRpc = false;

        puckHalo.SetActive(true);
        activeCompetitor.activePuckScript.SetPowerupText("foresight");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
    }

    public void BlockPowerup() // create a valueless blocking puck
    {
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(2);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        int swap = activeCompetitor.isPlayer || (ClientLogicScript.Instance.isRunning && activeCompetitor.goingFirst) ? 1 : -1;
        GameObject blockPuckObject = Instantiate(puckPrefab, new Vector3(Random.Range(2f * swap, 4f * swap), Random.Range(2f, 4f), -1.0f), Quaternion.identity);
        if (ClientLogicScript.Instance.isRunning) { blockPuckObject.GetComponent<NetworkObject>().Spawn(); }
        PuckScript blockPuckScript = blockPuckObject.GetComponent<PuckScript>();
        blockPuckScript.InitPuck(activeCompetitor.isPlayer, activeCompetitor.puckSpriteID);
        blockPuckScript.SetPuckBaseValue(0);
        blockPuckScript.SetPowerupText("valueless");
        blockPuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.SetPowerupText("block");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
    }

    private PuckScript pucki;
    public void BoltPowerup() // destroy a random puck with value greater than or equal to 1
    {
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(3);
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
                pucki.DestroyPuckServerRpc(); // for online
            }
            else
            {
                pucki.DestroyPuck();
            }
            activeCompetitor.activePuckScript.SetPowerupText("bolt");
            activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        }
    }

    [SerializeField] private ForcefieldScript forcefieldScript;
    public void ForceFieldPowerup()
    {
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(4);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPowerupText("force field");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        forcefieldScript.EnableForcefield(activeCompetitor.isPlayer);
    }

    public void PhasePowerup()
    {
        if (!fromClientRpc && ClientLogicScript.Instance.isRunning)
        {
            PowerupServerRpc(5);
            return;
        }
        fromClientRpc = false;
        GetActiveCompetitor();

        activeCompetitor.activePuckScript.SetPowerupText("phase");
        activeCompetitor.activePuckScript.CreatePowerupFloatingText();
        activeCompetitor.activePuckScript.SetPhase(true);
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
}
