// Script to control the predicted shot location on easy mode

using UnityEngine;

public class HaloScript : MonoBehaviour
{

    private LineScript line;
    private LogicScript logic;

    [SerializeField] float angleModifierX;
    [SerializeField] float angleModifierY;
    [SerializeField] float minusY;
    [SerializeField] float powerModifier;
    [SerializeField] float power;
    [SerializeField] float angle;

    private float sideModifier;
    private float sideModifier2;

    private bool debuggingInProgress = false;

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
        //sideModifier = 3.6f;
        //if (logic.LeftsTurn() && logic.player.isTurn || !logic.LeftsTurn() && logic.player.isTurn)
        //{
        //sideModifier = -3.6f;
        //}
        sideModifier2 = logic.LeftsTurn() ? (-3.6f) : (3.6f);

        if (debuggingInProgress) return;

        // calculate the pucks estimated position
        if (logic.activeBar == "angle")
        {
            this.transform.position = new Vector3(0.0f, -10.0f, 1.0f);
            angle = (float)((-line.value * 0.6) + 120);
            float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * angleModifierX;
            float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * angleModifierY;
            this.transform.position = new Vector3(xcomponent + sideModifier2, ycomponent - minusY, this.transform.position.z);
        }
        else if (logic.activeBar == "power")
        {
            power = (line.value - (line.value - 50) * 0.5f) * powerModifier;
            float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * angleModifierX * (powerModifier * power);
            float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * angleModifierY * (powerModifier * power);
            this.transform.position = new Vector3(xcomponent + sideModifier2, ycomponent - minusY, this.transform.position.z);
        }
    }

    // for debug menu
    public void SetHaloPostion(float angle, float power, float spin = 50)
    {
        debuggingInProgress = true;

        float haloAngel;
        this.transform.position = new Vector3(0.0f, -10.0f, 1.0f);
        haloAngel = (float)((-angle * 0.6) + 120);
        float angleXcomponent = Mathf.Cos(haloAngel * Mathf.PI / 180) * angleModifierX;
        float angleYcomponent = Mathf.Sin(haloAngel * Mathf.PI / 180) * angleModifierY;
        this.transform.position = new Vector3(angleXcomponent + sideModifier2, angleYcomponent - minusY, this.transform.position.z);

        float haloPower;
        haloPower = (power - (power - 50) * 0.5f) * powerModifier;
        float powerXcomponent = Mathf.Cos(haloAngel * Mathf.PI / 180) * angleModifierX * (powerModifier * haloPower);
        float powerYcomponent = Mathf.Sin(haloAngel * Mathf.PI / 180) * angleModifierY * (powerModifier * haloPower);
        this.transform.position = new Vector3(powerXcomponent + sideModifier2, powerYcomponent - minusY, this.transform.position.z);
    }
}
