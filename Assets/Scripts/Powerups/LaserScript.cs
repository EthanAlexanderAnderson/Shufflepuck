using System.Collections.Generic;
using UnityEngine;

//Attach this script to a GameObject to rotate around the target position.
public class LaserScript : MonoBehaviour
{
    // self
    public static LaserScript Instance;

    // dependancies
    private LineScript line;
    private LogicScript logic;
    private ClientLogicScript clientLogic;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private GameObject anchorPoint;

    private float sideModifier;
    private string activeBar;

    // state
    private bool laserEnabled = false;

    // base parameters
    [SerializeField] private float xBase = 0f;
    [SerializeField] private float yBase = 25f;
    [SerializeField] private float zBase = 0f;

    // moving parameters
    [SerializeField] private float angleParameter = 0.6f;
    [SerializeField] private float powerParameter = 10f;

    // pucks in laser path
    List<GameObject> pucksInPath = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    // Start is called before the first frame update
    void Start()
    {
        line = LineScript.Instance;
        logic = LogicScript.Instance;
        clientLogic = ClientLogicScript.Instance;
    }

    //Assign a GameObject in the Inspector to rotate around
    void Update()
    {
        if (!laserEnabled) { return; } // optimization

        if (logic != null && logic.gameIsRunning)
        {
            sideModifier = logic.activeCompetitor.isPlayer ? (-3.6f) : (3.6f);
            activeBar = logic.activeBar;
        }
        else if (clientLogic != null && clientLogic.isRunning)
        {
            sideModifier = clientLogic.IsStartingPlayer() ? (-3.6f) : (3.6f);
            activeBar = clientLogic.activeBar;
        }

        // calculate the pucks estimated position
        if (activeBar == "angle")
        {
            transform.localPosition = new Vector3(xBase, yBase, zBase);
            // Spin the object around the target at 20 degrees/second.
            anchorPoint.transform.rotation = Quaternion.Euler(0, 0, (-line.GetValue() + 50f) * angleParameter);
            anchorPoint.transform.localPosition = new Vector3(sideModifier, 7f, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!laserEnabled) { return; } // optimization
        if (collision.gameObject.layer != 3) // ignore center puck collider
        {
            if (!pucksInPath.Contains(collision.gameObject)) { pucksInPath.Add(collision.gameObject); }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!laserEnabled) { return; } // optimization
        if (collision.gameObject.layer != 3) // ignore center puck collider
        {
            pucksInPath.Remove(collision.gameObject);
        }
    }

    public void StartListeners(bool playersPuck)
    {
        if (!playersPuck) { return; } // only laser user

        laserEnabled = true;
        spriteRenderer.enabled = true;
        LeanTween.alpha(gameObject, 1f, 0.5f).setEase(LeanTweenType.easeInQuart);

        StopListeners();
        if (ClientLogicScript.Instance.isRunning) // vs online
        {
            ClientLogicScript.OnPlayerShot += FireLaser;
            ClientLogicScript.OnOpponentShot += FireLaser;
        }
        else if (LogicScript.Instance.gameIsRunning) // vs CPU
        {
            LogicScript.OnPlayerShot += FireLaser;
            LogicScript.OnOpponentShot += FireLaser;
        }
    }

    public void DisableLaser()
    {
        laserEnabled = false;
        spriteRenderer.enabled = false;
    }

    private void StopListeners()
    {
        ClientLogicScript.OnPlayerShot -= FireLaser;
        ClientLogicScript.OnOpponentShot -= FireLaser;
        LogicScript.OnPlayerShot -= FireLaser;
        LogicScript.OnOpponentShot -= FireLaser;
    }

    public void FireLaser()
    {
        // destroy all pucks in path
        List<GameObject> pucksToDestroy = new List<GameObject>(pucksInPath);
        foreach (GameObject puck in pucksToDestroy)
        {
            if (puck != null)
            {
                if (ClientLogicScript.Instance.isRunning)
                {
                    puck.GetComponent<PuckScript>().DestroyPuckServerRpc();
                }
                else
                {
                    puck.GetComponent<PuckScript>().DestroyPuck();
                }
            }
        }
        // turn off the laser
        StopListeners();
        LeanTween.alpha(gameObject, 0f, 0.5f).setEase(LeanTweenType.easeInQuart).setOnComplete(DisableLaser);
    }
}