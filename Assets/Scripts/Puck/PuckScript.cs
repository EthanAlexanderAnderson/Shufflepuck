/* Each puck is assigned one of
 * these scripts when it generates.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using TMPro;
using System;

public class PuckScript : NetworkBehaviour, IPointerClickHandler
{
    // dependacies
    private LogicScript logic;
    private PuckSkinManager puckSkinManager;

    // puck object components
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D puckCollider;
    [SerializeField] private AudioSource bonkHeavySFX;
    [SerializeField] private AudioSource bonkLightSFX;
    [SerializeField] private AudioSource noiseSFX;
    [SerializeField] private AudioSource shotSFX;
    [SerializeField] private AudioSource pointPlayerSFX;
    [SerializeField] private AudioSource pointCPUSFX;
    [SerializeField] private AudioSource minusPlayerSFX;
    [SerializeField] private AudioSource minusCPUSFX;
    [SerializeField] private GameObject animationLayer;
    [SerializeField] private TrailRenderer trail;

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
    private string powerupText;

    // sound effect volume
    private float SFXvolume;

    // particle colors
    private Color[] color = {new Color(0.5f, 0.5f, 0.5f)};

    // for powerups
    [SerializeField] private bool phase = false;

    void OnEnable()
    {
        logic = LogicScript.Instance;
        SFXvolume = SoundManagerScript.Instance.GetSFXVolume();
        puckSkinManager = PuckSkinManager.Instance;
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
            if (phase)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                puckCollider.isTrigger = true;
            }
        }

        // Calculate the magnitude of the velocity vector to determine the sliding noise volume
        velocity = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.y * rb.velocity.y);
        noiseSFX.volume = logic.gameIsRunning ? (velocity / 15.0f) * SFXvolume : 0f; // only play noise if game is running (not plinko)

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

        // for phase powerup
        if (phase && IsShot() && pastSafeLine && IsStopped()) // is slowed & shot & past safeline
        {
            // if this puck is within 2 units of the nearest puck, destroy it
            var pucks = GameObject.FindGameObjectsWithTag("puck");
            foreach (var puck in pucks)
            {
                if (puck != gameObject && Vector2.Distance(puck.transform.position, transform.position) < 2)
                {
                    DestroyPuck();
                    return;
                }
            }
            // othewrise, unphase it (make it visible again)
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            puckCollider.isTrigger = false;
            phase = false;
        }
    }

    // initiate a new puck
    public PuckScript InitPuck(bool IsPlayersPuckParameter, int puckSpriteID)
    {
        spriteRenderer.sprite = puckSkinManager.ColorIDtoPuckSprite(puckSpriteID);
        color = puckSkinManager.ColorIDtoColor(puckSpriteID);
        playersPuck = IsPlayersPuckParameter;
        // enable animation for atom
        animationLayer.SetActive(Math.Abs(puckSpriteID) == 40);
        return this;
    }

    [ClientRpc]
    public void InitPuckClientRpc(bool IsPlayersPuckParameter, int puckSpriteID, ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;

        var swapAlt = !IsPlayersPuckParameter && puckSpriteID == logic.player.puckSpriteID ? -1 : 1;

        Sprite puckSprite = puckSkinManager.ColorIDtoPuckSprite(puckSpriteID * swapAlt);
        spriteRenderer.sprite = puckSprite;
        color = puckSkinManager.ColorIDtoColor(puckSpriteID * swapAlt);
        playersPuck = IsPlayersPuckParameter;
        // enable animation for atom
        animationLayer.SetActive(Math.Abs(puckSpriteID) == 40);

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
    public int ComputeValue() { return (puckBaseValue * zoneMultiplier) + (zoneMultiplier > 0 ? puckBonusValue : 0); }
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
                zoneMultiplier = enteredZoneMultiplier;
                // show floating text
                var floatingText = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
                floatingText.GetComponent<TMP_Text>().text = ComputeValue().ToString();
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
                zoneMultiplier = enteredZoneMultiplier;
            }
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
        // Calculate the magnitude of the velocity vector to determine the bonk volume
        velocity = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.y * rb.velocity.y);

        if (velocity > 0.5f)
        {
            if (velocity > 3f)
            {
                bonkHeavySFX.volume = SFXvolume;
                bonkHeavySFX.Play();
            }
            else
            {
                bonkLightSFX.volume = (SFXvolume * velocity) / 3f;
                bonkLightSFX.Play();
            }
            // play collision particle effect
            ParticleSystem collisionParticleEffect = Instantiate(collisionParticleEffectPrefab, col.GetContact(0).point, Quaternion.identity);
            collisionParticleEffect.transform.position = col.GetContact(0).point;
            ParticleSystem.EmissionModule emission = collisionParticleEffect.emission;
            emission.rateOverTime = (velocity) * 50f;
            ParticleSystem.MainModule main = collisionParticleEffect.main;
            main.startSpeed = (velocity) * 4f;

            // set color of particle effect to puck color
            if (color == null || color.Length <= 0) // handle null color
            {
                main.startColor = new ParticleSystem.MinMaxGradient(Color.grey);
            }
            else if (color.Length == 1 ) // handle one color
            {
                main.startColor = color[0];
            }
            else // handle two or more colors
            {
                main.startColor = new ParticleSystem.MinMaxGradient(CreateRandomGradient());
            }

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

    Gradient CreateRandomGradient()
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[color.Length];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[1]; // One alpha key for consistent transparency

        for (int i = 0; i < color.Length; i++)
        {
            // Each color is mapped to a specific point in the gradient
            float time = i / (float)(color.Length - 1);
            colorKeys[i] = new GradientColorKey(color[i], time);
        }

        alphaKeys[0] = new GradientAlphaKey(1f, 0f); // Set constant alpha

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    public void SetPowerupText(string text)
    {
        powerupText = text;
    }

    // text to show what powerup was used under the puck. Only fades, no up/shrink.
    public void CreatePowerupFloatingText()
    {
        var floatingText = Instantiate(floatingTextPrefab, transform.position + new Vector3(0, -1.5f, 0), Quaternion.identity, transform);
        floatingText.GetComponent<FloatingTextScript>().Initialize(powerupText, 0, 0, 0.1f, 1, true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // show puck score when clicked
        var floatingText = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
        floatingText.GetComponent<TMP_Text>().text = ComputeValue().ToString();
        // if powerupText has been set, show it
        if (powerupText != null)
        {
            CreatePowerupFloatingText();
        }

    }

    public void DestroyPuck() // Destroys the puck with a particle and sound effect
    {
        ParticleSystem collisionParticleEffect = Instantiate(collisionParticleEffectPrefab, transform.position, Quaternion.identity);
        ParticleSystem.EmissionModule emission = collisionParticleEffect.emission;
        emission.rateOverTime = 1000f;
        ParticleSystem.MainModule main = collisionParticleEffect.main;
        main.startSpeed = 100f;
        // set color of particle effect to puck color
        if (color == null || color.Length <= 0) // handle null color
        {
            main.startColor = new ParticleSystem.MinMaxGradient(Color.grey);
        }
        else if (color.Length == 1 ) // handle one color
        {
            main.startColor = color[0];
        }
        else // handle two or more colors
        {
            main.startColor = new ParticleSystem.MinMaxGradient(CreateRandomGradient());
        }
        collisionParticleEffect.Play();
        Destroy(collisionParticleEffect.gameObject, 10f);
        Destroy(gameObject, 0.1f);
        logic.playDestroyPuckSFX(SFXvolume);
    }

    public void SetPhase(bool isPhase)
    {
        phase = isPhase;
    }
}
