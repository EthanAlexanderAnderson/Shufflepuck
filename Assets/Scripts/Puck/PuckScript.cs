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
    public NetworkVariable<float> velocityNetworkedRounded = new NetworkVariable<float>();

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
    private Color[] color = { new Color(0.5f, 0.5f, 0.5f) };

    // for powerups
    [SerializeField] private bool phase = false;
    [SerializeField] private bool lockPowerup = false;
    [SerializeField] private bool explosionPowerup = false;
    [SerializeField] private bool hydraPowerup = false;

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
        }

        // Calculate the magnitude of the velocity vector to determine the sliding noise volume
        velocity = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.y * rb.velocity.y);
        if (IsServer) { velocityNetworkedRounded.Value = velocity; }
        noiseSFX.volume = logic.gameIsRunning ? (velocity / 15.0f) * SFXvolume : 0f; // only play noise if game is running (not plinko)

        // update particle emisson based on velocity
        var PS = GetComponent<ParticleSystem>();
        ParticleSystem.EmissionModule emission = PS.emission;
        emission.rateOverTime = (velocity);
        ParticleSystem.MainModule main = PS.main;

        main.startColor = SetParticleColor();

        // phase powerup
        if (phase && (velocity > 1 || velocityNetworkedRounded.Value > 1.0f || !IsShot()))
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            puckCollider.isTrigger = true;
        }

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

        // for phase & lock powerup
        if (IsShot() && pastSafeLine && IsStopped() && (!ClientLogicScript.Instance.isRunning || velocityNetworkedRounded.Value < 0.05f))
        {
            if (phase)
            {
                // if this puck is within 2 units of the nearest puck, destroy it
                var pucks = GameObject.FindGameObjectsWithTag("puck");
                foreach (var puck in pucks)
                {
                    if (puck != gameObject && Vector2.Distance(puck.transform.position, transform.position) < 2)
                    {
                        if (ClientLogicScript.Instance.isRunning && !IsServer) { break; }
                        DestroyPuck();
                        return;
                    }
                }
                // othewrise, unphase it (make it visible again)
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                puckCollider.isTrigger = false;
                phase = false;
            }

            if (lockPowerup)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                rb.angularVelocity = 0;
                rb.velocity = Vector2.zero;
                lockPowerup = false;
            }
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

        if (transform.position.y > 0) // for block, hydra, any spawned puck above safe line
        {
            shot = true;
            safe = true;
        }

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

        if (transform.position.y < 0f)
        {
            ClientLogicScript.Instance.client.activePuckScript = this;
            ClientLogicScript.Instance.client.activePuckObject = gameObject;
        }
        ClientLogicScript.Instance.client.isPlayer = IsPlayersPuckParameter;

        if (transform.position.y > 0)
        {
            shot = true;
            safe = true;
        }

        Debug.Log($"Puck initialized. IsPlayersPuckParameter: {IsPlayersPuckParameter}. PuckSpriteID: {puckSpriteID}");
    }

    private float angle;
    private float power;
    private float spin;
    public void Shoot(float angleParameter, float powerParameter, float spinParameter = 50)
    {
        if (angleParameter < -5 || angleParameter > 105 || powerParameter < -5 || powerParameter > 105 || spinParameter < -5 || spinParameter > 105)
        {
            Debug.LogError("Invalid input: One or more parameters are out of the valid range (-5 to 105).");
        }
        // give high power shots an extra oomf
        var volumeBoost = 0f;
        if (powerParameter >= 95)
        {
            powerParameter += (powerParameter - 95) * powerModifier + 10;
            if (trail != null)
            {
                trail.startColor = new Color(1, 0.8f, 0);
                trail.endColor = Color.yellow;
            }
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
        // screen shake
        ScreenShake.Instance.Shake(powerParameter / 500);
    }

    [ServerRpc]
    public void ShootServerRpc(float angleParameter, float powerParameter, float spinParameter = 50)
    {
        Shoot(angleParameter, powerParameter, spinParameter);
    }

    // ---------- GETTERS AND SETTERS ----------
    public bool IsSlowed() { return rb.velocity.x < 2 && rb.velocity.y < 2 && IsShot() && transform.position.y > -9; }
    public bool IsSlowedMore() { return rb.velocity.x < 0.4 && rb.velocity.y < 0.4 && IsShot() && transform.position.y > -9; }
    public bool IsStopped() { return rb.velocity.x < 0.05 && rb.velocity.y < 0.05 && IsShot() && transform.position.y > -9; }
    public bool IsShot() { return shot; }
    public bool IsSafe() { return safe; }
    public bool IsPastSafeLine() { return pastSafeLine; }
    public bool IsPlayersPuck() { return playersPuck; }
    public int ComputeValue() { return (puckBaseValue * zoneMultiplier) + (zoneMultiplier > 0 ? puckBonusValue : 0); }
    public int GetZoneMultiplier() { return zoneMultiplier; }
    public void SetZoneMultiplier(int ZM) { zoneMultiplier = ZM; }
    public bool IsLocked() { return lockPowerup; }
    public bool IsExplosion() { return explosionPowerup; }
    public bool IsHydra() { return hydraPowerup; }

    // when a puck enters a scoring zone, update its score and play a SFX
    public void EnterScoreZone(bool isZoneSafe, int enteredZoneMultiplier)
    {
        // all zones are past safe line, so pastSafeLine can be set to true permanently
        pastSafeLine = true;
        safe = isZoneSafe;
        shot = true;

        // if puck moves into higher scoring zone and gains a point play SFX
        if (enteredZoneMultiplier > zoneMultiplier)
        {
            if (IsPlayersPuck())
            {
                pointPlayerSFX.volume = SFXvolume;
                pointPlayerSFX.pitch = 0.9f + (0.05f * enteredZoneMultiplier);
                pointPlayerSFX.Play();
            }
            else
            {
                pointCPUSFX.volume = SFXvolume;
                pointCPUSFX.pitch = 0.8f + (0.05f * enteredZoneMultiplier);
                pointCPUSFX.Play();
            }
        }
        // if puck moves into the off zone, play minus sfx
        else if (enteredZoneMultiplier < zoneMultiplier)
        {
            if (IsPlayersPuck())
            {
                minusPlayerSFX.volume = SFXvolume;
                minusPlayerSFX.pitch = 0.9f + (0.05f * enteredZoneMultiplier);
                minusPlayerSFX.Play();
            }
            else
            {
                minusCPUSFX.volume = SFXvolume;
                minusPlayerSFX.pitch = 0.8f + (0.05f * enteredZoneMultiplier);
                minusCPUSFX.Play();
            }
        }

        if (zoneMultiplier != enteredZoneMultiplier)
        {
            // if this puck object already has a floating text, destroy it
            foreach (Transform child in transform)
            {
                if (child.gameObject.CompareTag("floatingText"))
                {
                    Destroy(child.gameObject);
                }
            }

            zoneMultiplier = enteredZoneMultiplier;
            // show floating text
            var floatingText = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
            floatingText.GetComponent<FloatingTextScript>().Initialize(ComputeValue().ToString(), 1, 1, 1, 1.5f + (ComputeValue() / 10), true);
        }
        zoneMultiplier = enteredZoneMultiplier;
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
        if (zoneMultiplier > 0 && exitedZoneMultiplier == zoneMultiplier)
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
            emission.rateOverTime = (velocity) * 75f;
            ParticleSystem.MainModule main = collisionParticleEffect.main;
            main.startSpeed = (velocity) * 5f;

            main.startColor = SetParticleColor();

            collisionParticleEffect.Play();
            Destroy(collisionParticleEffect.gameObject, 5f);

            // Screen shake
            if (col.gameObject.CompareTag("puck"))
            {
                ScreenShake.Instance.Shake(velocity / 20);
            }
        }
        angularVelocityModifier = 0;

        // explosion powerup
        if (explosionPowerup && col.gameObject.CompareTag("puck"))
        {
            // Destroy the collided object
            col.gameObject.GetComponent<PuckScript>().DestroyPuck();
            // destroy self
            DestroyPuck();
        }
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
        floatingText.GetComponent<FloatingTextScript>().Initialize(ComputeValue().ToString(), 1, 1, 1, 1.5f + (ComputeValue() / 10), true);
        // if powerupText has been set, show it
        if (powerupText != null)
        {
            CreatePowerupFloatingText();
        }
    }

    public void DestroyPuck() // Destroys the puck with a particle and sound effect, used by powerups that say destroy
    {
        // update score
        puckBaseValue = 0;
        puckBonusValue = 0;
        zoneMultiplier = 0;
        LogicScript.Instance.UpdateScores();
        logic.playDestroyPuckSFX(SFXvolume);
        // hydra powerup
        if (hydraPowerup)
        {
            if (ClientLogicScript.Instance.isRunning)
            {
                ServerLogicScript.Instance.HydraServerRpc(playersPuck, transform.position.x, transform.position.y);
            }
            else
            {
                PowerupManager.Instance.HydraHelper(playersPuck, transform.position.x, transform.position.y);
            }
        }
        // Screen shake
        ScreenShake.Instance.Shake(0.25f);
        // actually destroy the gameobject
        Destroy(gameObject);
    }

    override public void OnDestroy()
    {
        if (!logic.gameIsRunning && !ClientLogicScript.Instance.isRunning) { return; }
        logic.playDestroyPuckSFX(SFXvolume); // TODO: investigate if this causes the SFX twice in any scenerio
        ParticleSystem collisionParticleEffect = Instantiate(collisionParticleEffectPrefab, transform.position, Quaternion.identity);
        ParticleSystem.EmissionModule emission = collisionParticleEffect.emission;
        emission.rateOverTime = 1000f;
        ParticleSystem.MainModule main = collisionParticleEffect.main;
        main.startSpeed = 100f;
        main.startColor = SetParticleColor();
        collisionParticleEffect.Play();
        Destroy(collisionParticleEffect.gameObject, 10f);

        // Check if the puck has a TrailRenderer
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            Destroy(trail); // Destroy the TrailRenderer
        }

        // update score
        puckBaseValue = 0;
        puckBonusValue = 0;
        zoneMultiplier = 0;
        LogicScript.Instance.UpdateScores();

        // un sub from events
        LogicScript.OnPlayerShot -= IncrementBonusValue;
        LogicScript.OnOpponentShot -= IncrementBonusValue;
        ClientLogicScript.OnPlayerShot -= IncrementBonusValue;
        ClientLogicScript.OnOpponentShot -= IncrementBonusValue;
        LogicScript.OnPlayerShot -= DisableLock;
        LogicScript.OnOpponentShot -= DisableLock;
        ClientLogicScript.OnPlayerShot -= DisableLock;
        ClientLogicScript.OnOpponentShot -= DisableLock;
    }

    [ServerRpc]
    public void DestroyPuckServerRpc()
    {
        if (!IsServer) return;
        DestroyPuck();
    }

    public void SetPhase(bool isPhase)
    {
        phase = isPhase;
    }

    [ClientRpc]
    public void InitBlockPuckClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;
        SetPuckBaseValue(0);
        SetPowerupText("valueless");
        CreatePowerupFloatingText();
        shot = true;
        safe = true;
    }

    public void EnableGrowth()
    {
        puckBonusValue--; // start one under, because it gets shot directly after this
        if (ClientLogicScript.Instance.isRunning) // growth online
        {
            if (playersPuck)
            {
                ClientLogicScript.OnPlayerShot += IncrementBonusValue;
            }
            else
            {
                ClientLogicScript.OnOpponentShot += IncrementBonusValue;
            }
        }
        else if (LogicScript.Instance.gameIsRunning) // growth vs CPU
        {
            if (playersPuck)
            {
                LogicScript.OnPlayerShot += IncrementBonusValue;
            }
            else
            {
                LogicScript.OnOpponentShot += IncrementBonusValue;
            }
        }
    }

    private void IncrementBonusValue()
    {
        if (this == null) { return; }
        puckBonusValue++;
        if (ComputeValue() == 0) { return; }
        LogicScript.Instance.UpdateScores();
        if (IsPlayersPuck())
        {
            pointPlayerSFX.volume = SFXvolume;
            pointPlayerSFX.pitch = 0.9f + (0.05f * ComputeValue());
            pointPlayerSFX.Play();
        }
        else
        {
            pointCPUSFX.volume = SFXvolume;
            pointCPUSFX.pitch = 0.8f + (0.05f * ComputeValue());
            pointCPUSFX.Play();
        }
        var floatingText = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
        floatingText.GetComponent<FloatingTextScript>().Initialize(ComputeValue().ToString(), 1, 1, 1, 1.5f + (ComputeValue() / 10), true);
    }

    public void EnableLock()
    {
        lockPowerup = true;

        if (ClientLogicScript.Instance.isRunning) // lock online
        {
            if (playersPuck)
            {
                ClientLogicScript.OnPlayerShot += DisableLock;
            }
            else
            {
                ClientLogicScript.OnOpponentShot += DisableLock;
            }
        }
        else if (LogicScript.Instance.gameIsRunning) // lock vs CPU
        {
            if (playersPuck)
            {
                LogicScript.OnPlayerShot += DisableLock;
            }
            else
            {
                LogicScript.OnOpponentShot += DisableLock;
            }
        }
    }

    private void DisableLock()
    {
        if (this == null) { return; }
        if (transform.position.y > -9)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    public void EnableExplosion()
    {
        explosionPowerup = true;
    }

    public void EnableHydra()
    {
        hydraPowerup = true;
    }

    public ParticleSystem.MinMaxGradient SetParticleColor()
    {
        // set color of particle effect to puck color
        if (color == null || color.Length <= 0) // handle null color
        {
            return new ParticleSystem.MinMaxGradient(Color.grey);
        }
        else if (color.Length == 1) // handle one color
        {
            return color[0];
        }
        else // handle two or more colors
        {
            return new ParticleSystem.MinMaxGradient(CreateRandomGradient());
        }
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
}
