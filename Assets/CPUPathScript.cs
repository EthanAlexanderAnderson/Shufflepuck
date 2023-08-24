using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUPathScript : MonoBehaviour
{

    public int count;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        count = pucksInPath.Count;
    }

    //private List<GameObject>
    private List<GameObject> pucksInPath = new List<GameObject>();
    public List<GameObject> GetPucksInPath() { return pucksInPath; }

    public float angle;
    public float power;
    public float spin;
    public int value;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("TRIGGER ENTER");
        if (!pucksInPath.Contains(collision.gameObject)) { pucksInPath.Add(collision.gameObject); }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        pucksInPath.Remove(collision.gameObject);
    }
}
