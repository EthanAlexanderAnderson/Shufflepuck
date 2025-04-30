using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PlayerAuthentication : MonoBehaviour
{
    public static PlayerAuthentication Instance;

    private string username;
    private string id;
    private string imgURL;

    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject titleScreenBackground;
    [SerializeField] private Sprite titleScreenDark;
    [SerializeField] private Sprite titleScreenBackgroundDark;

    public (string username, string id, string imgURL) GetProfile()
    {
        return (username, id, imgURL);
    }

    private async void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        LoadingSceneDarkMode();

        await InitializeUnityServices();

        await SignInAnonymously();
    }

    async Task InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
        }
    }

    private async Task SignInAnonymously()
    {
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Player is already signed in.");
            }
            else
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed In Anon as {AuthenticationService.Instance.PlayerId}");
            }

            if (AuthenticationService.Instance.IsSignedIn)
            {
                await PlayerDataManager.Instance.SyncWithCloudIfNeeded();
            }
            else
            {
                Debug.LogError("Sync Error: Couldn't Sign In.");
            }
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("Authentication failed: " + ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Request failed: " + ex.Message);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"An unexpected error occurred during sign in: {e.Message}");
        }

        if (loadingText != null && loadingText.gameObject != null)
        {
            loadingText.text = "loading game...";
        }
        SceneManager.LoadScene("SampleScene");
    }

    private void LoadingSceneDarkMode()
    {
        try
        {
            int darkMode = PlayerPrefs.GetInt("darkMode", 0);
            if (darkMode == 1)
            {
                if (titleScreen != null && titleScreenDark != null)
                {
                    titleScreen.GetComponent<Image>().sprite = titleScreenDark;
                }
                if (titleScreenBackground != null && titleScreenBackgroundDark != null)
                {
                    titleScreenBackground.GetComponent<Image>().sprite = titleScreenBackgroundDark;
                }
            }
        }
        catch (System.Exception)
        {
            Debug.Log("Failed to set dark mode on LoadingScene");
        }
    }
}
