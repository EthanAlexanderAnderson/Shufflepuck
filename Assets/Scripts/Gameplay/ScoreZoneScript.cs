/* Each scoring zone has one of these scripts,
 * and a collider to detect pucks.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreZoneScript : MonoBehaviour
{
    public bool boundary;
    public int zoneMultiplier;

    private PuckScript puck;
    private LogicScript logic;

    // Start is called before the first frame update
    void Start()
    {
        logic = LogicScript.Instance;
    }

    // when a puck enters, trigger it's function
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            puck = collision.gameObject.transform.parent.gameObject.GetComponent<PuckScript>();
            puck.EnterScoreZone(true, zoneMultiplier);
            logic.UpdateScores();
        }
    }

    // when a puck exits, trigger it's function
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            puck = collision.gameObject.transform.parent.gameObject.GetComponent<PuckScript>();
            puck.ExitScoreZone(boundary, zoneMultiplier);
            logic.UpdateScores();
        }
    }

    // set the puck multiplier in the zone 
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            puck = collision.gameObject.transform.parent.gameObject.GetComponent<PuckScript>();
            if (puck.GetZoneMultiplier() < zoneMultiplier && puck.transform.position.y < 15.75)
            {
                puck.SetZoneMultiplier(zoneMultiplier);
            }
        }
    }
}
