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
    [SerializeField] private float value;
    [SerializeField] private bool requiresPhasePowerup;
    private bool requiresExplosionPowerup;

    public (float, float, float) GetPath() { return (angle, power, spin); }

    public bool DoesPathRequirePhasePowerup() { return requiresPhasePowerup; }

    public bool DoesPathRequireExplosionPowerup() { return requiresExplosionPowerup; }

    public float CalculateValue(int modifiedDifficulty)
    {
        DisablePathVisualization();
        List<GameObject> pucksCurrentlyInPath = GetPucksInPath();
        int numberOfPucksCurrentlyInPath = pucksCurrentlyInPath.Count;
        requiresExplosionPowerup = false;

        // if not contact shot, only shoot if path is clear
        if (numberOfPucksCurrentlyInPath == 0 && !requiresPhasePowerup)
        {
            return modifiedDifficulty < 2 ? 0 : value; // don't consider regular paths for easy/medium with no powerups
        }
        // explosion shot
        else if (numberOfPucksCurrentlyInPath == 1 && pucksCurrentlyInPath[0].GetComponent<PuckScript>().IsPlayersPuck() && !pucksCurrentlyInPath[0].GetComponent<PuckScript>().HasHydra() && !pucksCurrentlyInPath[0].GetComponent<PuckScript>().HasResurrect() && !requiresPhasePowerup)
        {
            requiresExplosionPowerup = true;
            if (CPUBehaviorScript.HasExplosion())
            {
                return pucksCurrentlyInPath[0].GetComponent<PuckScript>().ComputeTotalFutureValue() - 2; // minus 2 because we need to consider the opportunity cost of exploding the CPU puck
            }
            else
            {
                return 0;
            }
        }
        else if (numberOfPucksCurrentlyInPath == 0 && requiresPhasePowerup && CPUBehaviorScript.HasPhase())
        {
            requiresPhasePowerup = true;
            return value;
        }
        else
        {
            return 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != 3 && collision.gameObject.CompareTag("puck")) // ignore center puck collider & nearby collider
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
        if (collision.gameObject.layer != 3 && collision.gameObject.CompareTag("puck")) // ignore center puck collider & nearby collider
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
        if (UIManagerScript.Instance.debugMode <= 0 && mode > 0) { return;  }
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
