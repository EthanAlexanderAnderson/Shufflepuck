using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugWindow : EditorWindow
{
    private LogicScript logic;
    public GameObject puck;

    public float angle;
    public float power;
    public float spin;

    private GameObject PuckObject;
    private PuckScript PuckScript;

    public Sprite playerPuckSprite;

    [MenuItem("Window/Debug Window")]
    public new static void Show()
    {
        // This method is called when the user selects the menu item in the Editor
        EditorWindow wnd = GetWindow<DebugWindow>();
        wnd.titleContent = new GUIContent("Debug Window");
    }

    public void CreateGUI()
    {
        logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();
        puck = logic.puck;
        FloatField angleFloatField = new FloatField();
        angleFloatField.label = "Angle";
        rootVisualElement.Add(angleFloatField);

        FloatField powerFloatField = new FloatField();
        rootVisualElement.Add(powerFloatField);
        powerFloatField.label = "Power";

        FloatField spinFloatField = new FloatField();
        rootVisualElement.Add(spinFloatField);
        spinFloatField.label = "Spin";

        Button shoot = new Button { text = "SHOOT" };
        //shoot.text = "SHOOT";
        shoot.clicked += () => DebugShoot(angleFloatField.value, powerFloatField.value, spinFloatField.value);
        rootVisualElement.Add(shoot);
    }

    public void DebugShoot(float angleParameter, float powerParameter, float spinParameter)
    {
        var objects = GameObject.FindGameObjectsWithTag("puck");
        foreach (var obj in objects)
        {
            obj.GetComponent<PuckScript>().shoot(angleParameter, powerParameter, spinParameter);
        }

        PuckObject = Instantiate(puck, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
        PuckScript = PuckObject.GetComponent<PuckScript>();
        PuckScript.initPuck(true, playerPuckSprite);
        PuckScript.shoot(angleParameter, powerParameter, spinParameter);
    }

    //private PuckScript pucki;
    public void destroyAllPucks()
    {
        var objects = GameObject.FindGameObjectsWithTag("puck");
        foreach (var obj in objects)
        {
            //obj.GetComponent<PuckScript>().shoot(angleParameter, powerParameter, spinParameter);
        }
    }
}