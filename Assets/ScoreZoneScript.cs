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

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            puck = collision.gameObject.transform.parent.gameObject.GetComponent<PuckScript>();
            //puck.isSafe = true;
            //puck.zoneMultiplier = zoneMultiplier;
            puck.enterScoreZone(true, zoneMultiplier);
            logic.updateScores();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            puck = collision.gameObject.transform.parent.gameObject.GetComponent<PuckScript>();
            puck.exitScoreZone(boundary, zoneMultiplier);
            logic.updateScores();
        }
    }

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
