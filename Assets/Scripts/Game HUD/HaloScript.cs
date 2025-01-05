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
    [SerializeField] private float fart = 0.13f;
    [SerializeField] private float poop = 0.0009f;

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

        if (!ClientLogicScript.Instance.isRunning) // offline
        {
            sideModifier = LogicScript.Instance.activeCompetitor.isPlayer ? (-3.6f) : (3.6f);
        }
        else // online
        {
            sideModifier = ClientLogicScript.Instance.client.goingFirst ? (-3.6f) : (3.6f);
        }

        var sr = GetComponent<SpriteRenderer>();

        // calculate the pucks estimated position
        if (logic.activeBar == "angle" || (ClientLogicScript.Instance.isRunning && ClientLogicScript.Instance.activeBar == "angle"))
        {
            this.transform.position = new Vector3(0.0f, -10.0f, 1.0f);
            angle = (float)((-line.GetValue() * 0.6) + 120);
            float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * angleModifierX;
            float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * angleModifierY;
            this.transform.position = new Vector3(xcomponent + sideModifier, ycomponent - minusY, this.transform.position.z);
            if (sr != null) { sr.color = new Color(1.0f, 1.0f, 1.0f, 1.0f); } // remove yellow tint
        }
        else if (logic.activeBar == "power" || (ClientLogicScript.Instance.isRunning && ClientLogicScript.Instance.activeBar == "power"))
        {
            power = (line.GetValue() - (line.GetValue() - 50) * 0.5f) * powerModifier;
            if (line.GetValue() >= 90)
            {
                // TODO: get feedback and uncomment this
                //power += (line.GetValue() - 95) * tempBoostPowerMod + tempBoostPowerModBase;
                if (sr != null) { sr.color = new Color(1.0f, 1.0f, 1.0f - 0.15f * (line.GetValue() - 90), 1.0f); } // tint halo yellow for extra high power shot
            }

            float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * angleModifierX * (powerModifier * power);
            float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * angleModifierY * (powerModifier * power);
            this.transform.position = new Vector3(xcomponent + sideModifier, ycomponent - minusY, this.transform.position.z);
        }
        else if (logic.activeBar == "spin" || (ClientLogicScript.Instance.isRunning && ClientLogicScript.Instance.activeBar == "spin"))
        {
            var spin = line.GetValue();
            var spinAngle = (float)((((-spin * 0.6) + 120) * (fart + power * 0.02)) - (90 * (fart + power * 0.02)));
            var spinPower = (float)(System.Math.Abs((spin - 50)) * poop);

            float xcomponent = Mathf.Cos((angle + spinAngle) * Mathf.PI / 180) * angleModifierX * (powerModifier * power - spinPower);
            float ycomponent = Mathf.Sin((angle + spinAngle) * Mathf.PI / 180) * angleModifierY * (powerModifier * power - spinPower);
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
        if (power >= 95)
        {
            haloPower += (power - 95) * tempBoostPowerMod + tempBoostPowerModBase;
        }

        var spinAngle = (float)((((-spin * 0.6) + 120) * (fart + haloPower * 0.02)) - (90 * (fart + haloPower * 0.02)));
        var spinPower = (float)(System.Math.Abs((spin - 50)) * poop);

        float powerXcomponent = Mathf.Cos((haloAngel + spinAngle) * Mathf.PI / 180) * angleModifierX * (powerModifier * haloPower - spinPower);
        float powerYcomponent = Mathf.Sin((haloAngel + spinAngle) * Mathf.PI / 180) * angleModifierY * (powerModifier * haloPower - spinPower);
        this.transform.position = new Vector3(powerXcomponent + sideModifier, powerYcomponent - minusY, this.transform.position.z);
    }

    public float tempBoostPowerMod; // 0.85
    public float tempBoostPowerModBase; // 0.7
}
