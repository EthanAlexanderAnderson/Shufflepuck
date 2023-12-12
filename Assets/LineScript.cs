// Script to control the moving line on the angle/power/spin bar

using UnityEngine;

public class LineScript : MonoBehaviour
{
    LogicScript logic;
    [SerializeField] SpriteRenderer spriteRenderer;

    bool movingLeft;
    float moveSpeed;

    public float value;
    public bool isActive;

    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = new Vector3(Random.Range(-10.0f, 10.0f), 0, -2);
        logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();
    }

    // Update is called once per frame
    void Update()
    {
        // change line speed based on match difficulty
        if (logic.difficulty == 2 && !logic.IsLocal && !logic.IsOnline)
        {
            moveSpeed = 20;
        }
        else
        {
            moveSpeed = 10;
        }
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
            } else if (transform.position.x < -10)
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
}
