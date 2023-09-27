// Script to control the predicted shot location on easy mode

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaloScript : MonoBehaviour
{

    private LineScript line;
    private LogicScript logic;

    public float angleModifierX;
    public float angleModifierY;
    public float minusY;
    public float powerModifier;
    public float power;
    public float angle;

    public bool angleSet;
    public bool powerSet;

    // Start is called before the first frame update
    void Start()
    {
        line = GameObject.FindGameObjectWithTag("line").GetComponent<LineScript>();
        logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();

        this.transform.position = new Vector3(0.0f, 7.0f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        // calculate the pucks estimated position
        if (logic.activeBar == "angle")
        {
            this.transform.position = new Vector3(0.0f, -10.0f, 1.0f);
            angle = (float)((-line.value * 0.6) + 120);
            float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * angleModifierX;
            float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * angleModifierY;
            this.transform.position = new Vector3(xcomponent, ycomponent - minusY, this.transform.position.z);
        }
        else if (logic.activeBar == "power")
        {
            power = (line.value - (line.value - 50) * 0.5f) * powerModifier;
            float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * angleModifierX * (powerModifier * power);
            float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * angleModifierY * (powerModifier * power);
            this.transform.position = new Vector3(xcomponent, ycomponent - minusY, this.transform.position.z);
        }
    }
}
