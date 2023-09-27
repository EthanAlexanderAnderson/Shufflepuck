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
        logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();
    }

    // when a puck enters, trigger it's function
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            puck = collision.gameObject.transform.parent.gameObject.GetComponent<PuckScript>();
            puck.enterScoreZone(true, zoneMultiplier);
            logic.updateScores();
        }
    }

    // when a puck exits, trigger it's function
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            puck = collision.gameObject.transform.parent.gameObject.GetComponent<PuckScript>();
            puck.exitScoreZone(boundary, zoneMultiplier);
            logic.updateScores();
        }
    }

    // set the puck multiplier in the zone 
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            puck = collision.gameObject.transform.parent.gameObject.GetComponent<PuckScript>();
            if (puck.getZoneMultiplier() < zoneMultiplier && puck.transform.position.y < 15.75)
            {
                puck.setZoneMultiplier(zoneMultiplier);
            }
        }
    }
}
