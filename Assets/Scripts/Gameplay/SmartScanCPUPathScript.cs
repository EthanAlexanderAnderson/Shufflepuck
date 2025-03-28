using System.Collections.Generic;
using UnityEngine;

public class SmartScanCPUPathScript : MonoBehaviour, CPUPathInterface
{
    
    [SerializeField] private GameObject anchorPoint;

    [SerializeField] private float scanDistance = 30f; // Max raycast distance
    [SerializeField] private LayerMask puckLayer;     // Which layer to raycast on

    private float bestAngle = 0f;
    private int highestValue = 0;
    float powerModifier = 0;
    int valueModifier = 0;

    // for gizmos
    private bool drawGizmos = true;
    private List<Vector3> hitPoints = new();
    private List<Vector3> gizRayOrigin = new();
    private List<Vector3> gizDirection = new();

    bool powerupsEnabled;

    public (float, float, float) GetPath() => (((180f - bestAngle) - 60f) * 1.66666f, System.Math.Min(95f + powerModifier, 105), 50f); // convert angel to line-readable format
    public bool DoesPathRequirePhasePowerup() => false;
    public bool DoesPathRequireExplosionPowerup() => false;
    public bool IsPathAContactShot() => powerModifier >= 5.0f; // use forcefield powerup if total shot power is 100 or above

    public int CalculateValue()
    {
        if (WallIsActive()) { return 0; }

        powerupsEnabled = PlayerPrefs.GetInt("PowerupsEnabled", 0) == 1;

        highestValue = 0;
        bestAngle = 0;
        powerModifier = 0;
        valueModifier = 0;

        // these 3 lists are just for gizmos
        gizRayOrigin.Clear();
        gizDirection.Clear();
        hitPoints.Clear();

        GameObject[] allPucks = GameObject.FindGameObjectsWithTag("puck");

        foreach (GameObject puck in allPucks)
        {
            if (puck.transform.position.y < 0) continue;  // Ignore pucks below y = 0

            PuckScript puckScript = puck.GetComponent<PuckScript>();
            if (puckScript == null || !IsValidPuck(puckScript)) continue;

            int puckValue = puckScript.ComputeValue();
            Vector3 puckPosition = puck.transform.position;

            if (puckValue > 0)
            {
                bool isVisible;
                float tempPowerModifier;
                int tempValueModifier;
                (isVisible, tempPowerModifier, tempValueModifier) = CheckPuckVisibility(puck);
                if (isVisible)
                {
                    float angle = GetAngleToAnchor(puckPosition);
                    if ((puckValue + tempValueModifier) > (highestValue + valueModifier))
                    {
                        highestValue = puckValue;
                        valueModifier = tempValueModifier;
                        bestAngle = angle;
                        powerModifier = tempPowerModifier;
                    }
                }
            }
        }

        // Apply the 180-degree mirror to the best angle
        bestAngle = (bestAngle + 180f) % 360f;

        // make sure we have good values
        if (highestValue <= 0 || (highestValue * 3 + valueModifier) <= 0) { return 0; } // this is just here so we don't Debug.Log for 0 value paths
        if (bestAngle < 60 || bestAngle > 120) { Debug.LogError("SMART SCAN ERROR: BAD ANGLE " + bestAngle); return 0; }

        // temporary nerf for non-powerups mode. (don't shoot unless two or more opponent pucks are in path, 25% chance override)
        float randomValue = Random.Range(0f, 1f);
        // if powerups are disabled AND only 1 puck in path : 70% chance to nerf
        // if above conditions AND the path value is less than 10 : 90% chance to nerf
        if (!powerupsEnabled && powerModifier <= 0 && randomValue <= (0.70 + ((highestValue * 3 + valueModifier) < 10 ? 0.2 : 0)))
        {
            return 0;
        }

        // return best puck
        Debug.Log($"Smart Scan Value: {highestValue * 3} + {valueModifier} at Angle: {bestAngle}");
        return highestValue * 3 + valueModifier;
    }

    private bool IsValidPuck(PuckScript puck)
    {
        return puck.IsPlayersPuck() && !puck.IsLocked() && !puck.IsExplosion();
    }

    private (bool, float, int) CheckPuckVisibility(GameObject puck)
    {
        float tempPowerModifier = 0;
        int tempValueModifier = 0;
        Vector3[] offsets = { new(-1f, 0, 0), new(1f, 0, 0) }; // 1 for puck width
        List<Vector3> directions = new();
        List<Vector3> rayOrigins = new();

        // two offsets, left and right side
        foreach (var offset in offsets)
        {
            Vector3 puckPos = puck.transform.position + offset;
            Vector3 anchorPos = anchorPoint.transform.position + offset;

            directions.Add((anchorPos - puckPos).normalized); // in front of target puck
            rayOrigins.Add(puckPos + directions[^1]); // use the direction we just added in the last line

            directions.Add((puckPos - anchorPos).normalized); // behind target puck
            rayOrigins.Add(puckPos + directions[^1]); // use the direction we just added in the last line
        }

        // for gizmos
        for (int i = 0; i < directions.Count; i++)
        {
            gizRayOrigin.Add(rayOrigins[i]);
            gizDirection.Add(directions[i]);
        }

        // evaluate all our hits (collisions of raycast on pucks)
        for (int i = 0; i < directions.Count; i++)
        {
            foreach (var hit in Physics2D.RaycastAll(rayOrigins[i], directions[i], scanDistance, puckLayer))
            {
                // Ignore non-puck collisions, self-hit, and opponent's active puck
                if (!hit.collider.CompareTag("puck") || hit.collider.gameObject == puck ||
                    hit.collider.gameObject == LogicScript.Instance.opponent.activePuckObject)
                    continue;

                // for forward facing raycasts (in front of target puck)
                if (i % 2 == 0)
                {
                    var puckScript = hit.collider.GetComponent<PuckScript>();
                    if (IsValidPuck(puckScript)) { tempPowerModifier += 2.5f; tempValueModifier += puckScript.ComputeValue(); continue; }
                }

                // for backwards facing raycasts (behind target puck)
                if (i % 2 == 1)
                {
                    var puckScript = hit.collider.GetComponent<PuckScript>();
                    if (puckScript.GetZoneMultiplier() <= 0) continue; // Ignore pucks in the offzone
                    if (!puckScript.IsLocked()) // locked puck behind prevents knockout
                    {
                        if (puckScript.IsPlayersPuck())
                        {
                            tempValueModifier += puckScript.ComputeValue();
                        }
                        else // it's okay to have CPU pucks behind, it just makes the path less valuable
                        {
                            tempValueModifier -= (puckScript.ComputeValue() * 4);
                        }
                        tempPowerModifier += 2.5f;
                        continue;
                    }
                }

                hitPoints.Add(hit.point);
                return (false, 0, 0); // Puck is blocking
            }
        }
        return (true, tempPowerModifier, tempValueModifier); // No pucks are blocking
    }

    private float GetAngleToAnchor(Vector3 puckPosition)
    {
        Vector3 direction = (anchorPoint.transform.position - puckPosition).normalized;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // highlist hit points in red
        Gizmos.color = Color.red;
        foreach (var hit in hitPoints)
        {
            Gizmos.DrawSphere(hit, 0.2f);
        }

        // draw scan lines from puck
        for (int i = 0; i < gizDirection.Count; i++)
        {
            Debug.DrawRay(gizRayOrigin[i], gizDirection[i] * scanDistance);
        }
    }

    // helper only for CalculateValue() contact shots
    private bool WallIsActive()
    {
        return LogicScript.Instance.WallIsActive();
    }

    public void EnablePathVisualization(int mode = 0)
    {
        drawGizmos = true;
    }
    public void DisablePathVisualization()
    {
        drawGizmos = false;
    }
}
