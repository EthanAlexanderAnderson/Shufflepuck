using System;
using Unity.Netcode;
using UnityEngine;

public class PuckManager : MonoBehaviour
{
    public static PuckManager Instance;

    private PuckScript pucki;
    public GameObject puckPrefab; // default puck prefab

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    public int GetPucksInPlayCount(bool getOnlyPucksWithValue = false, int owner = 0) // owner 1 = player & owner -1 = CPU & owner 0 = anyone
    {
        int count = 0;

        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            pucki = puck.GetComponent<PuckScript>();
            if (pucki.IsShot())
            {
                // skip based on parameters
                if (getOnlyPucksWithValue && pucki.ComputeValue() <= 0) continue;
                if (owner == -1 && pucki.IsPlayersPuck()) continue;
                if (owner == 1 && !pucki.IsPlayersPuck()) continue;

                count++;
            }
        }

        return count;
    }

    public void CreatePuck(Competitor competitor)
    {
        float xpos = (competitor.isPlayer ? -3.6f : 3.6f);
        competitor.activePuckObject = Instantiate(puckPrefab, new Vector3(xpos, -10.0f, -1.0f), Quaternion.identity);
        competitor.activePuckScript = competitor.activePuckObject.GetComponent<PuckScript>();
        competitor.activePuckScript.InitPuck(competitor.isPlayer, competitor.puckSpriteID);
    }

    public (int,int) UpdateScores()
    {
        int playerSum = 0;
        int opponentSum = 0;
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            pucki = puck.GetComponent<PuckScript>();
            if (pucki.IsPlayersPuck())
            {
                playerSum += pucki.ComputeValue();
            }
            else
            {
                opponentSum += pucki.ComputeValue();
            }
        }
        return (playerSum, opponentSum);
    }

    // helper for AllPucksAreSlowed, AllPucksAreSlowedMore, AllPucksAreStopped
    private bool CheckPucks(Func<PuckScript, bool> condition)
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            var pucki = puck.GetComponent<PuckScript>();
            if (pucki.IsShot() && !condition(pucki))
            {
                return false;
            }
        }
        return true;
    }

    public bool AllPucksAreSlowed()
    {
        return CheckPucks(puck => puck.IsSlowed());
    }

    public bool AllPucksAreSlowedMore()
    {
        return CheckPucks(puck => puck.IsSlowedMore());
    }

    public bool AllPucksAreStopped()
    {
        return CheckPucks(puck => puck.IsStopped());
    }

    public void CleanupDeadPucks()
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            pucki = puck.GetComponent<PuckScript>();
            // If puck has been shot and is stopped, but is not safe, destroy it
            if (pucki.IsShot() && pucki.IsStopped() && !pucki.IsSafe())
            {
                Destroy(puck);
            }
        }
    }

    public void ClearAllPucks()
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            Destroy(puck);
        }
    }

    // this special function is needed for the ClientLogicScript, because it cannot call functions on pucks it doesn't own
    public bool AllPucksAreSlowedClient()
    {
        var allPucks = GameObject.FindGameObjectsWithTag("puck");
        foreach (var puck in allPucks)
        {
            var puckScript = puck.GetComponent<PuckScript>();
            if (puckScript == null) continue;
            if (puckScript.velocityNetworkedRounded == null) continue;

            var velocity = puckScript.velocityNetworkedRounded.Value;

            if (velocity >= 1.0f || puck.transform.position.y < -9)
            {
                return false;
            }
        }
        return true;
    }

    // using this for ending the game, thank you chatgpt for doing the math stuff :)))
    public bool AllPucksAreNotNearingScoreZoneEdge()
    {
        GameObject[] pucks = GameObject.FindGameObjectsWithTag("puck");
        GameObject[] scoreZones = GameObject.FindGameObjectsWithTag("scoreZone");

        foreach (GameObject puckObj in pucks)
        {
            CircleCollider2D puckCenterCollider = puckObj.transform.GetChild(0).GetComponent<CircleCollider2D>();
            Rigidbody2D rb = puckObj.GetComponent<Rigidbody2D>();
            if (puckCenterCollider == null || rb == null)
                continue;

            Vector2 center = puckCenterCollider.transform.TransformPoint(puckCenterCollider.offset);
            float radius = puckCenterCollider.radius * puckCenterCollider.transform.lossyScale.x;
            float velocity = rb.velocity.magnitude;

            float minDistance = float.MaxValue;

            foreach (GameObject zoneObj in scoreZones)
            {
                Vector2 closestPoint = center;
                float distance = float.MaxValue;

                if (zoneObj.TryGetComponent(out PolygonCollider2D poly))
                {
                    distance = ClosestDistanceToPolygon(center, poly, out closestPoint);
                }
                else if (zoneObj.TryGetComponent(out BoxCollider2D box))
                {
                    distance = ClosestDistanceToBox(center, box, out closestPoint);
                }
                else if (zoneObj.TryGetComponent(out CircleCollider2D zoneCircle))
                {
                    if (zoneObj.transform.parent.gameObject == puckObj) continue; // don't consider pucks own Aura collider (nearby collider)
                    distance = ClosestDistanceToCircle(center, zoneCircle, out closestPoint);
                }

                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            float distanceToExit = minDistance - radius;
            if (distanceToExit < (velocity * 2))
            {
                return false;
            }
        }

        return true;
    }

    private float ClosestDistanceToPolygon(Vector2 point, PolygonCollider2D poly, out Vector2 closest)
    {
        Vector2[] points = poly.points;
        Transform t = poly.transform;

        float minDist = float.MaxValue;
        closest = point;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 a = t.TransformPoint(points[i]);
            Vector2 b = t.TransformPoint(points[(i + 1) % points.Length]);

            Vector2 c = ClosestPointOnSegment(point, a, b);
            float dist = Vector2.Distance(point, c);
            if (dist < minDist)
            {
                minDist = dist;
                closest = c;
            }
        }

        return minDist;
    }

    private float ClosestDistanceToBox(Vector2 point, BoxCollider2D box, out Vector2 closest)
    {
        // Get world-space corners
        Transform t = box.transform;
        Vector2 size = Vector2.Scale(box.size, t.lossyScale);
        Vector2 center = t.TransformPoint(box.offset);

        Vector2[] corners = new Vector2[4];
        Vector2 right = 0.5f * size.x * t.right;
        Vector2 up = 0.5f * size.y * t.up;
        corners[0] = center + right + up;
        corners[1] = center - right + up;
        corners[2] = center - right - up;
        corners[3] = center + right - up;

        float minDist = float.MaxValue;
        closest = point;

        for (int i = 0; i < 4; i++)
        {
            Vector2 a = corners[i];
            Vector2 b = corners[(i + 1) % 4];

            Vector2 c = ClosestPointOnSegment(point, a, b);
            float dist = Vector2.Distance(point, c);
            if (dist < minDist)
            {
                minDist = dist;
                closest = c;
            }
        }

        return minDist;
    }

    private float ClosestDistanceToCircle(Vector2 point, CircleCollider2D circle, out Vector2 closest)
    {
        Transform t = circle.transform;
        Vector2 center = t.TransformPoint(circle.offset);
        float radius = circle.radius * t.lossyScale.x;

        Vector2 dir = (point - center).normalized;
        closest = center + dir * radius;

        return Vector2.Distance(point, closest);
    }

    private Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = point - a;
        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / ab.sqrMagnitude);
        return a + t * ab;
    }
}
