using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        "CraftingCredits", "PackBooster", "StandardPacks", "PlusPacks"
    };

    string[] stringKeys = {
        "LastChallengeDate", "LastPlinkoRewardDate", "LastDailyWinDate", "LastPackBoosterDate",
        "PlinkoSkinsUnlocked",
        "Deck1", "Deck2", "Deck3", "Deck4", "Deck5", "CardCollection",
        "WonUsing"
    };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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

    public void SyncWithCloudIfNeededButtonHelper() { SyncWithCloudIfNeeded(); }
    public async Task SyncWithCloudIfNeeded()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        // set username and id in cloud
        try
        {
            var loginData = new Dictionary<string, object>
            {
                { "username", PlayerAuthentication.Instance.GetProfile().username },
                { "platformID", PlayerAuthentication.Instance.GetProfile().id }
            };
            await CloudSaveService.Instance.Data.Player.SaveAsync(loginData);
            Debug.Log("Player data successfully saved to Cloud Save.");
        }
        catch (System.Exception e)
        {
            Debug.Log($"loginData save failed {e}");
        }

        PlinkoDataSwap();

        // because easyWin should be the first player pref written, we know we have to save data to cloud if it exists locally but not in cloud
        var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "easyWin" });

        if (results.TryGetValue("easyWin", out var value))
        {
            try
            {
                int easyWin = value.Value.GetAs<int>(); // Convert JSON object to int
                if (easyWin > 0 && PlayerPrefs.GetInt("easyWin") == 0)
                {
                    Debug.Log("Prior backup found, attempting to load data from cloud...");
                    await LoadAllData();
                }
                else
                {
                    if (PlayerPrefs.GetInt("easyWin") > 0)
                    {
                        Debug.Log("Attempting to save all data to cloud...");
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
                Debug.Log("No prior backup, attempting to save all data to cloud...");
                await SaveAllData();
            }
        }

        SceneManager.LoadScene("SampleScene");
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

        foreach (string key in stringKeys)
        {
            await LoadDataString(key);
        }
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

    // generic load for strings
    private async Task LoadDataString(string key)
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

        if (results.TryGetValue(key, out var outValue))
        {
            try
            {
                string valueFromFile = outValue.Value.GetAs<string>(); // Convert JSON object to int
                Debug.Log($"Loaded data: {key} = {valueFromFile}");
                PlayerPrefs.SetString(key, valueFromFile);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError($"Failed to convert {key} to string: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"{key} key not found in Cloud Save.");
        }
    }

    // generic save for ints
    private async Task SaveDataInt(string key)
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var playerData = new Dictionary<string, object>();

        playerData[key] = PlayerPrefs.GetInt(key);

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
            Debug.Log("Player data successfully saved to Cloud Save.");
        }
        catch (CloudSaveException e)
        {
            Debug.LogError($"Failed to save {key} : {e.Message}");
        }
    }

    // generic save for ints
    private async Task SaveDataString(string key)
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var playerData = new Dictionary<string, object>();

        playerData[key] = PlayerPrefs.GetString(key);

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
            Debug.Log("Player data successfully saved to Cloud Save.");
        }
        catch (CloudSaveException e)
        {
            Debug.LogError($"Failed to save {key} : {e.Message}");
        }
    }

    // plinko data storage swap function
    public void PlinkoDataSwap()
    {
        // change data storage of plinko skin unlocks if needed
        if (!PlayerPrefs.HasKey("PlinkoSkinsUnlocked"))
        {
            List<int> unlocked = new();
            for (int i = 33; i < 45; i++)
            {
                if (PlayerPrefs.GetInt("puck" + i.ToString() + "unlocked") == 1)
                {
                    unlocked.Add(i);
                }
            }
            string plinkoSkinsUnlockedString = string.Join(",", unlocked);
            PlayerPrefs.SetString("PlinkoSkinsUnlocked", plinkoSkinsUnlockedString);
            PlayerPrefs.Save();
        }
    }
}