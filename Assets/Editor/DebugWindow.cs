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

    public Toggle allPucks;
    public Toggle left;

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
        angleFloatField.value = 50f;
        rootVisualElement.Add(angleFloatField);

        FloatField powerFloatField = new FloatField();
        rootVisualElement.Add(powerFloatField);
        powerFloatField.value = 50f;
        powerFloatField.label = "Power";

        FloatField spinFloatField = new FloatField();
        rootVisualElement.Add(spinFloatField);
        spinFloatField.value = 50f;
        spinFloatField.label = "Spin";

        // checkbox for if all pucks should be shot
        allPucks = new Toggle("shoot all pucks");
        rootVisualElement.Add(allPucks);

        // checkbox for if all pucks should be shot
        left = new Toggle("shoot from left side");
        rootVisualElement.Add(left);

        Button shoot = new() { text = "SHOOT" };
        shoot.clicked += () => DebugShootNew(angleFloatField.value, powerFloatField.value, spinFloatField.value);
        rootVisualElement.Add(shoot);

        
        Button shootall = new() { text = "SHOOT ALL" };
        shootall.clicked += () => DebugShootAll(angleFloatField.value, powerFloatField.value, spinFloatField.value);
        rootVisualElement.Add(shootall);

        // destroy all pucks button
        Button destroy = new() { text = "DESTROY ALL PUCKS" };
        destroy.clicked += () => PuckManager.Instance.ClearAllPucks();
        rootVisualElement.Add(destroy);

        Button addPlayer = new() { text = "addPlayer" };
        addPlayer.clicked += () => serverLogic.AddPlayerServerRpc(0);
        rootVisualElement.Add(addPlayer);

        Button printPlayerPrefs = new() { text = "printPlayerPrefs" };
        printPlayerPrefs.clicked += () => PrintPlayerPrefs();
        rootVisualElement.Add(printPlayerPrefs);


    }

    public void DebugShootNew(float angleParameter, float powerParameter, float spinParameter)
    {
        puckObject = Instantiate(puck, new Vector3((left.value ? -3.6f : 3.6f), -10.0f, 0.0f), Quaternion.identity);
        puckScript = puckObject.GetComponent<PuckScript>();
        puckScript.InitPuck(true, playerPuckSprite);
        puckScript.Shoot(angleParameter, powerParameter, spinParameter);
    }

    public void DebugShootAll(float angleParameter, float powerParameter, float spinParameter) {
        var objects = GameObject.FindGameObjectsWithTag("puck");
        foreach (var obj in objects)
        {
            obj.GetComponent<PuckScript>().Shoot(angleParameter, powerParameter, spinParameter);
        }

        puckObject = Instantiate(puck, new Vector3((left.value ? -3.6f : 3.6f), -10.0f, 0.0f), Quaternion.identity);
        puckScript = puckObject.GetComponent<PuckScript>();
        puckScript.InitPuck(true, playerPuckSprite);
        puckScript.Shoot(angleParameter, powerParameter, spinParameter);
    }

    private void PrintPlayerPrefs()
    {
        var log = "";

        log += "easyHighscore: " + PlayerPrefs.GetInt("easyHighscore") + "\n";
        log += "mediumHighscore: " + PlayerPrefs.GetInt("mediumHighscore") + "\n";
        log += "hardHighscore: " + PlayerPrefs.GetInt("hardHighscore") + "\n";

        log += "win: " + PlayerPrefs.GetInt("win") + "\n";
        log += "loss: " + PlayerPrefs.GetInt("loss") + "\n";
        log += "tie: " + PlayerPrefs.GetInt("tie") + "\n";

        log += "onlineWin: " + PlayerPrefs.GetInt("onlineWin") + "\n";
        log += "onlineLoss: " + PlayerPrefs.GetInt("onlineLoss") + "\n";
        log += "onlineTie: " + PlayerPrefs.GetInt("onlineTie") + "\n";

        log += "localWin: " + PlayerPrefs.GetInt("localWin") + "\n";
        log += "localLoss: " + PlayerPrefs.GetInt("localLoss") + "\n";
        log += "localTie: " + PlayerPrefs.GetInt("localTie") + "\n";

        log += "easyWin: " + PlayerPrefs.GetInt("easyWin") + "\n";
        log += "easyLoss: " + PlayerPrefs.GetInt("easyLoss") + "\n";
        log += "easyTie: " + PlayerPrefs.GetInt("easyTie") + "\n";

        log += "mediumWin: " + PlayerPrefs.GetInt("mediumWin") + "\n";
        log += "mediumLoss: " + PlayerPrefs.GetInt("mediumLoss") + "\n";
        log += "mediumTie: " + PlayerPrefs.GetInt("mediumTie") + "\n";

        log += "hardWin: " + PlayerPrefs.GetInt("hardWin") + "\n";
        log += "hardLoss: " + PlayerPrefs.GetInt("hardLoss") + "\n";
        log += "hardTie: " + PlayerPrefs.GetInt("hardTie") + "\n";

        log += "FPS: " + PlayerPrefs.GetInt("FPS") + "\n";
        log += "Puck: " + PlayerPrefs.GetInt("puck") + "\n";
        log += "SelectedTrack: " + PlayerPrefs.GetInt("SelectedTrack") + "\n";
        log += "ShowNewSkinAlert: " + PlayerPrefs.GetInt("ShowNewSkinAlert") + "\n";
        log += "MusicVolume: " + PlayerPrefs.GetFloat("MusicVolume") + "\n";
        log += "SFXVolume: " + PlayerPrefs.GetFloat("SFXVolume") + "\n";

        Debug.Log(log);
    }
}