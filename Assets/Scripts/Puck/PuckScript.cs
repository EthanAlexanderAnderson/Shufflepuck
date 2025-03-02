/* Each puck is assigned one of
 * these scripts when it generates.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using TMPro;
using System;
using System.Collections.Generic;

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
    [SerializeField] private AudioSource breakSFX;
    [SerializeField] private GameObject animationLayer;
    [SerializeField] private TrailRenderer trail;

    // script variables
    private bool shot;
    private bool safe;
    private bool pastSafeLine;
    private bool playersPuck;
    private bool requestedCleanup;
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
    List<string> powerupText = new List<string>();

    // sound effect volume
    private float SFXvolume;

    // particle colors
    private Color[] color = { new Color(0.5f, 0.5f, 0.5f) };

    // for powerups
    [SerializeField] private bool phasePowerup = false;
    [SerializeField] private int lockPowerup = 0;
    [SerializeField] private int explosionPowerup = 0;
    [SerializeField] private int hydraPowerup = 0;
    [SerializeField] private int shieldPowerup = 0;
    [SerializeField] private int resurrectPowerup = 0;
    [SerializeField] private bool factoryPowerup = false;
    [SerializeField] private bool pushPowerup = false;

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
        velocity = rb.velocity.magnitude;
        if (IsServer) { velocityNetworkedRounded.Value = velocity; }
        velocity = Math.Max(velocity, velocityNetworkedRounded.Value); // this is so joiner now has velocity

        // sliding noise SFX
        if (logic.gameIsRunning || ClientLogicScript.Instance.isRunning)
        {
            noiseSFX.volume = (velocity / 10.0f) * SFXvolume;
        }
        noiseSFX.mute = noiseSFX.volume < 0.01;  // this is weird but it prevents the 0.1 seconds of noise on spawn

        // update particle emisson based on velocity
        var PS = GetComponent<ParticleSystem>();
        ParticleSystem.EmissionModule emission = PS.emission;
        emission.rateOverTime = (velocity);
        ParticleSystem.MainModule main = PS.main;

        main.startColor = SetParticleColor();

        // phase powerup
        if (phasePowerup && (velocity > 1.0f || !IsShot()))
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            puckCollider.isTrigger = true;
        }

        if (!pastSafeLine && IsSafe())
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

        // for push
        if (IsShot() && pastSafeLine && IsSlowedMore())
        {
            if (pushPowerup)
            {
                GetComponentInChildren<NearbyPuckScript>().TriggerPush();
                pushPowerup = false;
                RemovePowerupText("push");
            }
        }

        // for phase & lock powerup
        if (IsShot() && pastSafeLine && velocity < 0.06) // using specific velocity here so it happens before game end trigger
        {
            if (phasePowerup)
            {
                // if this puck is within 2 units of the nearest puck, destroy it
                var phaseHasOverlap = false;
                var pucks = GameObject.FindGameObjectsWithTag("puck");
                foreach (var puck in pucks)
                {
                    if (puck != gameObject && Vector2.Distance(puck.transform.position, transform.position) < 2)
                    {
                        if (ClientLogicScript.Instance.isRunning && !IsServer) { break; }
                        phaseHasOverlap = true;
                        if (explosionPowerup > 0) // phase & explosion combo
                        {
                            puck.GetComponent<PuckScript>().DestroyPuck();
                        }
                    }
                }
                if (phaseHasOverlap)
                {
                    Explode();
                    return;
                }

                // othewrise, unphase it (make it visible again)
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                puckCollider.isTrigger = false;
                phasePowerup = false;
                RemovePowerupText("phase");
            }

            if (lockPowerup > 0 && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                rb.angularVelocity = 0;
                rb.velocity = Vector2.zero;
            }

            if (trail.endColor == Color.yellow)
            {
                trail.startColor = Color.white;
                trail.endColor = Color.white;
            }
        }

        if (!requestedCleanup && IsShot() && IsStopped())
        {
            if (LogicScript.Instance.gameIsRunning)
            {
                PuckManager.Instance.CleanupDeadPucks();
            }
            else if (ClientLogicScript.Instance.isRunning)
            {
                ServerLogicScript.Instance.CleanupDeadPucksServerRpc();
            }
            requestedCleanup = true;
            RemovePowerupText("triple"); // i have nowhere else to remove triple so its going here lol
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

        // animation
        gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, new Vector3(0.2f, 0.2f, 0.2f), 0.5f).setEase(LeanTweenType.easeOutQuint).setDelay(0.01f);

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
            LogicScript.Instance.activeCompetitor.activePuckScript = this;
            LogicScript.Instance.activeCompetitor.activePuckObject = gameObject;
        }

        if (transform.position.y > 0)
        {
            shot = true;
            safe = true;
        }

        // animation
        gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, new Vector3(0.2f, 0.2f, 0.2f), 0.5f).setEase(LeanTweenType.easeOutQuint).setDelay(0.01f);

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
        var boostedPowerParameter = powerParameter;
        if (powerParameter >= 95)
        {
            boostedPowerParameter += (powerParameter - 95) * powerModifier + 10;
        }
        // normalize power and then scale
        power = (boostedPowerParameter - (boostedPowerParameter - 50) * 0.5f) * powerModifier;
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
        // FX
        if (ClientLogicScript.Instance.isRunning) { ShotFXClientRpc(powerParameter); }
        else if (LogicScript.Instance.gameIsRunning) { ShotFX(powerParameter); }
    }

    [ServerRpc]
    public void ShootServerRpc(float angleParameter, float powerParameter, float spinParameter = 50)
    {
        Shoot(angleParameter, powerParameter, spinParameter);
    }

    [ClientRpc]
    public void ShotFXClientRpc(float powerParameter)
    {
        ShotFX(powerParameter);
    }

    private void ShotFX(float powerParameter)
    {
        ScreenShake.Instance.Shake(powerParameter / 500);
        var volumeBoost = 0f;
        if (trail != null && powerParameter >= 95)
        {
            volumeBoost += 0.2f;
            trail.startColor = powerParameter >= 99 ? Color.red : Color.yellow;
            trail.endColor = Color.yellow;
        }
        shotSFX.volume = SFXvolume + volumeBoost;
        shotSFX.Play();
    }

    // ---------- GETTERS AND SETTERS ----------
    public bool IsSlowed() { return velocity < 2 && IsShot() && transform.position.y > -9; }
    public bool IsSlowedMore() { return velocity < 0.4 && IsShot() && transform.position.y > -9; }
    public bool IsStopped() { return velocity < 0.05 && IsShot() && transform.position.y > -9; }
    public bool IsShot() { return shot; }
    public bool IsSafe() { return safe; }
    public bool IsPastSafeLine() { return pastSafeLine; }
    public bool IsPlayersPuck() { return playersPuck; }
    public int ComputeValue() { return (puckBaseValue * zoneMultiplier) + (zoneMultiplier > 0 ? puckBonusValue : 0); }
    public int GetZoneMultiplier() { return zoneMultiplier; }
    public void SetZoneMultiplier(int ZM) { zoneMultiplier = ZM; }
    public bool IsLocked() { return lockPowerup > 0; }
    public bool IsExplosion() { return explosionPowerup > 0; }
    public bool IsHydra() { return hydraPowerup > 0; }
    public bool IsPhase() { return phasePowerup; }
    public bool IsResurrect() { return resurrectPowerup > 0; }
    public bool IsFactory() { return factoryPowerup; }

    // when a puck enters a scoring zone, update its score and play a SFX
    public void EnterScoreZone(bool isZoneSafe, int enteredZoneMultiplier, bool isBoundry)
    {
        // all zones are past safe line, so pastSafeLine can be set to true permanently
        pastSafeLine = true;
        safe = isZoneSafe;
        shot = true;

        if (isBoundry) { return; } // don't update score or play SFX for the safe line boundry. This is important for hydra / factory powerups.

        // if puck moves into higher scoring zone and gains a point play SFX
        if (enteredZoneMultiplier > zoneMultiplier && (puckBaseValue + puckBonusValue) > 0)
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

    // when a puck exits a scoring zone. this only gets called if the zone is contained or is a boundry.
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

    [ClientRpc]
    public void EnterScoreZoneClientRpc(bool isZoneSafe, int enteredZoneMultiplier, bool isBoundry)
    {
        //if (enteredZoneMultiplier != zoneMultiplier) { Debug.Log("Mismatch");  }
        EnterScoreZone(isZoneSafe, enteredZoneMultiplier, isBoundry);
    }

    [ClientRpc]
    public void ExitScoreZoneClientRpc(bool isBoundry, int exitedZoneMultiplier)
    {
        ExitScoreZone(isBoundry, exitedZoneMultiplier);
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
        // FX
        if (ClientLogicScript.Instance.isRunning) { CollisionFXClientRpc(col.GetContact(0).point); }
        else if (LogicScript.Instance.gameIsRunning) { CollisionFX(col.GetContact(0).point); }
        angularVelocityModifier = 0;

        // explosion powerup
        if (ClientLogicScript.Instance.isRunning && !IsServer) return; // stop explosion shuffle bug
        if (explosionPowerup > 0 && col.gameObject.CompareTag("puck"))
        {
            // Destroy the collided object
            if (Vector2.Distance(col.gameObject.transform.position, transform.position) < 2.2f) // make sure it's nearby (trying to fix a weird bug)
            {
                col.gameObject.GetComponent<PuckScript>().DestroyPuck();
                Explode();
            }
        }
    }

    [ClientRpc]
    public void CollisionFXClientRpc(Vector2 colPoint)
    {
        CollisionFX(colPoint);
    }

    private void CollisionFX(Vector2 colPoint)
    {
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
            ParticleSystem collisionParticleEffect = Instantiate(collisionParticleEffectPrefab, colPoint, Quaternion.identity);
            collisionParticleEffect.transform.position = colPoint;
            ParticleSystem.EmissionModule emission = collisionParticleEffect.emission;
            emission.rateOverTime = (velocity) * 75f;
            ParticleSystem.MainModule main = collisionParticleEffect.main;
            main.startSpeed = (velocity) * 5f;

            main.startColor = SetParticleColor();

            collisionParticleEffect.Play();
            Destroy(collisionParticleEffect.gameObject, 5f);
            ScreenShake.Instance.Shake(velocity / 20);
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

    public void IncrementPuckBonusValue(int value)
    {
        puckBonusValue += value;
    }

    public void DoublePuckBaseValue()
    {
        puckBaseValue *= 2;
    }

    private void ZeroOutScoreHelper()
    {
        if (LogicScript.Instance.gameIsRunning)
        {
            ZeroOutScore();
        }
        else if (ClientLogicScript.Instance.isRunning)
        {
            ZeroOutScoreClientRpc();
        }
    }

    [ClientRpc]
    public void ZeroOutScoreClientRpc()
    {
        ZeroOutScore();
    }

    private void ZeroOutScore()
    {
        SetPuckBaseValue(0);
        SetPuckBonusValue(0);
        SetZoneMultiplier(0);
    }


    public void SetPowerupText(string text, bool showFloatingText = true)
    {
        powerupText.Add(text);
        if (showFloatingText) { CreatePowerupFloatingText(); }
    }

    public void RemovePowerupText(string text)
    {
        powerupText.Remove(text);
    }

    // text to show what powerup was used under the puck. Only fades, no up/shrink.
    public void CreatePowerupFloatingText()
    {
        var floatingText = Instantiate(floatingTextPrefab, transform.position + new Vector3(0, -1.5f, 0), Quaternion.identity, transform);
        floatingText.GetComponent<FloatingTextScript>().Initialize(string.Join("\n", powerupText), 0, 0, 0.1f, 1, true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // show puck score when clicked
        var floatingText = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
        floatingText.GetComponent<FloatingTextScript>().Initialize(ComputeValue().ToString(), 1, 1, 1, 1.5f + (ComputeValue() / 10), true);
        // if powerupText has been set, show it
        if (powerupText.Count > 0)
        {
            CreatePowerupFloatingText();
        }
    }

    public void DestroyPuck() // Destroys the puck with a particle and sound effect, used by powerups that say destroy
    {
        if (ClientLogicScript.Instance.isRunning && !IsOwner) { return; } // only owner can destroy

        if (shieldPowerup > 0)
        {
            TriggerShield();
            return;
        };
        // update score
        ZeroOutScoreHelper();
        LogicScript.Instance.UpdateScores();
        // hydra powerup
        while (hydraPowerup > 0)
        {
            // todo: move the online check into PuckSpawnHelper and call PuckSpawnHelperServerRpc from there
            if (ClientLogicScript.Instance.isRunning)
            {
                ServerLogicScript.Instance.PuckSpawnHelperServerRpc(playersPuck, transform.position.x, transform.position.y, 2);
                hydraPowerup--;
                RemovePowerupText("hydra");
            }
            else
            {
                PowerupManager.Instance.PuckSpawnHelper(playersPuck, transform.position.x, transform.position.y, 2);
                hydraPowerup--;
                RemovePowerupText("hydra");
            }
        }
        // resurrect powerup
        while (resurrectPowerup > 0)
        {
            if (ClientLogicScript.Instance.isRunning)
            {
                ServerLogicScript.Instance.AdjustPuckCountServerRpc(playersPuck, 1); // requires ownership
                resurrectPowerup--;
                RemovePowerupText("resurrect");
            }
            else
            {
                LogicScript.Instance.IncrementPuckCount(playersPuck);
                resurrectPowerup--;
                RemovePowerupText("resurrect");
            }
        }

        // SFX & screen shake
        if (ClientLogicScript.Instance.isRunning)
        {
            DestroyPuckFXClientRpc(); // todo put for phase
        }
        else
        {
            DestroyPuckFX();
        }

        // actually destroy the gameobject
        Destroy(gameObject);
    }

    private void DestroyPuckFX()
    {
        logic.playDestroyPuckSFX(SFXvolume);
        ScreenShake.Instance.Shake(0.25f);
    }

    [ClientRpc]
    public void DestroyPuckFXClientRpc()
    {
        DestroyPuckFX();
    }

    override public void OnDestroy()
    {
        if (!logic.gameIsRunning && !ClientLogicScript.Instance.isRunning) { return; }

        // update score
        ZeroOutScoreHelper();
        LogicScript.Instance.UpdateScores();

        // particle FX
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

        // un sub from events
        LogicScript.OnPlayerShot -= IncrementGrowthValue;
        LogicScript.OnOpponentShot -= IncrementGrowthValue;
        ClientLogicScript.OnPlayerShot -= IncrementGrowthValue;
        ClientLogicScript.OnOpponentShot -= IncrementGrowthValue;
        LogicScript.OnPlayerShot -= DisableLock;
        LogicScript.OnOpponentShot -= DisableLock;
        ClientLogicScript.OnPlayerShot -= DisableLock;
        ClientLogicScript.OnOpponentShot -= DisableLock;
        LogicScript.OnPlayerShot -= FactoryHelper;
        LogicScript.OnOpponentShot -= FactoryHelper;
        ClientLogicScript.OnPlayerShot -= FactoryHelper;
        ClientLogicScript.OnOpponentShot -= FactoryHelper;
        LogicScript.OnPlayerShot -= IncrementExponentValue;
        LogicScript.OnOpponentShot -= IncrementExponentValue;
        ClientLogicScript.OnPlayerShot -= IncrementExponentValue;
        ClientLogicScript.OnOpponentShot -= IncrementExponentValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyPuckServerRpc()
    {
        if (!IsServer) return;
        DestroyPuck();
    }

    public void SetPhase(bool isPhase)
    {
        phasePowerup = isPhase;
    }

    [ClientRpc]
    public void InitBlockPuckClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;
        InitBlockPuck();
    }

    public void InitBlockPuck()
    {
        SetPuckBaseValue(0);
        SetPowerupText("valueless");
        shot = true;
        safe = true;
    }

    public void EnableGrowth()
    {
        if (ClientLogicScript.Instance.isRunning) // growth online
        {
            if (playersPuck)
            {
                ClientLogicScript.OnPlayerShot += IncrementGrowthValue;
            }
            else
            {
                ClientLogicScript.OnOpponentShot += IncrementGrowthValue;
            }
        }
        else if (LogicScript.Instance.gameIsRunning) // growth vs CPU
        {
            if (playersPuck)
            {
                LogicScript.OnPlayerShot += IncrementGrowthValue;
            }
            else
            {
                LogicScript.OnOpponentShot += IncrementGrowthValue;
            }
        }
    }

    private void IncrementGrowthValue()
    {
        if (this == null || transform == null || transform.position.y < 0) { return; }
        IncrementPuckBonusValue(1);
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
        lockPowerup++;

        if (lockPowerup != 1) { return; } // we only want to add one lock listener (only one lock instance gets removed per opp shot)

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
            lockPowerup--;
            if (lockPowerup <= 0)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }
            RemovePowerupText("lock");
        }
    }

    bool explodeFromRPC;
    public void EnableExplosion()
    {
        explosionPowerup++;
    }

    private void Explode()
    {
        if (ClientLogicScript.Instance.isRunning && !explodeFromRPC) {
            ExplodeServerRpc();
            return;
        }
        explosionPowerup--;
        RemovePowerupText("explosion");
        DestroyPuck();
        explodeFromRPC = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ExplodeServerRpc()
    {
        ExplodeClientRpc();
    }

    [ClientRpc]
    private void ExplodeClientRpc()
    {
        explodeFromRPC = true;
        Explode();
    }

    public void EnableHydra()
    {
        hydraPowerup++;
    }

    public void EnableFactory()
    {
        factoryPowerup = true;
        SetPuckBaseValue(0); // set to valueless
        if (ClientLogicScript.Instance.isRunning) // factory online
        {
            if (playersPuck)
            {
                ClientLogicScript.OnPlayerShot += FactoryHelper;
            }
            else
            {
                ClientLogicScript.OnOpponentShot += FactoryHelper;
            }
        }
        else if (LogicScript.Instance.gameIsRunning) // factory vs CPU
        {
            if (playersPuck)
            {
                LogicScript.OnPlayerShot += FactoryHelper;
            }
            else
            {
                LogicScript.OnOpponentShot += FactoryHelper;
            }
        }
    }

    private void FactoryHelper()
    {
        if (this == null || transform == null || transform.position.y < 0) { return; }
        if (ClientLogicScript.Instance.isRunning)
        {
            ServerLogicScript.Instance.PuckSpawnHelperServerRpc(playersPuck, transform.position.x, transform.position.y, 1);
        }
        else
        {
            PowerupManager.Instance.PuckSpawnHelper(playersPuck, transform.position.x, transform.position.y, 1);
        }
    }

    public void EnableShield()
    {
        shieldPowerup++;
    }

    bool shieldFromRPC;
    private void TriggerShield()
    {
        if (ClientLogicScript.Instance.isRunning && !shieldFromRPC)
        {
            TriggerShieldServerRpc();
            return;
        }
        shieldPowerup--;
        RemovePowerupText("shield");
        // Grey particles
        ParticleSystem collisionParticleEffect = Instantiate(collisionParticleEffectPrefab, transform.position, Quaternion.identity);
        ParticleSystem.EmissionModule emission = collisionParticleEffect.emission;
        emission.rateOverTime = 500f;
        ParticleSystem.MainModule main = collisionParticleEffect.main;
        main.startSpeed = 50f;
        main.startColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        collisionParticleEffect.Play();
        Destroy(collisionParticleEffect.gameObject, 10f);
        // Screen shake
        ScreenShake.Instance.Shake(0.25f);
        // play break sfx
        breakSFX.volume *= SFXvolume;
        breakSFX.Play();
        shieldFromRPC = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerShieldServerRpc()
    {
        TriggerShieldClientRpc();
    }

    [ClientRpc]
    private void TriggerShieldClientRpc()
    {
        shieldFromRPC = true;
        TriggerShield();
    }

    public void EnableResurrect()
    {
        resurrectPowerup++;
    }

    public void EnableExponent()
    {
        if (ClientLogicScript.Instance.isRunning) // exponent online
        {
            if (playersPuck)
            {
                ClientLogicScript.OnPlayerShot += IncrementExponentValue;
            }
            else
            {
                ClientLogicScript.OnOpponentShot += IncrementExponentValue;
            }
        }
        else if (LogicScript.Instance.gameIsRunning) // exponent vs CPU
        {
            if (playersPuck)
            {
                LogicScript.OnPlayerShot += IncrementExponentValue;
            }
            else
            {
                LogicScript.OnOpponentShot += IncrementExponentValue;
            }
        }
    }

    private void IncrementExponentValue()
    {
        if (this == null || transform == null || transform.position.y < 0) { return; }
        DoublePuckBaseValue();
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

    public void EnablePush()
    {
        pushPowerup = true;
    }

    public void EnableErratic()
    {
        if (ClientLogicScript.Instance.isRunning) // Erratic online
        {
            if (playersPuck)
            {
                ClientLogicScript.OnOpponentShot += Erratic;
            }
            else
            {
                ClientLogicScript.OnPlayerShot += Erratic;
            }
        }
        else if (LogicScript.Instance.gameIsRunning) // Erratic vs CPU
        {
            if (playersPuck)
            {
                LogicScript.OnOpponentShot += Erratic;
            }
            else
            {
                LogicScript.OnPlayerShot += Erratic;
            }
        }
    }

    private void Erratic()
    {
        if (this == null || transform == null || transform.position.y < 0 || rb == null) { return; }
        // move the puck with physics impulse, default random
        float x = UnityEngine.Random.Range(-1f, 1f);
        float y = UnityEngine.Random.Range(-1f, 1f);
        // if greater than this y, move down
        if (transform.position.y > 15.5)
        {
            y = UnityEngine.Random.Range(-1f, -0.5f);
        }
        // if less than this y, move up
        else if (transform.position.y < 8.5)
        {
            y = UnityEngine.Random.Range(0.5f, 1f);
        }
        // if greater than this x, move left (negative x)
        if (transform.position.x > 7)
        {
            x = UnityEngine.Random.Range(-1f, -0.5f);
        }
        // if less than than this x, move right (positive x)
        else if (transform.position.x < -7)
        {
            x = UnityEngine.Random.Range(0.5f, 1f);
        }

        rb.AddForce(new Vector2(x, y).normalized*5, ForceMode2D.Impulse);
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
        while ( color.Length <= 4 ) // Double the colors until there are at least 4, to make the gradient switch colors more often
        {
            Color[] newColor = new Color[color.Length * 2];
            for (int i = 0; i < color.Length; i++)
            {
                newColor[i] = color[i];
                newColor[i + color.Length] = color[i];
            }
            color = newColor;
        }
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
