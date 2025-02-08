// These path objects are used for hard CPU AI shots.
// it basically just keeps track of if any pucks block the path.
// as of A3 the CPU now has paths to knock player pucks out of scoring zones.

using System.Collections.Generic;
using UnityEngine;

public class CPUPathScript : MonoBehaviour, CPUPathInterface
{
    List<GameObject> pucksInPath = new();
    private List<GameObject> GetPucksInPath() { return pucksInPath; }

    [SerializeField] private float angle;
    [SerializeField] private float power;
    [SerializeField] private float spin;
    [SerializeField] private int value;
    [SerializeField] private bool isContactShot;
    [SerializeField] private bool requiresPhasePowerup;
    private bool requiresExplosionPowerup;

    public (float, float, float) GetPath() { return (angle, power, spin); }

    public bool DoesPathRequirePhasePowerup() { return requiresPhasePowerup; }

    public bool DoesPathRequireExplosionPowerup() { return requiresExplosionPowerup; }

    public bool IsPathAContactShot() { return isContactShot; }

    public int CalculateValue()
    {
        DisablePathVisualization();
        List<GameObject> pucksCurrentlyInPath = GetPucksInPath();
        int numberOfPucksCurrentlyInPath = pucksCurrentlyInPath.Count;
        requiresExplosionPowerup = false;

        // if not contact shot, only shoot if path is clear
        if (!isContactShot)
        {
            if (numberOfPucksCurrentlyInPath == 0)
            {
                return value;
            }
            else if (numberOfPucksCurrentlyInPath == 1 && pucksCurrentlyInPath[0].GetComponent<PuckScript>().IsPlayersPuck() && !pucksCurrentlyInPath[0].GetComponent<PuckScript>().IsHydra() && !pucksCurrentlyInPath[0].GetComponent<PuckScript>().IsResurrect() && !requiresPhasePowerup && !isContactShot) // explosion shot
            {
                requiresExplosionPowerup = true;
                return pucksCurrentlyInPath[0].GetComponent<PuckScript>().ComputeValue() - 2; // minus 2 because we need to consider the opportunity cost of exploding the CPU puck
            }
            else
            {
                return 0;
            }
        }
        // if contact shot, we only shoot if we can contact an opp puck to push it out
        else
        {
            if (numberOfPucksCurrentlyInPath < 1) return 0;
            // contact shot for a single puck if it's valuable enough
            else if (numberOfPucksCurrentlyInPath == 1 && IsPlayersPuck(pucksCurrentlyInPath[0]) && IsNotLockedOrExposionOrPhase(pucksCurrentlyInPath[0]) && HasPostiveValue(pucksCurrentlyInPath[0]) && !WallIsActive())
            {
                return pucksCurrentlyInPath[0].GetComponent<PuckScript>().ComputeValue();
            }
            // contact shot for any two pucks with positive value
            else if (numberOfPucksCurrentlyInPath > 1 & pucksCurrentlyInPath.TrueForAll(IsPlayersPuck) && pucksCurrentlyInPath.TrueForAll(IsNotLockedOrExposionOrPhase) && pucksCurrentlyInPath.TrueForAll(HasPostiveValue) && !WallIsActive())
            {
                return value;
            }

            return 0;
        }
    }

    // helper only for CalculateValue() contact shots
    private bool IsPlayersPuck(GameObject p)
    {
        if (p == null) return false;

        if (p.GetComponent<PuckScript>() == null) return false;

        return p.GetComponent<PuckScript>().IsPlayersPuck();
    }

    // helper only for CalculateValue() contact shots
    private bool IsNotLockedOrExposionOrPhase(GameObject p)
    {
        if (p == null) return false;

        if (p.GetComponent<PuckScript>() == null) return false;

        return !(p.GetComponent<PuckScript>().IsLocked() || p.GetComponent<PuckScript>().IsExplosion() || p.GetComponent<PuckScript>().IsPhase());
    }

    // helper only for CalculateValue() contact shots
    private bool HasPostiveValue(GameObject p)
    {
        if (p == null) return false;

        if (p.GetComponent<PuckScript>() == null) return false;

        return (p.GetComponent<PuckScript>().ComputeValue() > 0 || p.GetComponent<PuckScript>().IsFactory());
    }

    // helper only for CalculateValue() contact shots
    private bool WallIsActive()
    {
        return LogicScript.Instance.WallIsActive();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != 3) // ignore center puck collider
        {
            if (!pucksInPath.Contains(collision.gameObject))
            {
                pucksInPath.Add(collision.gameObject);
            }
#if (UNITY_EDITOR)
            List<GameObject> pucksCurrentlyInPath = GetPucksInPath();
            int numberOfPucksCurrentlyInPath = pucksCurrentlyInPath.Count;
            if (numberOfPucksCurrentlyInPath >= 1)
            {
                EnablePathVisualization(1);
            }
#endif
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != 3) // ignore center puck collider
        { 
            pucksInPath.Remove(collision.gameObject);
#if (UNITY_EDITOR)
            List<GameObject> pucksCurrentlyInPath = GetPucksInPath();
            int numberOfPucksCurrentlyInPath = pucksCurrentlyInPath.Count;
            if (numberOfPucksCurrentlyInPath < 1)
            {
                DisablePathVisualization();
            }
#endif
        }
    }

    // These two are only used to help me create CPU paths
    public void EnablePathVisualization(int mode = 0)
    {
#if (UNITY_EDITOR)
        if (UIManagerScript.Instance.debugMode <= 0) { return;  }
        GetComponent<LineRenderer>().enabled = true;
        if (mode == 0)
        {
            GetComponent<LineRenderer>().startColor = Color.green;
            GetComponent<LineRenderer>().endColor = Color.green;
            GetComponent<LineRenderer>().startWidth = 0.08f;
            GetComponent<LineRenderer>().endWidth = 0.08f;
        }
        else if (mode == 1)
        {
            GetComponent<LineRenderer>().startColor = Color.white;
            GetComponent<LineRenderer>().endColor = Color.white;
            GetComponent<LineRenderer>().startWidth = 0.03f;
            GetComponent<LineRenderer>().endWidth = 0.03f;
        }
#endif
    }
    public void DisablePathVisualization()
    {
#if (UNITY_EDITOR)
        GetComponent<LineRenderer>().enabled = false;
#endif
    }
}
