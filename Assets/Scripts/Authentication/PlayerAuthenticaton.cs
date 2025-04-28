using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

public class PlayerAuthentication : MonoBehaviour
{
    public static PlayerAuthentication Instance;

    private bool isAuthenticated = false;

    ILocalPlayer localPlayer;

    public (string username, string id) GetProfile()
    {
        return (localPlayer.DisplayName, localPlayer.Identifier);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        GameServices.OnAuthStatusChange += OnAuthStatusChange;
    }

    private void OnDisable()
    {
        GameServices.OnAuthStatusChange -= OnAuthStatusChange;
    }

    private void Start()
    {
        StartAuthentication();
    }

    private async void StartAuthentication()
    {
        await UnityServices.InitializeAsync();

        if (isAuthenticated) return;
        if (GameServices.IsAvailable())
        {
            if (GameServices.IsAuthenticated)
            {
                Debug.Log("Player already authenticated");
                localPlayer = GameServices.LocalPlayer;
            }
            else
            {
                Debug.Log("calling GameServices.Authenticate");
                GameServices.Authenticate();
            }
        }
    }

    private void OnAuthStatusChange(GameServicesAuthStatusChangeResult result, Error error)
    {
        if (error == null)
        {
            Debug.Log("Auth Status Changed: " + result.AuthStatus);

            if (!isAuthenticated && result.AuthStatus == LocalPlayerAuthStatus.Authenticated)
            {
                isAuthenticated = true;

                localPlayer = result.LocalPlayer;

                Debug.Log($"Authenticated with platform as {localPlayer.DisplayName} ({localPlayer.Identifier})");

                GameServices.LoadServerCredentials(SignInWithUnityGooglePlay);
            }
        }
        else
        {
            Debug.LogError("Failed login with error: " + error);
            SignInWithUnityAnon();
        }
    }

    private async void SignInWithUnityGooglePlay(GameServicesLoadServerCredentialsResult result, Error error)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(result.ServerCredentials.AndroidProperties.ServerAuthCode);
        }
        catch (AuthenticationException e)
        {
            Debug.LogWarning($"Unity Google Play link failed: {e.Message}");
        }

        await HandlePostSignInAsync();
    }

    private async void SignInWithUnityAnon()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await HandlePostSignInAsync();
    }

    private async Task HandlePostSignInAsync()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            await PlayerDataManager.Instance.SyncWithCloudIfNeeded();
        }
        else
        {
            Debug.LogError("SaveAll/LoadAll Error: Not Signed In to Unity Authentication.");
        }
    }
}
