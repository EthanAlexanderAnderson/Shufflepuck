using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class PlinkoDropButtonScript : MonoBehaviour
{
    //dependacies
    private LevelManager levelManager;
    private PuckSkinManager puckSkinManager;
    private LogicScript logic;

    // imports
    public Button dropButton;
    public Button backButton;
    public GameObject puck;
    [SerializeField] private TMP_Text count;

    // local variables
    private float cooldown = -2f;
    int dropped = 0;
    int drops = 0;

    private void OnEnable()
    {
        levelManager = LevelManager.Instance;
        puckSkinManager = PuckSkinManager.Instance;
        logic = LogicScript.Instance;
        SetDropButtonText();
    }

    // Update is called once per frame
    void Update()
    {
        backButton.interactable = true;
        // move all pucks slightly to the left to prevent stuckage
        GameObject[] pucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (GameObject puck in pucks)
        {
            // slightly move all pucks
            if (puck.GetComponent<Rigidbody2D>().velocity.y <= 0.01f && puck.transform.position.y < 6.9f)
            {
                puck.transform.position = new Vector3(puck.transform.position.x - 0.001f, puck.transform.position.y, puck.transform.position.z);
            }

            // if any puck is off screen, destroy it
            if (puck.transform.position.y < -20f)
            {
                Destroy(puck);
                SetDropButtonText();
            }

            // if any single puck exists (if this loop is entered at least once) then the back button should be disabled
            backButton.interactable = false;
        }
        // if cooldown is active, disable button
        dropButton.interactable = ((Time.time - cooldown) > 2 && drops > 0);
    }

    private void SetDropButtonText()
    {
        (int XP, int level) = levelManager.GetXPAndLevel();
        dropped = PlayerPrefs.GetInt("PlinkoPegsDropped", 0);
        drops = level - dropped;
        count.text = drops.ToString();
    }

    public void Drop()
    {
        // 1 second cooldown on clicking the button
        if (Time.time - cooldown < 2 || drops <= 0)
        {
            return;
        }
        cooldown = Time.time;

        GameObject puckObject = Instantiate(puck, new Vector3(Random.Range(-8f, 7.66f) + 0.333f, 8f, 0.0f), Quaternion.identity);
        // set gravity scale to 1 on puckObject
        puckObject.GetComponent<Rigidbody2D>().gravityScale = 1;
        // set order in layer to 3
        puckObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        // set skin
        Sprite puckSprite = puckSkinManager.ColorIDtoPuckSprite(logic.player.puckSpriteID);
        puckObject.GetComponent<SpriteRenderer>().sprite = puckSprite;
        // diable the child object circle collider 2d (this prevents point sfx)
        puckObject.transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = false;

        PlayerPrefs.SetInt("PlinkoPegsDropped", PlayerPrefs.GetInt("PlinkoPegsDropped") + 1);
        dropped += 1;
        SetDropButtonText();
    }
}