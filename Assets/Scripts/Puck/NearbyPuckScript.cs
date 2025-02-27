using System.Collections.Generic;
using UnityEngine;

public class NearbyPuckScript : MonoBehaviour
{
    List<GameObject> pucksInPath = new();
    bool auraEnabled = false;
    int auraCount = 0;

    bool isPlayersPuck = false;

    public void EnableAura()
    {
        auraEnabled = true;
        auraCount++;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<CircleCollider2D>().enabled = true;
        isPlayersPuck = gameObject.transform.parent.gameObject.GetComponent<PuckScript>().IsPlayersPuck();
    }

    public void EnablePush()
    {
        GetComponent<CircleCollider2D>().enabled = true;
    }

    public void TriggerPush()
    {
        foreach(GameObject puck in pucksInPath)
        {
            var rb = puck.GetComponent<Rigidbody2D>();
            // push each puck away from the center
            rb.AddForce((puck.transform.position - transform.position).normalized*5, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != 3 && collision.gameObject.CompareTag("puck")) // ignore center puck collider & nearby collider
        {
            if (!pucksInPath.Contains(collision.gameObject))
            {
                pucksInPath.Add(collision.gameObject);
                if (auraEnabled)
                {
                    var puckScript = collision.GetComponent<PuckScript>();
                    if (puckScript.IsPlayersPuck() == isPlayersPuck)
                    {
                        puckScript.IncrementPuckBonusValue(auraCount);
                        LogicScript.Instance.UpdateScores();
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != 3 && collision.gameObject.CompareTag("puck")) // ignore center puck collider & nearby collider
        {
            if (pucksInPath.Contains(collision.gameObject))
            {
                pucksInPath.Remove(collision.gameObject);
                if (auraEnabled)
                {
                    var puckScript = collision.GetComponent<PuckScript>();
                    if (puckScript.IsPlayersPuck() == isPlayersPuck)
                    {
                        puckScript.IncrementPuckBonusValue(-auraCount);
                        LogicScript.Instance.UpdateScores();
                    }
                }
            }
        }
    }
}
