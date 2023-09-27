// These path objects are used for hard CPU AI shots
// it basically just keeps track of if any pucks block the path

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUPathScript : MonoBehaviour
{
    private List<GameObject> pucksInPath = new List<GameObject>();
    public List<GameObject> GetPucksInPath() { return pucksInPath; }

    public float angle;
    public float power;
    public float spin;
    public int value;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!pucksInPath.Contains(collision.gameObject)) { pucksInPath.Add(collision.gameObject); }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        pucksInPath.Remove(collision.gameObject);
    }
}
