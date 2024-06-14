/* Each puck is assigned one of
 * these scripts when it generates.
 */

using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PuckScript : NetworkBehaviour
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
    public AudioSource minusPlayerSFX;
    public AudioSource minusCPUSFX;

    // script variables
    private bool shot;
    private bool safe;
    private bool pastSafeLine;
    private bool playersPuck;
    private float velocity;

    // this one actually does stuff
    [SerializeField] float powerModifier;

    public float angularVelocity;
    public float angularVelocityModifier;
    public float counterForce;

    // scoring
    private int puckBaseValue = 1;
    private int zoneMultiplier = 0;

    // floating text
    public GameObject floatingTextPrefab;

    private LogicScript logic;

    void OnEnable()
    {
        logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();
    }

    void FixedUpdate()
    {
        // change the sliding SFX volume based on velocity
        velocity = rb.velocity.x + rb.velocity.y;
        noiseSFX.volume = (velocity / 20.0f) * logic.GetSFX();

        if (IsSafe())
        {
            pastSafeLine = true;
        }

        // handle spinning forces
        angularVelocity = rb.angularVelocity;
        var right = transform.InverseTransformDirection(transform.right);
        var up = transform.InverseTransformDirection(transform.up);
        if (!IsSlowed() && IsShot() && pastSafeLine)
        {
            // add horizontal spinning force
            rb.AddForce((right * angularVelocity * angularVelocityModifier) * -0.03f);

            // add counter force downwards
            if (angularVelocity > 0)
            {
                rb.AddForce((up * angularVelocity * angularVelocityModifier * counterForce) * -0.03f);
            }
            else
            {
                rb.AddForce((up * angularVelocity * angularVelocityModifier * counterForce) * 0.03f);
            }
        }
    }

    // initiate a new puck
    public PuckScript InitPuck(bool IsPlayersPuckParameter, Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
        playersPuck = IsPlayersPuckParameter;
        return this;
    }

    [ClientRpc]
    public void InitPuckClientRpc(bool IsPlayersPuckParameter, int puckSpriteID, ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;

        var swapAlt = !IsPlayersPuckParameter && puckSpriteID == logic.player.puckSpriteID ? -1 : 1;

        Sprite puckSprite = logic.ColorIDtoPuckSprite(puckSpriteID * swapAlt);
        spriteRenderer.sprite = puckSprite;
        playersPuck = IsPlayersPuckParameter;

        Debug.Log($"Puck initialized. IsPlayersPuckParameter: {IsPlayersPuckParameter}. PuckSpriteID: {puckSpriteID}");
    }

    private float angle;
    private float power;
    private float spin;
    public void Shoot(float angleParameter, float powerParameter, float spinParameter = 50)
    {
        // give high power shots an extra oomf
        var volumeBoost = 0f;
        if (powerParameter >= 95) { 
            powerParameter += (powerParameter - 95) * powerModifier + 10;
            gameObject.GetComponent<TrailRenderer>().startColor = new Color(1, 0.8f, 0);
            gameObject.GetComponent<TrailRenderer>().endColor = Color.yellow;
            volumeBoost += 0.2f;
        }
        // normalize power and then scale
        power = (powerParameter - (powerParameter - 50) * 0.5f) * powerModifier;
        // convert angle from 0-100 range to 60-120 range
        angle = (float)((-angleParameter * 0.6) + 120);
        float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * power;
        float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * power;
        Vector2 vector = new Vector2(xcomponent, ycomponent);
        // adjust spin
        spin = (spinParameter - 50) * -10;
        // Shoot
        rb.AddForce(vector);
        shot = true;
        // add spin
        rb.AddTorque(spin);
        // SFX
        shotSFX.volume += volumeBoost;
        shotSFX.volume *= logic.GetSFX();
        Debug.Log(shotSFX.volume);
        shotSFX.Play();
    }

    [ServerRpc]
    public void ShootServerRpc(float angleParameter, float powerParameter, float spinParameter = 50)
    {
        Shoot(angleParameter, powerParameter, spinParameter);
    }

    // ---------- GETTERS AND SETTERS ----------
    public bool IsSlowed() { return rb.velocity.x < 1 && rb.velocity.y < 1 && IsShot() && transform.position.y > -9; }
    public bool IsSlowedMore() { return rb.velocity.x < 0.4 && rb.velocity.y < 0.4 && IsShot() && transform.position.y > -9; }
    public bool IsStopped() { return rb.velocity.x < 0.1 && rb.velocity.y < 0.1 && IsShot() && transform.position.y > -9; }
    public bool IsShot() { return shot; }
    public bool IsSafe() { return safe; }
    public bool IsPastSafeLine() { return pastSafeLine; }
    public bool IsPlayersPuck() { return playersPuck; }
    public int ComputeValue() { return puckBaseValue * zoneMultiplier; }
    public int GetZoneMultiplier() { return zoneMultiplier; }
    public void SetZoneMultiplier(int ZM) { zoneMultiplier = ZM; }

    // when a puck enters a scoring zone, update its score and play a SFX
    public void EnterScoreZone( bool isZoneSafe, int enteredZoneMultiplier)
    {
        // all zones are past safe line, so pastSafeLine can be set to true permanently
        pastSafeLine = true;
        safe = isZoneSafe;
        if (enteredZoneMultiplier > zoneMultiplier || enteredZoneMultiplier == 0)
        {
            // if puck moves into higher scoring zone and gains a point play SFX
            if (enteredZoneMultiplier > zoneMultiplier && !IsStopped())
            {
                if (IsPlayersPuck())
                {
                    pointPlayerSFX.volume *= logic.GetSFX();
                    pointPlayerSFX.Play();
                }
                else
                {
                    pointCPUSFX.volume *= logic.GetSFX();
                    pointCPUSFX.Play();
                }
                // show floating text
                var floatingText = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
                floatingText.GetComponent<TMP_Text>().text = enteredZoneMultiplier.ToString();
            }
            // if puck moves into the off zone, play minus sfx
            else if (enteredZoneMultiplier < zoneMultiplier && !IsStopped())
            {
                if (IsPlayersPuck())
                {
                    minusCPUSFX.volume *= logic.GetSFX();
                    minusCPUSFX.Play();
                }
                else
                {
                    minusPlayerSFX.volume *= logic.GetSFX();
                    minusPlayerSFX.Play();
                }
            }
            
            zoneMultiplier = enteredZoneMultiplier;
        }
    }

    // when a puck exits a scoring zone
    public void ExitScoreZone(bool isBoundary, int exitedZoneMultiplier)
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
            "\n IsPlayersPuck: " + IsPlayersPuck() +
            "\n puckBaseValue: " + puckBaseValue +
            "\n zoneMultiplier: " + zoneMultiplier +
            "\n IsShot: " + IsShot() +
            "\n IsSafe: " + IsSafe() +
            "\n IsStopped: " + IsStopped()
            ); 
    }

    // play bonk SFX when pucks collide
    void OnCollisionEnter2D(Collision2D col)
    {
        if (rb.velocity.x + rb.velocity.y > 0.3)
        {
            if (rb.velocity.x + rb.velocity.y > 1)
            {
                bonkHeavySFX.volume *= logic.GetSFX();
                bonkHeavySFX.Play();
            }
            else
            {
                bonkLightSFX.volume *= logic.GetSFX();
                bonkLightSFX.Play();
            }
            
        }
        angularVelocityModifier = 0;
    }
}
