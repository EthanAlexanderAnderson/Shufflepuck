// Script to control the moving line on the angle/power/spin bar

using UnityEngine;

public class LineScript : MonoBehaviour
{
    // self
    public static LineScript Instance;

    [SerializeField] private SpriteRenderer spriteRenderer;

    bool movingLeft;
    float moveSpeed = 10;

    [SerializeField] private float value;
    public bool isActive;

    // Start is called before the first frame update

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    void Start()
    {
        transform.localPosition = new Vector3(Random.Range(-10.0f, 10.0f), 0, -2);
    }

    // Update is called once per frame
    void Update()
    {
        // move the line back and forth
        if (isActive)
        {
            spriteRenderer.enabled = true;
            if (movingLeft)
            {
                transform.position += (Vector3.left * moveSpeed) * Time.deltaTime;
            } 
            else
            {
                transform.position += (Vector3.right * moveSpeed) * Time.deltaTime;
            }

            if (transform.position.x > 10)
            {
                movingLeft = true;
            } 
            else if (transform.position.x < -10)
            {
                movingLeft = false;
            }

            // update the lines value based on its location
            value = (transform.position.x + 10) * 5;
        }
        else
        {
            spriteRenderer.enabled = false;
        }
    }

    internal float GetValue()
    {
        if ( value > 105 )
        {
            return 105;
        }
        else if ( value < -5 )
        {
            return -5;
        }
        else
        {
            return value;
        }
    }

    public void HalfSpeed()
    {
        moveSpeed = 5;
        ClientLogicScript.OnPlayerShot += FullSpeed;
        ClientLogicScript.OnOpponentShot += FullSpeed;
        LogicScript.OnPlayerShot += FullSpeed;
        LogicScript.OnOpponentShot += FullSpeed;
    }

    private void FullSpeed()
    {
        moveSpeed = 10;
    }
}
