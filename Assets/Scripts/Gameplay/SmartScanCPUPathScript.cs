using System.Collections.Generic;
using UnityEngine;

public class SmartScanCPUPathScript : MonoBehaviour, CPUPathInterface
{
    
    [SerializeField] private GameObject anchorPoint;

    [SerializeField] private float scanDistance = 30f; // Max raycast distance
    [SerializeField] private LayerMask puckLayer;     // Which layer to raycast on

    int highestValue = 0;
    int valueModifier = 0;
    float bestAngle = 0f;
    float powerModifier = 0;

    // for gizmos
    private bool drawGizmos = true;
    private List<Vector3> hitPoints = new();
    private List<Vector3> gizRayOrigin = new();
    private List<Vector3> gizDirection = new();

    public (float, float, float) GetPath() => (((180f - bestAngle) - 60f) * 1.66666f, System.Math.Min(95f + powerModifier, 102.5f), 50f); // convert angel to line-readable format
    public bool DoesPathRequirePhasePowerup() => false;
    public bool DoesPathRequireExplosionPowerup() => false; // TODO: requires explosion IF theres no pucks in front and there are 3+ pucks behind

    public float CalculateValue(int modifiedDifficulty)
    {
        if (WallIsActive()) { return 0; }
        if (modifiedDifficulty < 0) { modifiedDifficulty = 0; }

        highestValue = 0;
        valueModifier = 0;
        bestAngle = 0;
        powerModifier = 0;

        // these 3 lists are just for gizmos
        gizRayOrigin.Clear();
        gizDirection.Clear();
        hitPoints.Clear();

        GameObject[] allPucks = GameObject.FindGameObjectsWithTag("puck");

        foreach (GameObject puck in allPucks)
        {
            if (puck.transform.position.y < 0) continue;  // Ignore pucks below y = 0 because they should be destroyed by the cleanup

            PuckScript puckScript = puck.GetComponent<PuckScript>();
            if (puckScript == null) continue;
            if (!puckScript.IsPlayersPuck() || puckScript.IsLocked()) continue;

            int puckValue = puckScript.ComputeTotalFutureValue();

            Vector3 puckPosition = puck.transform.position;

            if (puckValue > 0)
            {
                bool isVisible;
                float tempPowerModifier;
                int tempValueModifier;
                (isVisible, tempPowerModifier, tempValueModifier) = CheckPuckVisibility(puck);
                if (isVisible)
                {
                    float angle = GetAngleToAnchor(puckPosition);puck.GetComponent<PuckScript>();
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
        if (highestValue <= 0 || (highestValue + valueModifier) <= 0) { return 0; } // this is just here so we don't Debug.Log for 0 value paths
        if (bestAngle < 60 || bestAngle > 120) { Debug.LogError("SMART SCAN ERROR: BAD ANGLE " + bestAngle); return 0; }

        // nerf based on modifiedDifficulty. (lower modifiedDifficulty = greater nerf). also additional nerf for earlier shots.
        int diffNerf = Mathf.Max(0, (int)Mathf.Pow(2f, 4 - modifiedDifficulty) + (LogicScript.Instance.opponent.puckCount - 3));

        // return best puck, highest value times two because hitting out the opponent's puck + CPU ending up there is net value of double
        Debug.Log($"Smart Scan Value: {highestValue + valueModifier - diffNerf}: ({highestValue} + {valueModifier} - {diffNerf}) at Angle: {bestAngle}");
        return highestValue + valueModifier - diffNerf;
    }

    private bool IsValidPuck(PuckScript puck)
    {
        return puck.IsPlayersPuck() && !puck.IsLocked() && !puck.IsExplosion();
    }

    private (bool, float, int) CheckPuckVisibility(GameObject puck)
    {
        PuckScript targetPuckScript = puck.GetComponent<PuckScript>();
        // CPU puck should end up vaguely where the hitout puck was (unless the target puck will explode the shot puck), so start the shot value with that
        int tempValueModifier = !targetPuckScript.IsExplosion() ? targetPuckScript.GetZoneMultiplier() : 0;
        float tempPowerModifier = 0;

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
                // Ignore non-puck collisions, self-hit, and CPU's active puck
                if (!hit.collider.CompareTag("puck") || hit.collider.gameObject == puck ||
                    hit.collider.gameObject == LogicScript.Instance.opponent.activePuckObject)
                    continue;

                hitPoints.Add(hit.point);

                // for forward facing raycasts (in front of target puck)
                if (i % 2 == 0)
                {
                    var puckScript = hit.collider.GetComponent<PuckScript>();
                    // if there is a lock or explosion in front, the path is blocked
                    if (puckScript.IsLocked() || puckScript.IsExplosion())
                    {
                        return (false, 0, 0); // Puck is blocking
                    }
                    // if there is a players puck in front, add more power to the shot
                    else if (puckScript.IsPlayersPuck())
                    {
                        tempPowerModifier += 2.5f;
                        tempValueModifier += puckScript.ComputeTotalFutureValue() - 1; // minus 1 here, so we slightly favor aiming towards the front puck
                    }
                    // if there is a CPU puck in front, make the path slightly less valuable
                    else
                    {
                        tempValueModifier -= puckScript.ComputeTotalFutureValue() / 2;
                    }
                    continue;
                }

                // for backwards facing raycasts (behind target puck)
                // TODO: ignore pucks behind if the target puck has explosion
                if (i % 2 == 1)
                {
                    var puckScript = hit.collider.GetComponent<PuckScript>();
                    if (puckScript.GetZoneMultiplier() <= 0) continue; // Ignore pucks in the offzone
                    if (!puckScript.IsLocked()) // locked puck behind prevents knockout
                    {
                        if (puckScript.IsPlayersPuck())
                        {
                            tempPowerModifier += 2.5f;
                            tempValueModifier += puckScript.ComputeTotalFutureValue();
                        }
                        else // it's okay to have CPU pucks behind, it just makes the path less valuable
                        {
                            tempValueModifier -= (puckScript.ComputeTotalFutureValue());
                        }
                        tempPowerModifier += 2.5f;
                        continue;
                    }
                }
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
