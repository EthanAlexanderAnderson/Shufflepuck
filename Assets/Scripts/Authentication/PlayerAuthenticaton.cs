using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
#if UNITY_IOS
using System.Runtime.InteropServices;
// note for me later: it might be possible to use apple gamekit plugin here as long it's building on unity cloud build servers?
#endif

public class PlayerAuthentication : MonoBehaviour
{
    public static PlayerAuthentication Instance;

    private string username;
    private string id;
    private string imgURL;

    public (string username, string id, string imgURL) GetProfile()
    {
        return (username, id, imgURL);
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void AuthenticateGameCenterPlayer();
    [DllImport("__Internal")] private static extern string GetGameCenterPlayerID();
    [DllImport("__Internal")] private static extern string GetGameCenterTeamID();
    [DllImport("__Internal")] private static extern string GetGameCenterPublicKeyURL();
    [DllImport("__Internal")] private static extern string GetGameCenterSalt();
    [DllImport("__Internal")] private static extern ulong GetGameCenterTimestamp();
    [DllImport("__Internal")] private static extern string GetGameCenterSignature();
    [DllImport("__Internal")] private static extern string GetGameCenterDisplayName();

#endif

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
#if UNITY_ANDROID
        PlayGamesPlatform.Activate();
#endif
        await UnityServices.InitializeAsync();

        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Player is already signed in.");
            return;
        }

#if UNITY_ANDROID
        AuthenticateWithGooglePlayGames();
#elif UNITY_IOS
        SignInWithAppleGameCenterAsync();
#else
        SignInAnonymously();
#endif
    }

    // ---------- SIGN-IN FLOW ----------

#if UNITY_ANDROID
    private void AuthenticateWithGooglePlayGames()
    {
        try
        {
            PlayGamesPlatform.Instance.Authenticate(success =>
            {
                if (success == SignInStatus.Success)
                {
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                    {
                        CompleteGooglePlaySignInAsync(code);
                    });
                }
                else
                {
                    Debug.LogWarning("Google Play sign-in failed.");
                    SignInAnonymously();
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Google Play - unexpected exception: {e.Message}");
            SignInAnonymously();
        }
    }

    private async void CompleteGooglePlaySignInAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("Signed in with Google Play Games!");

            // These are checked for null values in ProfileScreenScript before they are used.
            username = PlayGamesPlatform.Instance.GetUserDisplayName();
            id = PlayGamesPlatform.Instance.GetUserId();
            imgURL = PlayGamesPlatform.Instance.GetUserImageUrl();

            await HandlePostSignInAsync();
        }
        catch (AuthenticationException e)
        {
            Debug.LogWarning($"Google Play sign-in failed: {e.Message}");
        }
    }
#endif

#if UNITY_IOS
    private async void SignInWithAppleGameCenterAsync()
    {
        try
        {
            AuthenticateGameCenterPlayer();

            string playerId = GetGameCenterPlayerID();
            id = playerId;
            string teamId = GetGameCenterTeamID();
            string publicKeyUrl = GetGameCenterPublicKeyURL();
            string salt = GetGameCenterSalt();
            ulong timestamp = GetGameCenterTimestamp();
            string signature = GetGameCenterSignature();

            if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(signature))
            {
                Debug.LogWarning("Failed to retrieve Game Center credentials.");
                SignInAnonymously();
                return;
            }

            await AuthenticationService.Instance.SignInWithAppleGameCenterAsync(
                signature, teamId, publicKeyUrl, salt, timestamp, new SignInOptions { CreateAccount = true });

            try
            {
                username = GetGameCenterDisplayName();
            }
            catch (System.Exception)
            {
                Debug.Log("Couldn't get display name.");
            }

            Debug.Log("Successfully signed in with Apple Game Center!");

            await HandlePostSignInAsync();
        }
        catch (AuthenticationException e)
        {
            Debug.LogWarning($"Apple Game Center sign-in failed: {e.Message}");
            SignInAnonymously();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Apple Game Center - unexpected exception: {e.Message}");
            SignInAnonymously();
        }
    }
#endif

            private async void SignInAnonymously()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Signed In Anon as {AuthenticationService.Instance.PlayerId}");
            SceneManager.LoadScene("SampleScene");
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"Guest sign-in failed: {e.Message}");
        }
    }

    // ---------- ACCOUNT LINKING UI METHODS ----------


    public void LinkGooglePlayGames()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.Authenticate(success =>
        {
            if (success == SignInStatus.Success)
            {
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    LinkWithGooglePlay(code);
                });
            }
        });
#else
        Debug.Log("This service is not yet available on your device.");
#endif
    }
#if UNITY_ANDROID
    private async void LinkWithGooglePlay(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
            Debug.Log("Google Play Games linked successfully!");
            await PlayerDataManager.Instance.SaveAllData();
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"Failed to link Google Play: {e.Message}");
        }
    }
#endif

    public async void LinkWithAppleGameCenter()
    {
#if UNITY_IOS
        try
        {
            AuthenticateGameCenterPlayer();

            string playerId = GetGameCenterPlayerID();
            string teamId = GetGameCenterTeamID();
            string publicKeyUrl = GetGameCenterPublicKeyURL();
            string salt = GetGameCenterSalt();
            ulong timestamp = GetGameCenterTimestamp();
            string signature = GetGameCenterSignature();

            if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(signature))
            {
                Debug.LogWarning("Failed to retrieve Game Center credentials for linking.");
                return;
            }

            await AuthenticationService.Instance.LinkWithAppleGameCenterAsync(
                signature, teamId, publicKeyUrl, salt, timestamp, new LinkOptions { ForceLink = true });

            Debug.Log("Successfully linked Apple Game Center to guest account!");
            await PlayerDataManager.Instance.SaveAllData();
        }
        catch (AuthenticationException e)
        {
            Debug.LogWarning($"Failed to link Apple Game Center: {e.Message}");
        }
#else
        Debug.Log("This service is not yet available on your device.");
#endif
    }

private async Task HandlePostSignInAsync()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            await PlayerDataManager.Instance.SyncWithCloudIfNeeded();
        }
        else
        {
            Debug.LogError("SaveAll/LoadAll Error: Not Signed In.");
        }
    }
}
