/* Each puck is assigned one of
 * these scripts when it generates.
 */

using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PuckScript : NetworkBehaviour
{
    // puck object components
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AudioSource bonkHeavySFX;
    [SerializeField] private AudioSource bonkLightSFX;
    [SerializeField] private AudioSource noiseSFX;
    [SerializeField] private AudioSource shotSFX;
    [SerializeField] private AudioSource pointPlayerSFX;
    [SerializeField] private AudioSource pointCPUSFX;
    [SerializeField] private AudioSource minusPlayerSFX;
    [SerializeField] private AudioSource minusCPUSFX;

    // script variables
    private bool shot;
    private bool safe;
    private bool pastSafeLine;
    private bool playersPuck;
    private float velocity;

    // this one actually does stuff
    [SerializeField] private float powerModifier;

    [SerializeField] private float angularVelocity;
    [SerializeField] private float angularVelocityModifier;
    [SerializeField] private float counterForce;

    // scoring
    private int puckBaseValue = 1;
    private int puckBonusValue = 0;
    private int zoneMultiplier = 0;

    // floating text
    [SerializeField] private GameObject floatingTextPrefab;

    private LogicScript logic;
    private PuckSkinManager puckSkinManager;

    private float SFXvolume;

    private TrailRenderer trail;

    void OnEnable()
    {
        logic = LogicScript.Instance;
        SFXvolume = SoundManagerScript.Instance.GetSFXVolume();
        puckSkinManager = PuckSkinManager.Instance;
        trail = gameObject.GetComponent<TrailRenderer>();
    }

    private Vector2 shotForceToAdd;
    private float shotTorqueToAdd;
    void FixedUpdate()
    {
        // if shot, add the force / torque
        if (IsShot() && shotForceToAdd != Vector2.zero)
        {
            rb.AddForce(shotForceToAdd);
            rb.AddTorque(shotTorqueToAdd);
            shotForceToAdd = Vector2.zero;
            shotTorqueToAdd = 0.0f;
        }

        // change the sliding SFX volume based on velocity
        velocity = rb.velocity.x + rb.velocity.y;
        noiseSFX.volume = (velocity / 20.0f) * SFXvolume;

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

        Sprite puckSprite = puckSkinManager.ColorIDtoPuckSprite(puckSpriteID * swapAlt);
        spriteRenderer.sprite = puckSprite;
        playersPuck = IsPlayersPuckParameter;

        Debug.Log($"Puck initialized. IsPlayersPuckParameter: {IsPlayersPuckParameter}. PuckSpriteID: {puckSpriteID}");
    }

    private float angle;
    private float power;
    private float spin;
    public void Shoot(float angleParameter, float powerParameter, float spinParameter = 50)
    {
        if ( angleParameter < -5 || angleParameter > 105 || powerParameter < -5 || powerParameter > 105 || spinParameter < -5 || spinParameter > 105 )
        {
            Debug.LogError("Invalid input: One or more parameters are out of the valid range (-5 to 105).");
        }
        // give high power shots an extra oomf
        var volumeBoost = 0f;
        if (powerParameter >= 95) { 
            powerParameter += (powerParameter - 95) * powerModifier + 10;
            trail.startColor = new Color(1, 0.8f, 0);
            trail.endColor = Color.yellow;
            volumeBoost += 0.2f;
        }
        // normalize power and then scale
        power = (powerParameter - (powerParameter - 50) * 0.5f) * powerModifier;
        // convert angle from 0-100 range to 60-120 range
        angle = (float)((-angleParameter * 0.6) + 120);
        float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * power;
        float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * power;
        Vector2 vector = new(xcomponent, ycomponent);
        // adjust spin
        spin = (spinParameter - 50) * -10;
        // Shoot
        shotForceToAdd = vector;
        shotTorqueToAdd = spin;
        shot = true;
        // SFX
        shotSFX.volume += volumeBoost;
        shotSFX.volume *= SFXvolume;
        shotSFX.Play();
    }

    [ServerRpc]
    public void ShootServerRpc(float angleParameter, float powerParameter, float spinParameter = 50)
    {
        Shoot(angleParameter, powerParameter, spinParameter);
    }

    // ---------- GETTERS AND SETTERS ----------
    public bool IsSlowed() { return rb.velocity.x < 2 && rb.velocity.y < 2 && IsShot() && transform.position.y > -9; }
    public bool IsSlowedMore() { return rb.velocity.x < 0.4 && rb.velocity.y < 0.4 && IsShot() && transform.position.y > -9; }
    public bool IsStopped() { return rb.velocity.x < 0.1 && rb.velocity.y < 0.1 && IsShot() && transform.position.y > -9; }
    public bool IsShot() { return shot; }
    public bool IsSafe() { return safe; }
    public bool IsPastSafeLine() { return pastSafeLine; }
    public bool IsPlayersPuck() { return playersPuck; }
    public int ComputeValue() { return (puckBaseValue * zoneMultiplier) + puckBonusValue; }
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
                    pointPlayerSFX.volume = SFXvolume;
                    pointPlayerSFX.Play();
                }
                else
                {
                    pointCPUSFX.volume = SFXvolume;
                    pointCPUSFX.Play();
                }
                // if this puck object already has a floating text, destroy it
                foreach (Transform child in transform)
                {
                    if (child.gameObject.tag == "floatingText")
                    {
                        Destroy(child.gameObject);
                    }
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
                    minusPlayerSFX.volume = SFXvolume;
                    minusPlayerSFX.Play();
                }
                else
                {
                    minusCPUSFX.volume = SFXvolume;
                    minusCPUSFX.Play();
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
    public override string ToString()
    {
        return (
            "\n IsPlayersPuck: " + IsPlayersPuck() +
            "\n puckBaseValue: " + puckBaseValue +
            "\n puckBonusValue: " + puckBonusValue +
            "\n zoneMultiplier: " + zoneMultiplier +
            "\n IsShot: " + IsShot() +
            "\n IsSafe: " + IsSafe() +
            "\n IsStopped: " + IsStopped()
            ); 
    }

    [SerializeField] private ParticleSystem collisionParticleEffectPrefab;
 
    // play bonk SFX when pucks collide
    void OnCollisionEnter2D(Collision2D col)
    {
        if (rb.velocity.x + rb.velocity.y > 0.3f)
        {
            if (rb.velocity.x + rb.velocity.y > 1f)
            {
                bonkHeavySFX.volume *= SFXvolume;
                bonkHeavySFX.Play();
            }
            else
            {
                bonkLightSFX.volume *= SFXvolume;
                bonkLightSFX.Play();
            }
            // play collision particle effect
            ParticleSystem collisionParticleEffect = Instantiate(collisionParticleEffectPrefab, col.GetContact(0).point, Quaternion.identity);
            collisionParticleEffect.transform.position = col.GetContact(0).point;
            ParticleSystem.EmissionModule emission = collisionParticleEffect.emission;
            emission.rateOverTime = (rb.velocity.x + rb.velocity.y) * 100f;
            ParticleSystem.MainModule main = collisionParticleEffect.main;
            main.startSpeed = (rb.velocity.x + rb.velocity.y) * 4f;
            collisionParticleEffect.Play();
            Destroy(collisionParticleEffect.gameObject, 5f);
        }
        angularVelocityModifier = 0;
    }

    // base value is multplied by score zone value
    public void SetPuckBaseValue(int value)
    {
        puckBaseValue = value;
    }

    // bonus value is added onto base value * score zone value
    public void SetPuckBonusValue(int value)
    {
        puckBonusValue = value;
    }
}
