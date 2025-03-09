using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Runtime.InteropServices;

public class PlayerAuthentication : MonoBehaviour
{
    // self
    public static PlayerAuthentication Instance;

    private string username;
    private string id;
    private string imgURL;

    public (string, string, string) GetProfile()
    {
        return (username, id, imgURL);
    }

    // Import native functions from iOS plugin
    [DllImport("__Internal")]
    private static extern void AuthenticateGameCenterPlayer();

    [DllImport("__Internal")]
    private static extern string GetGameCenterPlayerID();

    [DllImport("__Internal")]
    private static extern string GetGameCenterTeamID();

    [DllImport("__Internal")]
    private static extern string GetGameCenterPublicKeyURL();

    [DllImport("__Internal")]
    private static extern string GetGameCenterSalt();

    [DllImport("__Internal")]
    private static extern ulong GetGameCenterTimestamp();

    [DllImport("__Internal")]
    private static extern string GetGameCenterSignature();


    private async void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        //TODO: figure out if this is necessary
        //PlayGamesPlatform.Activate();

        await UnityServices.InitializeAsync();

        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Player is already signed in.");
            return;
        }

#if UNITY_ANDROID
        SignInWithGooglePlayGames();
#elif UNITY_IOS
        SignInWithAppleGameCenter();
#else
        SignInAnonymously();
#endif
    }

    // ---------- SIGN-IN FLOW ----------

    private void SignInWithGooglePlayGames()
    {
        try
        {
            PlayGamesPlatform.Instance.Authenticate(success =>
            {
                if (success == SignInStatus.Success)
                {
                    // Request Server Auth Code
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                    {
                        Debug.Log("Server Auth Code: " + code);
                        SignInWithGooglePlay(code);
                    });
                }
                else
                {
                    Debug.LogWarning($"Google Play sign-in failed.");
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

    private async void SignInWithGooglePlay(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("Signed in with Google Play Games!");

            username = PlayGamesPlatform.Instance.GetUserDisplayName();
            id = PlayGamesPlatform.Instance.GetUserId();
            imgURL = PlayGamesPlatform.Instance.GetUserImageUrl();
        }
        catch (AuthenticationException e)
        {
            Debug.LogWarning($"Google Play sign-in failed: {e.Message}");
            SignInWithAppleGameCenter();
        }
    }

    private async void SignInWithAppleGameCenter()
    {
        try
        {
            AuthenticateGameCenterPlayer(); // Triggers Game Center login

            string playerId = GetGameCenterPlayerID();
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
                signature, teamId, publicKeyUrl, salt, timestamp, new SignInOptions { CreateAccount = true }
            );

            Debug.Log("Successfully signed in with Apple Game Center!");
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

    private async void SignInAnonymously()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Signed In Anon as {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"Guest sign-in failed: {e.Message}");
        }
    }

    // ---------- ACCOUNT LINKING UI METHODS ----------

    public void LinkGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate(success =>
        {
            if (success == SignInStatus.Success)
            {
                // Request Server Auth Code
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("Server Auth Code: " + code);
                    LinkWithGooglePlay(code);
                });
            }
        });
    }

    private async void LinkWithGooglePlay(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
            Debug.Log("Google Play Games linked successfully!");
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"Failed to link Google Play: {e.Message}");
        }
    }

    public async void LinkWithAppleGameCenter()
    {
        try
        {
            AuthenticateGameCenterPlayer(); // Make sure player is authenticated

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
                signature, teamId, publicKeyUrl, salt, timestamp, new LinkOptions { ForceLink = true }
            );

            Debug.Log("Successfully linked Apple Game Center to guest account!");
        }
        catch (AuthenticationException e)
        {
            Debug.LogWarning($"Failed to link Apple Game Center: {e.Message}");
        }
    }
}
