using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;
using System;
using UnityEngine.Networking;

public class PlayerAuthentication : MonoBehaviour
{
    public static PlayerAuthentication Instance;

    private string username;
    private string id;
    private string imgURL;

    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text progressText;
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

        SetLoadingText("checking connection...");
        if (!await HasInternetConnection())
        {
            Debug.LogWarning("No internet connection.");
            SetLoadingText("loading game...");
            SceneManager.LoadScene("SampleScene");
            return;
        }

        SetLoadingText("starting online services...");
        await InitializeUnityServices();

        if (GameServices.IsAvailable())
        {
            GameServices.Authenticate();
        }
        else
        {
            Debug.LogError("GameServices is not available");
            await SignInAnonymously();
            SetLoadingText("loading game...");
            SceneManager.LoadScene("SampleScene");
        }
    }

    private void OnEnable()
    {
        // register for events
        GameServices.OnAuthStatusChange += OnAuthStatusChange;
    }

    private void OnDisable()
    {
        // unregister from events
        GameServices.OnAuthStatusChange -= OnAuthStatusChange;
    }

    private async void OnAuthStatusChange(GameServicesAuthStatusChangeResult result, Error error)
    {
        if (error != null || result.AuthStatus != LocalPlayerAuthStatus.Authenticated)
        {
            Debug.LogError("Failed to authenticate with Game Services");
            await SignInAnonymously();
            SetLoadingText("loading game...");
            SceneManager.LoadScene("SampleScene");
            return;
        }

        string localPlayer = result.LocalPlayer.Identifier;
        username = result.LocalPlayer.DisplayName;
        Debug.Log("Username: " + username);
        id = result.LocalPlayer.Identifier;
        Debug.Log("Identifier: " + username);

        // Link / Sign in
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                // Already signed in anonymously: now link
                if (Application.platform == RuntimePlatform.Android)
                {
                    Debug.Log("Linking with GooglePlayGames...");
                    SetLoadingText("linking with GooglePlayGames...");
                    string androidServerAuthCode = await GetGooglePlayGamesServerAuthCodeAsync();
                    await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(androidServerAuthCode);
                    Debug.Log($"Linked with GooglePlayGames as {AuthenticationService.Instance.PlayerId}");
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    Debug.Log("Linking with AppleGameCenter...");
                    SetLoadingText("linking with AppleGameCenter...");
                    var (appleSignature, appleTeamPlayerId, applePublicKeyURL, appleSalt, appleTimestamp) = await GetAppleCredentialsAsync(localPlayer);
                    await AuthenticationService.Instance.LinkWithAppleGameCenterAsync(appleSignature, appleTeamPlayerId, applePublicKeyURL, appleSalt, appleTimestamp);
                    Debug.Log($"Linked with AppleGameCenter as {AuthenticationService.Instance.PlayerId}");
                }
            }
            else
            {
                // Not signed in yet: sign in directly
                if (Application.platform == RuntimePlatform.Android)
                {
                    Debug.Log("Signing In with GooglePlayGames...");
                    SetLoadingText("signing in with GooglePlayGames...");
                    string androidServerAuthCode = await GetGooglePlayGamesServerAuthCodeAsync();
                    await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(androidServerAuthCode);
                    Debug.Log($"Signed In with GooglePlayGames as {AuthenticationService.Instance.PlayerId}");
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    Debug.Log("Signing In with AppleGameCenter...");
                    SetLoadingText("signing in with AppleGameCenter...");
                    var (appleSignature, appleTeamPlayerId, applePublicKeyURL, appleSalt, appleTimestamp) = await GetAppleCredentialsAsync(localPlayer);
                    await AuthenticationService.Instance.SignInWithAppleGameCenterAsync(appleSignature, appleTeamPlayerId, applePublicKeyURL, appleSalt, appleTimestamp);
                    Debug.Log($"Signed In with AppleGameCenter as {AuthenticationService.Instance.PlayerId}");
                }
            }

            // Now sync cloud data
            if (AuthenticationService.Instance.IsSignedIn)
            {
                SetLoadingText("syncing with cloud...");
                await PlayerDataManager.Instance.SyncWithCloudIfNeeded();
            }
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("Auth error: " + ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Request failed: " + ex.Message);
        }

        // if native login fails, try anon login
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await SignInAnonymously();
        }
        SetLoadingText("loading game...");
        SceneManager.LoadScene("SampleScene");
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

    private Task<string> GetGooglePlayGamesServerAuthCodeAsync()
    {
        var tcs = new TaskCompletionSource<string>();

        GameServices.LoadServerCredentials((result, error) =>
        {
            if (error == null && result?.ServerCredentials?.AndroidProperties != null)
            {
                string code = result.ServerCredentials.AndroidProperties.ServerAuthCode;
                Debug.Log("ServerAuthCode: " + code);
                tcs.SetResult(code);
            }
            else
            {
                tcs.SetException(new Exception("Failed to get ServerAuthCode"));
            }
        });

        return tcs.Task;
    }

    private Task<(string signature, string playerId, string publicKeyUrl, string salt, ulong timestamp)> GetAppleCredentialsAsync(string localPlayerId)
    {
        var tcs = new TaskCompletionSource<(string signature, string playerId, string publicKeyUrl, string salt, ulong timestamp)>();

        GameServices.LoadServerCredentials((result, error) =>
        {
            if (error == null && result?.ServerCredentials?.IosProperties != null)
            {
                var props = result.ServerCredentials.IosProperties;

                Debug.Log("Apple Signature: " + props.Signature);
                Debug.Log("Salt: " + props.Salt);
                Debug.Log("Timestamp: " + props.Timestamp);

                tcs.SetResult((
                    signature: props.Signature.ToString(),
                    playerId: localPlayerId,
                    publicKeyUrl: props.PublicKeyUrl,
                    salt: props.Salt.ToString(),
                    timestamp: (ulong)props.Timestamp
                ));
            }
            else
            {
                tcs.SetException(new Exception("Failed to get Apple Game Center credentials."));
            }
        });

        return tcs.Task;
    }

    private async Task SignInAnonymously()
    {
        SetLoadingText("signing in...");

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
                SetLoadingText("syncing with cloud...");

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
                if (loadingText != null)
                {
                    loadingText.color = Color.white;
                }
            }
        }
        catch (System.Exception)
        {
            Debug.Log("Failed to set dark mode on LoadingScene");
        }
    }

    public void SetLoadingText(string text)
    {
        if (loadingText == null) return;
        loadingText.text = text;
    }

    public void SetProgressText(string text)
    {
        if (progressText == null) return;
        progressText.text = text;
    }

    public async Task<bool> HasInternetConnection()
    {
        using (UnityWebRequest request = UnityWebRequest.Head("https://www.google.com/generate_204"))
        {
            request.timeout = 3; // short timeout to avoid delay

            await request.SendWebRequest();

            return !request.result.Equals(UnityWebRequest.Result.ConnectionError) &&
                   !request.result.Equals(UnityWebRequest.Result.ProtocolError);
        }
    }
}
