// These path objects are used for hard CPU AI shots
// it basically just keeps track of if any pucks block the path

using System.Collections.Generic;
using UnityEngine;

public class CPUPathScript : MonoBehaviour
{
    List<GameObject> pucksInPath = new();
    public List<GameObject> GetPucksInPath() { return pucksInPath; }

    [SerializeField] float angle;
    [SerializeField] float power;
    [SerializeField] float spin;
    public int value;

    public (float, float, float) GetPath() { return (angle, power, spin); }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!pucksInPath.Contains(collision.gameObject)) { pucksInPath.Add(collision.gameObject); }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        pucksInPath.Remove(collision.gameObject);
    }
}
