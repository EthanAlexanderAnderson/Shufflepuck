using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugWindow : EditorWindow
{
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

        Button sethalo = new() { text = "SET HALO" };
        sethalo.clicked += () => HaloScript.Instance.SetHaloPostion(angleFloatField.value, powerFloatField.value, spinFloatField.value, left.value);
        rootVisualElement.Add(sethalo);

        // destroy all pucks button
        Button destroy = new() { text = "DESTROY ALL PUCKS" };
        destroy.clicked += () => PuckManager.Instance.ClearAllPucks();
        rootVisualElement.Add(destroy);

        Button addPlayer = new() { text = "addPlayer" };
        addPlayer.clicked += () => ServerLogicScript.Instance.AddPlayerServerRpc(0, true);
        rootVisualElement.Add(addPlayer);

        Button printPlayerPrefs = new() { text = "printPlayerPrefs" };
        printPlayerPrefs.clicked += () => PrintPlayerPrefs();
        rootVisualElement.Add(printPlayerPrefs);

        Button setDateAs0001 = new() { text = "setDateAs0001" };
        setDateAs0001.clicked += () => SetDateAs0001();
        rootVisualElement.Add(setDateAs0001);

        Button setDateAsYesterday = new() { text = "setDateAsYesterday" };
        setDateAsYesterday.clicked += () => SetDateAsYesterday();
        rootVisualElement.Add(setDateAsYesterday);

        Button setDateAs4HoursAgo = new() { text = "setDateAs4HoursAgo" };
        setDateAs4HoursAgo.clicked += () => SetDateAs4HoursAgo();
        rootVisualElement.Add(setDateAs4HoursAgo);

        Button setDateAsTomorrow = new() { text = "setDateAsTomorrow" };
        setDateAsTomorrow.clicked += () => SetDateAsTomorrow();
        rootVisualElement.Add(setDateAsTomorrow);

        Button setDateAsNull = new() { text = "setDateAsNull" };
        setDateAsNull.clicked += () => SetDateAsNull();
        rootVisualElement.Add(setDateAsNull);

        Button resetAllProgress = new() { text = "resetAllProgress" };
        resetAllProgress.clicked += () => ResetAllProgress();
        rootVisualElement.Add(resetAllProgress);

        Button reGenerateChallenges = new() { text = "reGenerateChallenges" };
        reGenerateChallenges.clicked += () => ReGenerateChallenges();
        rootVisualElement.Add(reGenerateChallenges);

        // simulate game win
        IntegerField diff = new IntegerField();
        diff.label = "Difficulty";
        diff.value = 0;
        rootVisualElement.Add(diff);

        IntegerField winBy = new IntegerField();
        winBy.label = "Win By";
        winBy.value = 1;
        rootVisualElement.Add(winBy);

        Button simulateGameWin = new() { text = "simulate game win" };
        simulateGameWin.clicked += () => SimulateGameWin(diff.value, winBy.value);
        rootVisualElement.Add(simulateGameWin);
    }

    public void DebugShootNew(float angleParameter, float powerParameter, float spinParameter)
    {
        puck = LogicScript.Instance.puckPrefab;
        puckObject = Instantiate(puck, new Vector3((left.value ? -3.6f : 3.6f), -10.0f, 0.0f), Quaternion.identity);
        puckScript = puckObject.GetComponent<PuckScript>();
        puckScript.InitPuck(true, 1);
        puckScript.Shoot(angleParameter, powerParameter, spinParameter);
    }

    public void DebugShootAll(float angleParameter, float powerParameter, float spinParameter) {
        var objects = GameObject.FindGameObjectsWithTag("puck");
        foreach (var obj in objects)
        {
            obj.GetComponent<PuckScript>().Shoot(angleParameter, powerParameter, spinParameter);
        }

        puck = LogicScript.Instance.puckPrefab;
        puckObject = Instantiate(puck, new Vector3((left.value ? -3.6f : 3.6f), -10.0f, 0.0f), Quaternion.identity);
        puckScript = puckObject.GetComponent<PuckScript>();
        puckScript.InitPuck(true, 1);
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
        log += "darkMode: " + PlayerPrefs.GetInt("darkMode") + "\n";

        log += "tutorialCompleted: " + PlayerPrefs.GetInt("tutorialCompleted") + "\n";

        log += "LastChallengeDate: " + PlayerPrefs.GetString("LastChallengeDate") + "\n";
        log += "DailyChallenge1: " + PlayerPrefs.GetInt("DailyChallenge1") + "\n";
        log += "DailyChallenge2: " + PlayerPrefs.GetInt("DailyChallenge2") + "\n";
        log += "LastPlinkoRewardDate: " + PlayerPrefs.GetInt("LastPlinkoRewardDate") + "\n";
        log += "PlinkoReward: " + PlayerPrefs.GetInt("PlinkoReward") + "\n";

        Debug.Log(log);

        PowerupCardData.LogRarityStats();

        log = "";
        log += "holo: " + PowerupCardData.GetCardOwnedSum(-1) + "\n";
        log += "standard: " + PowerupCardData.GetCardOwnedSum(-2) + "\n";
        log += "bronze: " + PowerupCardData.GetCardOwnedSum(-3) + "\n";
        log += "gold: " + PowerupCardData.GetCardOwnedSum(-4) + "\n";
        log += "diamond: " + PowerupCardData.GetCardOwnedSum(-5) + "\n";
        log += "celestial: " + PowerupCardData.GetCardOwnedSum(-6) + "\n";
        log += "different: " + PowerupCardData.GetCardOwnedSum(-7) + "\n";
        log += "any: " + PowerupCardData.GetCardOwnedSum(-8) + "\n";

        Debug.Log(log);
    }

    private void SetDateAs0001()
    {
        PlayerPrefs.SetString("LastChallengeDate", "0001-01-01");
        PlayerPrefs.SetString("LastPlinkoRewardDate", "0001-01-01");
        PlayerPrefs.SetString("LastDailyWinDate", "0001-01-01");
        PlayerPrefs.SetString("LastPackBoosterDate", "0001-01-01");
    }

    private void SetDateAsYesterday()
    {
        PlayerPrefs.SetString("LastChallengeDate", DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
        PlayerPrefs.SetString("LastPlinkoRewardDate", DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
        PlayerPrefs.SetString("LastDailyWinDate", DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
        PlayerPrefs.SetString("LastPackBoosterDate", DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));
        StreakManager.Instance.IncrementStreak();
    }

    private void SetDateAs4HoursAgo()
    {
        PlayerPrefs.SetString("LastChallengeDate", DateTime.Now.AddHours(-4).ToString());
        StreakManager.Instance.IncrementStreak();
    }

    private void SetDateAsTomorrow()
    {
        var currentSavedDate = PlayerPrefs.GetString("LastChallengeDate", DateTime.Today.ToString("yyyy-MM-dd"));
        var tomorrow = DateTime.Parse(currentSavedDate).AddDays(1);
        PlayerPrefs.SetString("LastChallengeDate", tomorrow.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetString("LastPlinkoRewardDate", tomorrow.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetString("LastDailyWinDate", tomorrow.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetString("LastPackBoosterDate", tomorrow.ToString("yyyy-MM-dd"));
    }

    private void SetDateAsNull()
    {
        PlayerPrefs.DeleteKey("LastChallengeDate");
        PlayerPrefs.DeleteKey("LastPlinkoRewardDate");
        PlayerPrefs.DeleteKey("LastDailyWinDate");
        PlayerPrefs.DeleteKey("LastPackBoosterDate");
    }

    private void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey("easyHighscore");
        PlayerPrefs.DeleteKey("mediumHighscore");
        PlayerPrefs.DeleteKey("hardHighscore");
        PlayerPrefs.DeleteKey("win");
        PlayerPrefs.DeleteKey("loss");
        PlayerPrefs.DeleteKey("tie");
        PlayerPrefs.DeleteKey("onlineWin");
        PlayerPrefs.DeleteKey("onlineLoss");
        PlayerPrefs.DeleteKey("onlineTie");
        PlayerPrefs.DeleteKey("easyWin");
        PlayerPrefs.DeleteKey("easyLoss");
        PlayerPrefs.DeleteKey("easyTie");
        PlayerPrefs.DeleteKey("mediumWin");
        PlayerPrefs.DeleteKey("mediumLoss");
        PlayerPrefs.DeleteKey("mediumTie");
        PlayerPrefs.DeleteKey("hardWin");
        PlayerPrefs.DeleteKey("hardTie");
        PlayerPrefs.DeleteKey("DailyChallenge1");
        PlayerPrefs.DeleteKey("DailyChallenge2");
        PlayerPrefs.DeleteKey("OngoingChallenge");
        PlayerPrefs.DeleteKey("Streak");
        PlayerPrefs.DeleteKey("PackBooster");
        PlayerPrefs.DeleteKey("StandardPacks");
        PlayerPrefs.DeleteKey("PlusPacks");
        PlayerPrefs.DeleteKey("WelcomeBonus");
        PlayerPrefs.DeleteKey("XP");
        PlayerPrefs.DeleteKey("CraftingCredits");
        PlayerPrefs.DeleteKey("PlinkoPegsDropped");
        PlayerPrefs.DeleteKey("PlinkoSkinsUnlocked");
        PlayerPrefs.DeleteKey("puck");
        PlayerPrefs.DeleteKey("ShowNewSkinAlert");
        PlayerPrefs.DeleteKey("PlinkoReward");
        PlayerPrefs.DeleteKey("Deck1");

        SetDateAsNull();
    }

    private void ReGenerateChallenges()
    {
        ChallengeManager.Instance.ReGenerateDailyChallenges();
    }

    private void SimulateGameWin(int diff, int winBy)
    {
        int playerScore = 0;
        int opponentScore = 0;
        bool isOnline = (diff < 0);
        if (winBy > 0) { playerScore = winBy; }
        else if (winBy < 0) { opponentScore = winBy; }

        UIManagerScript.Instance.UpdateGameResult(playerScore, opponentScore, diff, false, isOnline);
    }
}