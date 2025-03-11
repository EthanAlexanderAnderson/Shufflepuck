using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    // self
    public static PlayerDataManager Instance;

    string[] intKeys = {
        "puck", "easyHighscore", "mediumHighscore", "hardHighscore",
        "win", "loss", "tie",
        "onlineWin", "onlineLoss", "onlineTie",
        "localWin", "localLoss", "localTie",
        "easyWin", "easyLoss", "easyTie",
        "mediumWin", "mediumLoss", "mediumTie",
        "hardWin", "hardLoss", "hardTie",
        "DailyChallenge1", "DailyChallenge2", "OngoingChallenge",
        "PlinkoReward", "PlinkoPegsDropped", "WelcomeBonus", "Streak", "XP",
        "puck33unlocked", "puck34unlocked", "puck35unlocked", "puck36unlocked", "puck37unlocked", "puck38unlocked", "puck39unlocked", "puck40unlocked", "puck41unlocked", "puck42unlocked", "puck43unlocked", "puck44unlocked"
        };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    public void TestSaveButtonHelper() { TestSave(); }
    public async Task TestSave()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        try
        {
            var playerData = new Dictionary<string, object>{
            {"testKey", "a text value"},
        };
            await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
            Debug.Log("Test save successful");
        }
        catch (System.Exception e)
        {
            Debug.Log($"Test save failed {e}");
        }
    }

    public void CheckIfSaveAllIsNecessaryButtonHelper() { CheckIfSaveAllIsNecessary(); }
    public async Task CheckIfSaveAllIsNecessary()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        // because easyWin should be the first player pref written, we know we have to save data to cloud if it exists locally but not in cloud
        var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "easyWin" });

        if (results.TryGetValue("easyWin", out var value))
        {
            try
            {
                int easyWin = value.Value.GetAs<int>(); // Convert JSON object to int
                if (easyWin > 0 && PlayerPrefs.GetInt("easyWin") == 0)
                {
                    Debug.Log("Prior backup found, attempting to load data...");
                    await LoadAllData();
                }
                else
                {
                    if (PlayerPrefs.GetInt("easyWin") > 0)
                    {
                        Debug.Log("No prior backup, attempting to save all...");
                        await SaveAllData();
                    }
                }
            }
            catch (CloudSaveException e)
            {
                Debug.LogError($"Failed to convert 'puck' to int: {e.Message}");
            }
        }
        else
        {
            if (PlayerPrefs.GetInt("easyWin") > 0)
            {
                Debug.Log("No prior backup, attempting to save all...");
                await SaveAllData();
            }
        }
    }

    public void SaveAllDataButtonHelper() { SaveAllData(); }
    public async Task SaveAllData()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var playerData = new Dictionary<string, object>();

        // Add only existing integer keys
        foreach (string key in intKeys)
        {
            if (PlayerPrefs.HasKey(key))
            {
                playerData[key] = PlayerPrefs.GetInt(key);
            }
        }

        // Handle string keys
        string[] stringKeys = { "LastChallengeDate" };
        foreach (string key in stringKeys)
        {
            if (PlayerPrefs.HasKey(key))
            {
                playerData[key] = PlayerPrefs.GetString(key);
            }
        }

        if (playerData.Count > 0) // Save only if there's data to save
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
            Debug.Log("Player data successfully saved to Cloud Save.");
        }
        else
        {
            Debug.Log("No valid player data found to save.");
        }
    }

    // for now, we just load puck playerpref as test
    public void LoadAllDataButtonHelper() { LoadAllData(); }
    public async Task LoadAllData()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        // TODO: daily quest date, main quest progress

        foreach (string key in intKeys)
        {
            await LoadDataInt(key);
        }

        ProcessLoadedData();
    }

    // generic load for ints
    private async Task LoadDataInt(string key)
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

        if (results.TryGetValue(key, out var outValue))
        {
            try
            {
                int valueFromFile = outValue.Value.GetAs<int>(); // Convert JSON object to int
                Debug.Log($"Loaded data: {key} = {valueFromFile}");
                PlayerPrefs.SetInt(key, valueFromFile);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError($"Failed to convert {key} to int: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"{key} key not found in Cloud Save.");
        }
    }

    // generic save for ints
    private async Task SaveDataInt(string key, int value)
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

        if (results.TryGetValue(key, out var outValue))
        {
            try
            {
                int valueFromFile = outValue.Value.GetAs<int>(); // Convert JSON object to int
                Debug.Log($"Loaded data: {key} = {valueFromFile}");
                PlayerPrefs.SetInt(key, valueFromFile);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError($"Failed to convert {key} to int: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"{key} key not found in Cloud Save.");
        }
    }

    // this is for doing any extra work to apply loaded data to the game state
    private void ProcessLoadedData()
    {
        // set puck skin
        PuckSkinManager.Instance.SelectPlayerPuckSprite(PlayerPrefs.GetInt("puck") == 0 && PlayerPrefs.GetInt("hardHighscore") <= 5 ? 1 : PlayerPrefs.GetInt("puck"));

        // load challenges
        DailyChallengeManagerScript.Instance.SetText();
        OngoingChallengeManagerScript.Instance.SetText();

        // XP
        LevelManager.Instance.LoadXP();
    }
}