using System;
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

            if (velocity >= 1 || puck.transform.position.y < -9)
            {
                return false;
            }
        }
        return true;
    }

}
