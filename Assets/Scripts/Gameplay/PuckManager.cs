using System;
using UnityEngine;

public class PuckManager : MonoBehaviour
{
    public static PuckManager Instance;

    private PuckScript pucki;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    public void CreatePuck(bool isPlayersPuck)
    {
        return;
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
}