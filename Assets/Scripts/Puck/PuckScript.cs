/* Each puck is assigned one of
 * these scripts when it generates.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using System;
using System.Collections.Generic;

public class PuckScript : NetworkBehaviour, IPointerClickHandler
{
    // --------- OBJECT COMPONENTS ----------
    #region OBJECT COMPONENTS
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
    #endregion

    // ---------- LOCAL FIELDS ----------
    #region LOCAL FIELDS
    // script variables
    private bool shot;
    private bool safe;
    private bool pastSafeLine;
    private bool playersPuck;
    private bool requestedCleanup;
    private float velocity;
    public NetworkVariable<float> velocityNetworkedRounded = new NetworkVariable<float>();

    // movement modifiers (constant)
    [SerializeField] private float powerModifier;
    [SerializeField] private float angularVelocityModifier;
    [SerializeField] private float counterForce;

    // shot parameters
    private float angle;
    private float power;
    private float spin;
    private Vector2 shotForceToAdd;
    private float shotTorqueToAdd;

    // scoring
    private int puckBaseValue = 1;
    private int puckBonusValue = 0;
    private int zoneMultiplier = 0;

    // floating text
    [SerializeField] private GameObject floatingTextPrefab;
    Dictionary<string, int> powerupText = new Dictionary<string, int>();

    // sound effect volume
    private float SFXvolume;

    // particle colors
    private Color[] color = { new Color(0.5f, 0.5f, 0.5f) };
    #endregion

    // ---------- GETTERS AND SETTERS ----------
    #region GETTERS AND SETTERS
    public bool IsShot() { return shot; }
    public bool IsSafe() { return safe; }
    public bool IsPastSafeLine() { return pastSafeLine; }
    public bool IsSlowed() { return velocity < 2 && IsShot() && (transform.position.y > -9 || transform.position.y < -10 || pastSafeLine); }
    public bool IsSlowedMore() { return velocity < 0.4 && IsShot() && (transform.position.y > -9 || transform.position.y < -10 || pastSafeLine); }
    public bool IsStopped() { return velocity < 0.05 && IsShot() && (transform.position.y > -9 || transform.position.y < -10 || pastSafeLine); }
    public bool IsPlayersPuck() { return playersPuck; }
    // ComputeValue() is the value scored, and shown to the player
    public int ComputeValue() { return phasePowerup ? 0 : (puckBaseValue * zoneMultiplier) + (zoneMultiplier > 0 ? puckBonusValue : 0); }
    // ComputeTotalFutureValue() is what the final value of the puck would be at the end of the match if uninteracted with.
    // (Or in the case of factory, how much value that puck would create).
    // This is used for CPU behavior to put higher priority on dealing with pucks that create value over time.
    public int ComputeTotalFutureValue()
    {
        if (phasePowerup) return 0;
        if (zoneMultiplier <= 0 && transform.position.y > 9) return 0;

        // for future value, if the puck is able to be hit into a scoring zone (below y 9), treat it as minimum zone mult of 1
        // if the puck is also placed middle-ish, treat it as minimum zone mult of 2 instead
        int tempZoneMultForTotalFutureValue = Math.Max((transform.position.x > -5 && transform.position.x < 5) ? 2 : 1, zoneMultiplier);

        int puckValue = (puckBaseValue * tempZoneMultForTotalFutureValue) + (tempZoneMultForTotalFutureValue > 0 ? puckBonusValue : 0);
        if (HasGrowth()) puckValue += LogicScript.Instance.player.puckCount * GetGrowthCount();
        if (HasFactory()) puckValue += LogicScript.Instance.player.puckCount * 2 * GetFactoryCount();
        if (HasExponent()) puckValue += Math.Max(0, puckBaseValue * tempZoneMultForTotalFutureValue * (int)Math.Pow((int)Mathf.Pow(2, GetExponentCount()), LogicScript.Instance.player.puckCount) - (puckBaseValue * tempZoneMultForTotalFutureValue));
        return puckValue;
    }

    // base value is multplied by score zone value
    public int GetPuckBaseValue() { return puckBaseValue; }
    private void SetPuckBaseValue(int value) { puckBaseValue = value; }
    public void DoublePuckBaseValue() { puckBaseValue *= 2; }

    // bonus value is added onto base value * score zone value
    public int GetPuckBonusValue() { return puckBonusValue; }
    private void SetPuckBonusValue(int value) { puckBonusValue = value; }
    public void IncrementPuckBonusValue(int value) { puckBonusValue += value; }

    // score zone multiplier is multplied by base value
    public int GetZoneMultiplier() { return zoneMultiplier; }
    private void SetZoneMultiplier(int ZM) { zoneMultiplier = ZM; }

    private void ZeroOutScore()
    {
        SetPuckBaseValue(0);
        SetPuckBonusValue(0);
        SetZoneMultiplier(0);
    }

    [ClientRpc] public void ZeroOutScoreClientRpc() { ZeroOutScore(); }

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
    #endregion

    // ---------- INITIALIZER ----------
    #region INITIALIZER
    // initiate a new puck
    public PuckScript InitPuck(bool IsPlayersPuckParameter, int puckSpriteID)
    {
        spriteRenderer.sprite = PuckSkinManager.Instance.ColorIDtoPuckSprite(puckSpriteID);
        color = PuckSkinManager.Instance.ColorIDtoColor(puckSpriteID);
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

        var swapAlt = !IsPlayersPuckParameter && puckSpriteID == LogicScript.Instance.player.puckSpriteID ? -1 : 1;

        Sprite puckSprite = PuckSkinManager.Instance.ColorIDtoPuckSprite(puckSpriteID * swapAlt);
        spriteRenderer.sprite = puckSprite;
        color = PuckSkinManager.Instance.ColorIDtoColor(puckSpriteID * swapAlt);
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

        Debug.Log($"Puck initialized. Players: {IsPlayersPuckParameter}. SpriteID: {puckSpriteID}");
    }
    #endregion

    // ---------- UNITY METHODS ----------
    #region UNITY METHODS
    void OnEnable()
    {
        SFXvolume = SoundManagerScript.Instance.GetSFXVolume();
    }

    void FixedUpdate()
    {
        // if shot, add the force / torque once
        if (IsShot() && shotForceToAdd != Vector2.zero)
        {
            rb.AddForce(shotForceToAdd);
            rb.AddTorque(shotTorqueToAdd);
            shotForceToAdd = Vector2.zero;
            shotTorqueToAdd = 0f;
        }

        // Calculate the magnitude of the velocity vector to determine the sliding noise volume
        velocity = rb.linearVelocity.magnitude;
        if (IsServer) { velocityNetworkedRounded.Value = velocity; }
        velocity = Math.Max(velocity, velocityNetworkedRounded.Value); // this is so joiner now has velocity

        // play sliding noise SFX
        if (LogicScript.Instance.gameIsRunning || ClientLogicScript.Instance.isRunning)
        {
            noiseSFX.volume = (velocity / 10.0f) * SFXvolume;
        }
        noiseSFX.mute = noiseSFX.volume < 0.01f;  // this is weird but it prevents the 0.1 seconds of noise on spawn

        // update particle emisson based on velocity
        var PS = GetComponent<ParticleSystem>();
        ParticleSystem.EmissionModule emission = PS.emission;
        emission.rateOverTime = (velocity);
        ParticleSystem.MainModule main = PS.main;

        main.startColor = SetParticleColor();

        // handle spinning forces
        var right = transform.InverseTransformDirection(transform.right);
        var up = transform.InverseTransformDirection(transform.up);
        if (!IsSlowed() && IsShot() && pastSafeLine)
        {
            // add horizontal spinning force
            rb.AddForce((right * rb.angularVelocity * angularVelocityModifier) * -0.03f);

            // add counter force downwards
            if (rb.angularVelocity > 0)
            {
                rb.AddForce((up * rb.angularVelocity * angularVelocityModifier * counterForce) * -0.03f);
            }
            else
            {
                rb.AddForce((up * rb.angularVelocity * angularVelocityModifier * counterForce) * 0.03f);
            }
        }

        if (!pastSafeLine && IsSafe())
        {
            pastSafeLine = true;
        }

        // phase powerup (in motion)
        if (phasePowerup && (velocity > 1.0f || !IsShot())) // while moving, phase out
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            puckCollider.isTrigger = true;
        }

        // push powerup
        if (IsShot() && pastSafeLine && IsSlowedMore())
        {
            if (pushPowerup > 0)
            {
                for (int i = 0; i < pushPowerup; i++)
                {
                    GetComponentInChildren<NearbyPuckScript>().TriggerPush();
                    RemovePowerupText("push");
                }
                pushPowerup = 0;
                //PowerupAnimationManager.Instance.PlayPowerupActivationAnimation(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.PushPowerup), transform.position);
            }
        }

        // for phase & lock powerup stopping
        if (IsShot() && pastSafeLine && velocity < 0.06) // using specific velocity here so it happens before game end trigger
        {
            if (phasePowerup)
            {
                // handle overlaps
                bool phaseHasOverlap = false;
                bool phaseHasOverlapWithPhase = false;
                var pucks = GameObject.FindGameObjectsWithTag("puck");
                // if this puck comes to a stop overlapping with another phase, destroy them both because they can never fade in
                foreach (var puck in pucks)
                {
                    if (puck != gameObject && Vector2.Distance(puck.transform.position, transform.position) < 2)
                    {
                        phaseHasOverlap = true;
                        if (ClientLogicScript.Instance.isRunning && !IsServer) { break; }
                        // if two stopped phases overlap, they can never phase in, so destroy them both
                        if (puck.GetComponent<PuckScript>().HasPhase() && puck.GetComponent<PuckScript>().velocityNetworkedRounded.Value < 0.06f)
                        {
                            phaseHasOverlapWithPhase = true;
                            puck.GetComponent<PuckScript>().DestroyPuck();
                            DestroyPuck();
                        }
                    }
                }
                // if this puck has explosion and overlaps with a non-phased puck, explode them both
                foreach (var puck in pucks)
                {
                    if (puck != gameObject && Vector2.Distance(puck.transform.position, transform.position) < 2)
                    {
                        phaseHasOverlap = true;
                        if (ClientLogicScript.Instance.isRunning && !IsServer) { break; }
                        // phase & explosion combo, explode both (don't do this if it's ALSO overlapping with a phase)
                        if (HasExplosion() && !phaseHasOverlapWithPhase)
                        {
                            puck.GetComponent<PuckScript>().DestroyPuck(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.ExplosionPowerup));
                            Explode();
                        }
                    }
                }

                // if phase comes to a stop unobstructed
                if (!phaseHasOverlap)
                {
                    /// unphase it (make it visible again)
                    spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                    puckCollider.isTrigger = false;
                    phasePowerup = false;
                    RemoveAllPowerupText("phase");
                    // if it's in a scoring zone, give it score
                    if (zoneMultiplier > 0)
                    {
                        if (IsPlayersPuck())
                        {
                            pointPlayerSFX.volume = SFXvolume;
                            pointPlayerSFX.pitch = 0.9f + (0.05f * zoneMultiplier);
                            pointPlayerSFX.Play();
                        }
                        else
                        {
                            pointCPUSFX.volume = SFXvolume;
                            pointCPUSFX.pitch = 0.8f + (0.05f * zoneMultiplier);
                            pointCPUSFX.Play();
                        }
                    }
                    CreateScoreFloatingText();
                    LogicScript.Instance.UpdateScores();
                }
            }

            // trigger lock upon stopping
            if (HasLock() && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                rb.angularVelocity = 0;
                rb.linearVelocity = Vector2.zero;
            }

            // reset trail color to white upon stopping
            if (trail.endColor == Color.yellow)
            {
                trail.startColor = Color.white;
                trail.endColor = Color.white;
            }
        }

        // after stopping, request cleanup of all pucks. So that this puck is destroyed if it is outside the play area
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
        }
    }
    #endregion

    // ---------- CUSTOM METHODS ----------
    #region CUSTOM METHODS

    // --- Shooting ---
    #region Shooting
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
    #endregion

    // --- Scoring Zones ---
    #region Scoring Zones
    // when a puck enters a scoring zone, update its score and play a SFX
    public void EnterScoreZone(bool isZoneSafe, int enteredZoneMultiplier, bool isBoundry)
    {
        // all zones are past safe line, so pastSafeLine can be set to true permanently
        pastSafeLine = true;
        safe = isZoneSafe;
        shot = true;

        if (isBoundry) { return; } // don't update score or play SFX for the safe line boundry. This is important for hydra / factory powerups.

        // if puck moves into higher scoring zone and gains a point play SFX
        if (enteredZoneMultiplier > zoneMultiplier && (puckBaseValue + puckBonusValue) > 0 && !phasePowerup)
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
        else if (enteredZoneMultiplier < zoneMultiplier && (puckBaseValue + puckBonusValue) > 0 && !phasePowerup)
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
            CreateScoreFloatingText();
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
    #endregion

    // --- Collisions ---
    #region Collisions
    [SerializeField] private ParticleSystem collisionParticleEffectPrefab;

    void OnCollisionEnter2D(Collision2D col)
    {
        // FX
        if (ClientLogicScript.Instance.isRunning) { CollisionFXClientRpc(col.GetContact(0).point); }
        else { CollisionFX(col.GetContact(0).point); }
        angularVelocityModifier = 0;

        // explosion powerup
        if (ClientLogicScript.Instance.isRunning && !IsServer) return; // stop explosion shuffle bug
        if (HasExplosion() && col.gameObject.CompareTag("puck"))
        {
            // Destroy the collided object
            if (Vector2.Distance(col.gameObject.transform.position, transform.position) < 2.2f) // make sure it's nearby (trying to fix a weird bug)
            {
                // if this puck has multiple explosions effects, try to destroy the collided puck that many times. But don't use any more explosions than required to destroy it.
                int explosionsTriggered = 0;
                for (int i = 0; i < GetExplosionCount(); i++)
                {
                    // if the target puck has no shield, we only need to trigger one more explosion
                    if (col.gameObject.GetComponent<PuckScript>().GetShieldCount() <= 0)
                    {
                        i += 9999;
                    }
                    // trigger a destroy on target puck
                    if (col.gameObject != null && col.gameObject.GetComponent<PuckScript>() != null && col.gameObject.GetComponent<PuckScript>() != null)
                    {
                        col.gameObject.GetComponent<PuckScript>().DestroyPuck(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.ExplosionPowerup));
                        explosionsTriggered++;
                    }
                }
                // destroy THIS puck that many times
                for (int i = 0; i < explosionsTriggered; i++)
                {
                    Explode();
                }
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
            ScreenShake.Instance.Shake(velocity / (LogicScript.Instance.gameIsRunning ? 20 : 100));
        }
    }
    #endregion

    // --- Floating Text ---
    #region Floating Text
    private void CreateScoreFloatingText()
    {
        var floatingText = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
        floatingText.GetComponent<FloatingTextScript>().Initialize(ComputeValue().ToString(), 1, 1, 1, 1.5f + (ComputeValue() / 10), true);
    }

    public void AddPowerupText(string text, bool showFloatingText = true)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        if (powerupText.ContainsKey(text))
        {
            powerupText[text]++;
        }
        else
        {
            powerupText[text] = 1;
        }

        if (showFloatingText) { CreatePowerupFloatingText(); }
    }

    public void RemovePowerupText(string text)
    {
        if (powerupText.ContainsKey(text))
        {
            powerupText[text]--;
            if (powerupText[text] <= 0)
            {
                powerupText.Remove(text);
            }
        }
    }

    public void RemoveAllPowerupText(string text = null)
    {
        if (text != null)
        {
            powerupText.Remove(text);
        }
        else
        {
            powerupText.Clear();
        }
    }

    public List<string> GetFormattedPowerupText()
    {
        List<string> result = new List<string>();

        foreach (var pair in powerupText)
        {
            if (pair.Value > 1)
                result.Add($"{pair.Key} x{pair.Value}");
            else
                result.Add(pair.Key);
        }

        return result;
    }


    private GameObject activePowerupFloatingText;
    private void CreatePowerupFloatingText()
    {
        // Destroy previous powerup floating text if it exists
        if (activePowerupFloatingText != null)
        {
            Destroy(activePowerupFloatingText);
        }

        // Create new powerup floating text
        activePowerupFloatingText = Instantiate(floatingTextPrefab, transform.position + new Vector3(0, -1.5f, 0), Quaternion.identity, transform);
        activePowerupFloatingText.GetComponent<FloatingTextScript>().Initialize(string.Join("\n", GetFormattedPowerupText()), 0, 0, 0.1f, 1, true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // show puck score when clicked
        CreateScoreFloatingText();
        // if powerupText has been set, show it
        if (powerupText.Count > 0)
        {
            CreatePowerupFloatingText();
        }
    }
    #endregion

    // --- Destroying Pucks ---
    #region Destroying Pucks
    public void DestroyPuck(int effectIndex = -1) // Destroys the puck with a particle and sound effect, used by powerups that say destroy
    {
        if (ClientLogicScript.Instance.isRunning && !IsOwner) { return; } // only owner can destroy

        if (HasShield())
        {
            TriggerShield();
            return;
        };
        // update score
        ZeroOutScoreHelper();
        LogicScript.Instance.UpdateScores();
        // hydra powerup
        while (HasHydra())
        {
            // todo: move the online check into PuckSpawnHelper and call PuckSpawnHelperServerRpc from there
            if (ClientLogicScript.Instance.isRunning)
            {
                ServerLogicScript.Instance.PuckSpawnHelperServerRpc(playersPuck, transform.position.x, transform.position.y, 2);
                hydraPowerup--;
                RemovePowerupText("hydra");
                //PowerupAnimationManager.Instance.PlayPowerupActivationAnimation(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.HydraPowerup), transform.position);
            }
            else
            {
                PowerupManager.Instance.PuckSpawnHelper(playersPuck, transform.position.x, transform.position.y, 2);
                hydraPowerup--;
                RemovePowerupText("hydra");
                //PowerupAnimationManager.Instance.PlayPowerupActivationAnimation(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.HydraPowerup), transform.position);
            }
        }
        // resurrect powerup
        while (HasResurrect())
        {
            if (ClientLogicScript.Instance.isRunning)
            {
                ServerLogicScript.Instance.AdjustPuckCountServerRpc(playersPuck, 1); // requires ownership
                resurrectPowerup--;
                RemovePowerupText("resurrect");
                //PowerupAnimationManager.Instance.PlayPowerupActivationAnimation(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.ResurrectPowerup), transform.position);
            }
            else
            {
                LogicScript.Instance.IncrementPuckCount(playersPuck);
                resurrectPowerup--;
                RemovePowerupText("resurrect");
                //PowerupAnimationManager.Instance.PlayPowerupActivationAnimation(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.ResurrectPowerup), transform.position);
            }
        }

        // SFX & screen shake
        if (ClientLogicScript.Instance.isRunning)
        {
            DestroyPuckFXClientRpc(effectIndex); // todo put for phase
        }
        else
        {
            DestroyPuckFX(effectIndex);
        }

        // actually destroy the gameobject
        Destroy(gameObject);
    }

    private void DestroyPuckFX(int effectIndex = -1)
    {
        SoundManagerScript.Instance.PlayDestroyPuckSFX(SFXvolume);
        ScreenShake.Instance.Shake(0.25f);
        if (effectIndex > 0)
        {
            PowerupAnimationManager.Instance.PlayPowerupActivationAnimation(effectIndex, transform.position);
        }
    }

    [ClientRpc]
    public void DestroyPuckFXClientRpc(int effectIndex = -1)
    {
        DestroyPuckFX(effectIndex);
    }

    override public void OnDestroy()
    {
        if (!LogicScript.Instance.gameIsRunning && !ClientLogicScript.Instance.isRunning) { return; }

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

        // un sub from events to prevent memory leaks and ensure events don't try to call methods on a destroyed object
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
    public void DestroyPuckServerRpc(int effectIndex = -1)
    {
        if (!IsServer) return;
        DestroyPuck(effectIndex);
    }
    #endregion
    #endregion

    // ---------- POWERUP LOCAL FIELDS & GETTERS ----------
    # region POWERUP LOCAL FIELDS & GETTERS
    private int plusOnePowerup = 0;
    public int GetPlusOneCount() { return plusOnePowerup; }
    public bool HasPlusOne() { return plusOnePowerup > 0; }

    private bool phasePowerup = false;
    public bool HasPhase() { return phasePowerup; }

    private int growthPowerup = 0;
    public int GetGrowthCount() { return growthPowerup; }
    public bool HasGrowth() { return growthPowerup > 0; }

    private int lockPowerup = 0;
    public int GetLockCount() { return lockPowerup; }
    public bool HasLock() { return lockPowerup > 0; }

    private int explosionPowerup = 0;
    public int GetExplosionCount() { return explosionPowerup; }
    public bool HasExplosion() { return explosionPowerup > 0; }

    private int hydraPowerup = 0;
    public int GetHydraCount() { return hydraPowerup; }
    public bool HasHydra() { return hydraPowerup > 0; }

    private int factoryPowerup = 0;
    public int GetFactoryCount() { return factoryPowerup; }
    public bool HasFactory() { return factoryPowerup > 0; }

    private int shieldPowerup = 0;
    public int GetShieldCount() { return shieldPowerup; }
    public bool HasShield() { return shieldPowerup > 0; }

    private int timesTwoPowerup = 0;
    public int GetTimesTwoCount() { return timesTwoPowerup; }
    public bool HasTimesTwo() { return timesTwoPowerup > 0; }

    private int resurrectPowerup = 0;
    public int GetResurrectCount() { return resurrectPowerup; }
    public bool HasResurrect() { return resurrectPowerup > 0; }

    private int exponentPowerup = 0;
    public int GetExponentCount() { return exponentPowerup; }
    public bool HasExponent() { return exponentPowerup > 0; }

    public int GetAuraCount() { return GetComponentInChildren<NearbyPuckScript>().GetAuraCount(); }
    public bool HasAura() { return GetComponentInChildren<NearbyPuckScript>().GetAuraCount() > 0; }

    private int pushPowerup = 0;
    public int GetPushCount() { return pushPowerup; }
    public bool HasPush() { return pushPowerup > 0; }

    private int erraticPowerup = 0;
    public int GetErraticCount() { return erraticPowerup; }
    public bool HasErratic() { return erraticPowerup > 0; }

    private int plusThreePowerup = 0;
    public int GetPlusThreeCount() { return plusThreePowerup; }
    public bool HasPlusThree() { return plusThreePowerup > 0; }
    #endregion

    // ---------- POWERUP ACTIVATORS & METHODS ----------
    #region POWERUP ACTIVATORS & METHODS
    public void ActivatePlusOne()
    {
        plusOnePowerup++;
        AddPowerupText("plus one");

        IncrementPuckBonusValue(1);
    }

    public void InitBlockPuck()
    {
        SetPuckBaseValue(0);
        AddPowerupText("valueless");
        InitSpawnedPuck();
    }

    [ClientRpc]
    public void InitBlockPuckClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsClient) return;
        InitBlockPuck();
    }

    public void InitSpawnedPuck()
    {
        shot = true;
        safe = true;
    }

    public void ActivatePhase()
    {
        phasePowerup = true;
        AddPowerupText("phase");
    }

    public void ActivateGrowth()
    {
        growthPowerup++;
        AddPowerupText("growth");

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
        CreateScoreFloatingText();
        //PowerupAnimationManager.Instance.PlayPowerupActivationAnimation(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.GrowthPowerup), transform.position);
    }

    public void ActivateLock()
    {
        lockPowerup++;
        AddPowerupText("lock");

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
    public void ActivateExplosion()
    {
        explosionPowerup++;
        AddPowerupText("explosion");
    }

    private void Explode()
    {
        if (ClientLogicScript.Instance.isRunning && !explodeFromRPC) {
            ExplodeServerRpc();
            return;
        }
        explosionPowerup--;
        RemovePowerupText("explosion");
        DestroyPuck(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.ExplosionPowerup));
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

    public void ActivateHydra()
    {
        hydraPowerup++;
        AddPowerupText("hydra");
    }

    [ClientRpc]
    public void ActivateHydraClientRpc()
    {
        ActivateHydra();
    }

    public void ActivateFactory()
    {
        factoryPowerup++;
        AddPowerupText("factory");
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
        //PowerupAnimationManager.Instance.PlayPowerupActivationAnimation(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.FactoryPowerup), transform.position);
    }

    public void ActivateShield()
    {
        shieldPowerup++;
        AddPowerupText("shield");
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
        PowerupAnimationManager.Instance.PlayPowerupActivationAnimation(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.ShieldPowerup), transform.position);
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

    public void ActivateTimesTwo()
    {
        timesTwoPowerup++;
        AddPowerupText("times two");

        DoublePuckBaseValue();
    }

    public void ActivateResurrect()
    {
        resurrectPowerup++;
        AddPowerupText("resurrect");
    }

    public void ActivateExponent()
    {
        exponentPowerup++;
        AddPowerupText("exponent");

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
        CreateScoreFloatingText();
        //PowerupAnimationManager.Instance.PlayPowerupActivationAnimation(Array.IndexOf(PowerupManager.Instance.methodArray, PowerupManager.Instance.ExponentPowerup), transform.position);
    }

    public void ActivateAura()
    {
        AddPowerupText("aura");

        GetComponentInChildren<NearbyPuckScript>().ActivateAura();
    }

    public void ActivatePush()
    {
        pushPowerup++;
        AddPowerupText("push");

        GetComponentInChildren<NearbyPuckScript>().EnablePush();
    }

    public void ActivateErratic()
    {
        erraticPowerup++;
        AddPowerupText("erratic");

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

    public void ActivatePlusThree()
    {
        plusThreePowerup++;
        AddPowerupText("plus three");

        IncrementPuckBonusValue(3);
    }
    #endregion

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

    public override string ToString()
    {
        return "IsPlayersPuck: " + IsPlayersPuck() +
            "\n puckBaseValue: " + puckBaseValue +
            "\n puckBonusValue: " + puckBonusValue +
            "\n zoneMultiplier: " + zoneMultiplier +
            "\n IsShot: " + IsShot() +
            "\n IsSafe: " + IsSafe() +
            "\n IsStopped: " + IsStopped();
    }
}
