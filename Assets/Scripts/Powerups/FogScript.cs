using UnityEngine;

public class FogScript : MonoBehaviour
{
    // self
    public static FogScript Instance;

    private bool fogEnabled = false;
    private float moveSpeed = 0.01f;  // Speed of movement
    private float fadeSpeed = 0.01f;  // Speed of fade in/out
    private bool movingRight = true;  // Direction flag
    private SpriteRenderer sr;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        // change opacity
        if (fogEnabled && sr.color.a < 1)
        {
            sr.color = new Color(1, 1, 1, sr.color.a + fadeSpeed);
        }
        else if (!fogEnabled && sr.color.a > 0)
        {
            sr.color = new Color(1, 1, 1, sr.color.a - fadeSpeed);
        }

        if (!fogEnabled) { return; }

        // Check the current direction and move the object
        if (movingRight)
        {
            transform.position += moveSpeed * Vector3.right;
            if (transform.position.x >= 38)
            {
                movingRight = false;  // Change direction to left
            }
        }
        else
        {
            transform.position += moveSpeed * Vector3.left;
            if (transform.position.x <= -38)
            {
                movingRight = true;  // Change direction to right
            }
        }
    }

    private void EnableFog()
    {
        fogEnabled = true;
    }

    public void DisableFog()
    {
        fogEnabled = false;
        StopListeners();
    }

    public void StartListeners(bool playersPuck)
    {
        StopListeners();
        if (ClientLogicScript.Instance.isRunning) // lock online
        {
            if (playersPuck)
            {
                ClientLogicScript.OnPlayerShot += EnableFog;
                ClientLogicScript.OnOpponentShot += DisableFog;
            }
            else
            {
                ClientLogicScript.OnOpponentShot += EnableFog;
                ClientLogicScript.OnPlayerShot += DisableFog;
            }
        }
        else if (LogicScript.Instance.gameIsRunning) // lock vs CPU
        {
            if (playersPuck)
            {
                LogicScript.OnPlayerShot += EnableFog;
                LogicScript.OnOpponentShot += DisableFog;
            }
            else
            {
                LogicScript.OnOpponentShot += EnableFog;
                LogicScript.OnPlayerShot += DisableFog;
            }
        }
    }

    private void StopListeners()
    {
        ClientLogicScript.OnPlayerShot -= EnableFog;
        ClientLogicScript.OnPlayerShot -= DisableFog;
        ClientLogicScript.OnOpponentShot -= EnableFog;
        ClientLogicScript.OnOpponentShot -= DisableFog;
        LogicScript.OnPlayerShot -= EnableFog;
        LogicScript.OnPlayerShot -= DisableFog;
        LogicScript.OnOpponentShot -= EnableFog;
        LogicScript.OnOpponentShot -= DisableFog;
    }
}
