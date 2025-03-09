using System.Collections;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ProfileScreenScript : MonoBehaviour
{
    private string username;
    private string id;
    private string imgURL;

    [SerializeField] private Image profilepicture;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] GameObject authenticationButtonsParent;

    private void OnEnable()
    {
        UpdateAuthenticationUI();
    }

    public void UpdateAuthenticationUI()
    {
        if (PlayerAuthentication.Instance != null)
        {
            (username, id, imgURL) = PlayerAuthentication.Instance.GetProfile();
            usernameText.text = username;
            string profilepictureURL = imgURL;
            StartCoroutine(LoadProfilePicture(profilepictureURL));

            if (username != "")
            {
                authenticationButtonsParent.SetActive(false);
            }
            else
            {
                authenticationButtonsParent.SetActive(true);
            }
            Debug.Log(id);
        }
        else
        {
            Debug.LogError("PlayerAuthentication is null");
        }

    }

    private IEnumerator LoadProfilePicture(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            profilepicture.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
    }

    public void LogUnityPlayerID()
    {
        Debug.Log(AuthenticationService.Instance.PlayerId);
    }

}