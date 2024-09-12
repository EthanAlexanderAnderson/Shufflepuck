// Script to control the predicted shot location on easy mode

using UnityEngine;

public class HaloScript : MonoBehaviour
{
    // self
    public static HaloScript Instance;

    // dependancies
    private LineScript line;
    private LogicScript logic;

    [SerializeField] private float angleModifierX; // 20
    [SerializeField] private float angleModifierY; // 20
    [SerializeField] private float minusY; // 10
    [SerializeField] private float powerModifier; // 0.1396
    [SerializeField] private float power;
    [SerializeField] private float angle;

    private float sideModifier;

    private bool debuggingInProgress = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    // Start is called before the first frame update
    void Start()
    {
        line = LineScript.Instance;
        logic = LogicScript.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (debuggingInProgress) return;

        sideModifier = logic.LeftsTurn() ? (-3.6f) : (3.6f);

        // calculate the pucks estimated position
        if (logic.activeBar == "angle")
        {
            this.transform.position = new Vector3(0.0f, -10.0f, 1.0f);
            angle = (float)((-line.GetValue() * 0.6) + 120);
            float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * angleModifierX;
            float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * angleModifierY;
            this.transform.position = new Vector3(xcomponent + sideModifier, ycomponent - minusY, this.transform.position.z);
        }
        else if (logic.activeBar == "power")
        {
            power = (line.GetValue() - (line.GetValue() - 50) * 0.5f) * powerModifier;
            float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * angleModifierX * (powerModifier * power);
            float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * angleModifierY * (powerModifier * power);
            this.transform.position = new Vector3(xcomponent + sideModifier, ycomponent - minusY, this.transform.position.z);
        }
    }

    // for debug menu
    public void SetHaloPostion(float angle, float power, float spin = 50, bool isLeft = false)
    {
        debuggingInProgress = true;

        sideModifier = isLeft ? (-3.6f) : (3.6f);

        float haloAngel;
        this.transform.position = new Vector3(0.0f, -10.0f, 1.0f);
        haloAngel = (float)((-angle * 0.6) + 120);
        float angleXcomponent = Mathf.Cos(haloAngel * Mathf.PI / 180) * angleModifierX;
        float angleYcomponent = Mathf.Sin(haloAngel * Mathf.PI / 180) * angleModifierY;
        this.transform.position = new Vector3(angleXcomponent + sideModifier, angleYcomponent - minusY, this.transform.position.z);

        float haloPower;
        haloPower = (power - (power - 50) * 0.5f) * powerModifier;
        float powerXcomponent = Mathf.Cos(haloAngel * Mathf.PI / 180) * angleModifierX * (powerModifier * haloPower);
        float powerYcomponent = Mathf.Sin(haloAngel * Mathf.PI / 180) * angleModifierY * (powerModifier * haloPower);
        this.transform.position = new Vector3(powerXcomponent + sideModifier, powerYcomponent - minusY, this.transform.position.z);
    }
}
