using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;
using System;

public class PlinkoDropButtonScript : MonoBehaviour
{
    // imports
    public Button dropButton;
    public Button backButton;
    public GameObject puck;
    [SerializeField] private TMP_Text count;
    [SerializeField] private GameObject floatingTextPrefab;

    // local variables
    private float cooldown = -2f;
    int dropped = 0;
    int drops = 0;
    int previousDropsValue = -1;
    [SerializeField] float floatingTextLocation = 3.5f;
    int playerXPonEnable = 0;
    private float xAxisRandomRange;

    // experimental controlling plinko drop point feature
    bool experimentalFeaturesEnabled = false;
    [SerializeField] private GameObject dropPoint;
    bool right = true;
    [SerializeField] private int force;

    private void OnEnable()
    {
        previousDropsValue = -1;
        SetDropButtonText();
        (playerXPonEnable, _) = LevelManager.Instance.GetXPAndLevel();
        PuckManager.Instance.ClearAllPucks();

        experimentalFeaturesEnabled = PlayerPrefs.GetInt("experimental") == 1;

        dropPoint.SetActive(experimentalFeaturesEnabled);
    }

    // Update is called once per frame
    void Update()
    {
        backButton.interactable = true;
        // move all pucks slightly to the left to prevent stuckage
        GameObject[] pucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (GameObject puck in pucks)
        {
            Rigidbody2D rb = puck.GetComponent<Rigidbody2D>();

            // teleport fully stopped pucks
            if (rb.linearVelocity.magnitude <= 0.0000001f && puck.transform.position.y < 7.9f)
            {
                puck.transform.position = new Vector3(puck.transform.position.x, 8.4f, puck.transform.position.z);
            }
            // slightly move all slowed pucks
            else if (rb.linearVelocity.magnitude <= 0.05f && puck.transform.position.y < 7.9f)
            {
                puck.transform.position = new Vector3(puck.transform.position.x - 0.001f, puck.transform.position.y, puck.transform.position.z);
            }

            // if any puck is off screen, destroy it
            if (puck.transform.position.y < -20f)
            {
                Destroy(puck);
                SetDropButtonText();
            }

            // if any single dropped puck exists (if this loop and if statement is entered at least once) then the back button should be disabled
            if (rb.gravityScale > 0)
            {
                backButton.interactable = false;
            }
        }
        // if cooldown is active, disable button
        dropButton.interactable = ((Time.time - cooldown) > 2 && drops > 0);


        // experimental controlling plinko drop point feature
        if (experimentalFeaturesEnabled)
        {
            if (dropPoint.transform.position.x < -10)
            {
                LeanTween.moveX(dropPoint, 10.01f, 2f).setEase(LeanTweenType.easeInOutQuad);
                right = true;
            }
            else if (dropPoint.transform.position.x > 10)
            {
                LeanTween.moveX(dropPoint, -10.01f, 2f).setEase(LeanTweenType.easeInOutQuad);
                right = false;
            }
        }

    }

    public void SetDropButtonText()
    {
        (_, int level) = LevelManager.Instance.GetXPAndLevel();
        dropped = PlayerPrefs.GetInt("PlinkoPegsDropped", 0);
        drops = level - dropped;
        count.text = drops.ToString();
        // if we gain a drop while plinko screen is active OTHER THAN when it's enabled, show +1 floating text visual feedback
        if (previousDropsValue >= 0 && drops > previousDropsValue)
        {
            var floatingText = Instantiate(floatingTextPrefab, transform.position + new Vector3(floatingTextLocation, 0, 0), Quaternion.identity, transform);
            floatingText.GetComponent<FloatingTextScript>().Initialize("+" + (drops - previousDropsValue).ToString(), 0.25f, 25);
            PlinkoManager.Instance.UpdateSideBucketText();
        }
        previousDropsValue = drops;
    }

    public void Drop()
    {
#if !UNITY_EDITOR
        // 1 second cooldown on clicking the button
        if (Time.time - cooldown < 2 || drops <= 0)
        {
            return;
        }
        cooldown = Time.time;
# else
        PlinkoManager.Instance.IncrementDropped(0);
#endif

        xAxisRandomRange = (playerXPonEnable >= 100 && playerXPonEnable < 460) ? 3f : 7f; // Greater than level 1 (gets first drop) and less than level 4. Thus boosted odds for first 3 drops.

        float xPos = experimentalFeaturesEnabled ? dropPoint.transform.position.x : Random.Range(-xAxisRandomRange, xAxisRandomRange);
        GameObject puckObject = Instantiate(puck, new Vector3(xPos, 8.4f, 0.0f), Quaternion.identity);
        PuckScript puckScript = puckObject.GetComponent<PuckScript>();
        puckScript.InitPuck(true, LogicScript.Instance.player.puckSpriteID);
        // set gravity scale to 1 on puckObject
        puckObject.GetComponent<Rigidbody2D>().gravityScale = 1;
        // set order in layer to 3
        puckObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
        // diable the child object circle collider 2d (this prevents point sfx)
        puckObject.transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = false;

        if (experimentalFeaturesEnabled)
        {
            int momentumForce = (right ? force : -force) + Random.Range(-(force / 2), force / 2);
            puckObject.GetComponent<Rigidbody2D>().AddForceX(momentumForce);
        }

        PlayerPrefs.SetInt("PlinkoPegsDropped", PlayerPrefs.GetInt("PlinkoPegsDropped") + 1);
        dropped += 1;
        SetDropButtonText();
    }
}
