using System.Collections.Generic;
using System.Linq;
//using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugWindow : EditorWindow
{
    private LogicScript logic;
    private ServerLogicScript serverLogic;
    public GameObject puck;

    public float angle;
    public float power;
    public float spin;

    private GameObject puckObject;
    private PuckScript puckScript;

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
        serverLogic = GameObject.FindGameObjectWithTag("logic").GetComponent<ServerLogicScript>();
        puck = logic.puckPrefab;
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
        shoot.clicked += () => DebugShoot(angleFloatField.value, powerFloatField.value, spinFloatField.value);
        rootVisualElement.Add(shoot);

        Button shootRpc = new Button { text = "SHOOT RPC" };
        shootRpc.clicked += () => serverLogic.DebugShootServerRpc(angleFloatField.value, powerFloatField.value, spinFloatField.value);
        rootVisualElement.Add(shootRpc);

        Button addPlayer = new Button { text = "addPlayer" };
        addPlayer.clicked += () => serverLogic.AddPlayerServerRpc();
        rootVisualElement.Add(addPlayer);

        //serverLogicScript.AddPlayerServerRpc(); 
    }

    public void DebugShoot(float angleParameter, float powerParameter, float spinParameter)
    {
        var objects = GameObject.FindGameObjectsWithTag("puck");
        foreach (var obj in objects)
        {
            obj.GetComponent<PuckScript>().Shoot(angleParameter, powerParameter, spinParameter);
        }

        puckObject = Instantiate(puck, new Vector3(0.0f, -10.0f, 0.0f), Quaternion.identity);
        puckScript = puckObject.GetComponent<PuckScript>();
        puckScript.InitPuck(true, playerPuckSprite);
        puckScript.Shoot(angleParameter, powerParameter, spinParameter);
    }


}