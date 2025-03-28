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
        // animation
        gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.6f);
        LeanTween.scale(gameObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setOnComplete(DisableSpriteRenderer);
    }

    private void DisableSpriteRenderer()
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.2980392f);
        if (auraEnabled)
        {
            return;
        }
        GetComponent<SpriteRenderer>().enabled = false;
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
