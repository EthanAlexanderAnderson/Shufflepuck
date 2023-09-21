using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckScript : MonoBehaviour
{
    // puck object components
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public AudioSource bonkHeavySFX;
    public AudioSource bonkLightSFX;
    public AudioSource noiseSFX;
    public AudioSource shotSFX;
    public AudioSource pointPlayerSFX;
    public AudioSource pointCPUSFX;

    // script variables ( this are public only for debugging, changing power/angle does nothing)

    private bool shot;
    private bool safe;
    private bool pastSafeLine;
    private bool playersPuck;
    private float velocity;

    // this one actually does stuff
    public float powerModifier = 10;

    public float angularVelocity;
    public float angularVelocityModifier;
    public float counterForce;

    // scoring
    private int puckBaseValue = 1;
    private int zoneMultiplier = 0;

    // ---------- DEFAULT UNITY FUNCTIONS ----------
    void Start()
    {

    }

    void Update()
    {
        velocity = rb.velocity.x + rb.velocity.y;
        noiseSFX.volume = velocity / 25.0f;

        if (isSafe())
        {
            pastSafeLine = true;
        }

        angularVelocity = rb.angularVelocity;
        var right = transform.InverseTransformDirection(transform.right);
        var up = transform.InverseTransformDirection(transform.up);
        if (!isSlowed() && isShot() && pastSafeLine)
        {
            rb.AddForce((right * angularVelocity * angularVelocityModifier) * -0.03f);
            // counter force down
            if (angularVelocity > 0)
            {
                rb.AddForce((up * angularVelocity * angularVelocityModifier * counterForce) * -0.03f);
            }
            else
            {
                rb.AddForce((up * angularVelocity * angularVelocityModifier * counterForce) * 0.03f);
            }


            //oldVelocity = velocity;
        }


    }

    // ---------- ALTER PUCK ----------
    public PuckScript initPuck(bool isPlayersPuckParameter, Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
        playersPuck = isPlayersPuckParameter;
        return this;
    }

    private float angle;
    private float power;
    private float spin;
    public void shoot(float angleParameter, float powerParameter, float spinParameter = 50)
    {
        // normalize power and then scale
        power = (powerParameter - (powerParameter - 50) * 0.5f) * powerModifier;
        // convert angle from 0-100 range to 60-120 range
        angle = (float)((-angleParameter * 0.6) + 120);
        float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * power;
        float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * power;
        Vector2 vector = new Vector2(xcomponent, ycomponent);
        // adjust spin
        spin = (spinParameter - 50) * -10;
        // shoot
        rb.AddForce(vector);
        shot = true;
        // add spin
        rb.AddTorque(spin);
        // SFX
        shotSFX.Play();
    }

    // ---------- GETTERS AND SETTERS ----------
    // y > -9 is not ideal code quality wise, but it keeps triggering isSlowed true just after the puck is shot
    public bool isSlowed() { return rb.velocity.x < 1 && rb.velocity.y < 1 && isShot() && transform.position.y > -9; }
    public bool isStopped() { return rb.velocity.x < 0.1 && rb.velocity.y < 0.1 && isShot() && transform.position.y > -9; }
    public bool isShot() { return shot; }
    public bool isSafe() { return safe; }
    public bool isPastSafeLine() { return pastSafeLine; }
    public bool isPlayersPuck() { return playersPuck; }
    public int computeValue() { return puckBaseValue * zoneMultiplier; }
    public int getZoneMultiplier() { return zoneMultiplier; }
    public void setZoneMultiplier(int ZM) { zoneMultiplier = ZM; }

    public void enterScoreZone( bool isZoneSafe, int enteredZoneMultiplier)
    {
        // all zones are past safe line, so pastSafeLine can be set to true permanently
        pastSafeLine = true;
        safe = isZoneSafe;
        if (enteredZoneMultiplier > zoneMultiplier || enteredZoneMultiplier == 0)
        {
            // if put moves into higher scoring zone and gains a point play SFX
            if (enteredZoneMultiplier > zoneMultiplier && !isStopped())
            {
                if (isPlayersPuck())
                {
                    pointPlayerSFX.Play();
                }
                else
                {
                    pointCPUSFX.Play();
                }
            }
            zoneMultiplier = enteredZoneMultiplier;
        }
    }
    public void exitScoreZone(bool isBoundary, int exitedZoneMultiplier)
    {
        // if you exit a boundry, the puck is no longer safe
        if (isBoundary)
        {
            safe = false;
        }
        // decrement score unless exiting a zero zone
        if (zoneMultiplier > 0)
        {
            zoneMultiplier = exitedZoneMultiplier - 1;
        }
    }


    // ---------- OTHER ----------
    public string toString()
    {
        return (
            "\n isPlayersPuck: " + isPlayersPuck() +
            "\n puckBaseValue: " + puckBaseValue +
            "\n zoneMultiplier: " + zoneMultiplier +
            "\n isShot: " + isShot() +
            "\n isSafe: " + isSafe() +
            "\n isStopped: " + isStopped()
            ); 
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (rb.velocity.x + rb.velocity.y > 0.3)
        {
            //Debug.Log("collide");
            if (rb.velocity.x + rb.velocity.y > 1)
            {
                bonkHeavySFX.Play();
            }
            else
            {
                bonkLightSFX.Play();
            }
            
        }
        angularVelocityModifier = 0;
    }
}
