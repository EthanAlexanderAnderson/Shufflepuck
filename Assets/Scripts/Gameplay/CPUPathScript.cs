// These path objects are used for hard CPU AI shots.
// it basically just keeps track of if any pucks block the path.
// as of A3 the CPU now has paths to knock player pucks out of scoring zones.

using System.Collections.Generic;
using UnityEngine;

public class CPUPathScript : MonoBehaviour
{
    List<GameObject> pucksInPath = new();
    private List<GameObject> GetPucksInPath() { return pucksInPath; }

    [SerializeField] private float angle;
    [SerializeField] private float power;
    [SerializeField] private float spin;
    [SerializeField] private int value;
    [SerializeField] private bool isContactShot;

    public (float, float, float) GetPath() { return (angle, power, spin); }

    public int CalculateValue()
    {
        List<GameObject> pucksCurrentlyInPath = GetPucksInPath();
        int numberOfPucksCurrentlyInPath = pucksCurrentlyInPath.Count;

        // if not contact shot, only shoot if path is clear
        if (!isContactShot)
        {
            if (numberOfPucksCurrentlyInPath == 0)
            {
                return value;
            }
            else
            {
                return 0;
            }
        }
        // if contact shot, we only shoot if we can contact an opp puck to push it out
        else
        {
            if (numberOfPucksCurrentlyInPath <=1) return 0;

            if (pucksCurrentlyInPath.TrueForAll(IsPlayersPuck))
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != 3) // ignore center puck collider
        {
            if (!pucksInPath.Contains(collision.gameObject)) { pucksInPath.Add(collision.gameObject); }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != 3) // ignore center puck collider
        { 
            pucksInPath.Remove(collision.gameObject);
        }
    }

    // These two are only used to help me create CPU paths
    public void EnablePathVisualization()
    {
#if (UNITY_EDITOR)
        GetComponent<LineRenderer>().enabled = true;
#endif
    }
    public void DisablePathVisualization()
    {
#if (UNITY_EDITOR)
        GetComponent<LineRenderer>().enabled = false;
#endif
    }
}
