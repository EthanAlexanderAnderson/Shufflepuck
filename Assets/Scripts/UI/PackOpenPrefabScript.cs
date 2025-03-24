using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PackOpenPrefabScript : MonoBehaviour
{
    [SerializeField] private GameObject puckImageObject;
    [SerializeField] private ParticleSystem rarityParticleSystem;

    [SerializeField] private GameObject cardParent;
    [SerializeField] private GameObject cardIconOutlineObject;
    [SerializeField] private Image cardIconOutline;
    [SerializeField] private GameObject cardIconObject;
    [SerializeField] private Image cardIcon;
    [SerializeField] private GameObject cardTextObject;
    [SerializeField] private TMP_Text cardText;

    [SerializeField] private GameObject cardRarityParentObject;
    [SerializeField] private Image cardRarityIcon;
    [SerializeField] private TMP_Text cardRarityText;

    [SerializeField] private GameObject holoParent;

    [SerializeField] private GameObject rankParticleSystemObject;
    [SerializeField] private GameObject shineIconObject;
    [SerializeField] private GameObject shineTextObject;

    private string[] rarityTexts = { "common", "uncomon", "rare", "epic", "legendary" };
    [SerializeField] private Sprite[] rarityIcons = new Sprite[5];

    int targetClicks;
    int clicks;

    bool holo;
    int rank;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && puckImageObject.transform.localScale == Vector3.one && clicks < targetClicks)
        {
            if (ClickIsNotOnPack() && !Application.isEditor) return;

            LeanTween.cancel(puckImageObject);

            ParticleSystem.MainModule main = rarityParticleSystem.main;
            Color rarityColor = GetRarityColor();
            main.startColor = rarityColor;

            puckImageObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            LeanTween.scale(puckImageObject, Vector3.one, 0.5f).setEaseOutElastic();
            rarityParticleSystem.Play();
            clicks++;
        }
        else if (Input.GetMouseButtonDown(0) && puckImageObject.transform.localScale == Vector3.one && clicks == targetClicks)
        {
            if (ClickIsNotOnPack() && !Application.isEditor) return;

            LeanTween.cancel(puckImageObject);

            ParticleSystem.MainModule main = rarityParticleSystem.main;
            Color rarityColor = GetRarityColor();
            main.startColor = rarityColor;
            cardRarityText.color = rarityColor;
            if (clicks == 0) cardRarityIcon.color = rarityColor;
            cardRarityText.text = rarityTexts[clicks];
            cardRarityIcon.sprite = rarityIcons[clicks];
            if (clicks > 2)
            {
                Transform icon = cardRarityIcon.gameObject.transform;
                icon.localPosition = new Vector3(icon.localPosition.x + (clicks - 2) * 10, icon.localPosition.y);
            }

            puckImageObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            LeanTween.scale(puckImageObject, Vector3.one, 0.5f).setEaseOutElastic();
            rarityParticleSystem.Play();
            clicks++;

            LeanTween.scale(puckImageObject, Vector3.zero, 0.5f).setEaseInOutQuint().setDelay(0.45f);

            cardIcon.color = new Color(1f, 1f, 1f, 1f);
            cardText.color = new Color(1f, 1f, 1f, 1f);
            LeanTween.scale(cardIconOutlineObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.8f);
            LeanTween.scale(cardIconObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.8f);
            LeanTween.scale(cardTextObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(1f);
            LeanTween.scale(cardRarityParentObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(1.2f);

            EnableHolo();
            SetRank();
        }
    }

    public void InitializePackOpen(int cardIndex, int rank, bool holo)
    {
        targetClicks = PowerupCardData.GetCardRarity(cardIndex);
        cardIcon.sprite = PowerupManager.Instance.powerupIcons[cardIndex];
        cardIconOutline.sprite = PowerupManager.Instance.powerupIcons[cardIndex];
        cardText.text = PowerupManager.Instance.powerupTexts[cardIndex];
        this.holo = holo;
        this.rank = rank;
        AddToInventory(cardIndex);
    }

    private Color GetRarityColor()
    {
        switch (clicks)
        {
            case 0:
                return(new Color(1f, 1f, 1f, 1f));
            case 1:
                return(new Color(0.4862745f, 0.7725491f, 0.4627451f));
            case 2:
                return (new Color(0.0000000f, 0.7490196f, 0.9529412f));
            case 3:
                return (new Color(0.5215687f, 0.3764706f, 0.6588235f));
            case 4:
                return (new Color(1.0000000f, 0.9607844f, 0.4078432f));
            default:
                return (new Color(1f, 1f, 1f, 1f));
        }
    }

    private void SetRank()
    {
        switch (rank)
        {
            case 0:
                cardText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                break;
            case 1:
                cardText.color = new Color(0.9f, 0.4f, 0f, 1f);
                cardIcon.color = new Color(0.9f, 0.4f, 0f, 1f);
                break;
            case 2:
                cardText.color = new Color(0.95f, 0.7f, 0f, 1f);
                cardIcon.color = new Color(0.95f, 0.7f, 0f, 1f);
                break;
            case 3:
                cardText.color = new Color(0f, 1f, 1f, 1f);
                cardIcon.color = new Color(0f, 1f, 1f, 1f);
                break;
            case 4:
                cardText.color = new Color(1f, 0f, 1f, 1f);
                cardIcon.color = new Color(1f, 0f, 1f, 1f);
                break;
            default:
                cardText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                break;
        }
        if (rank > 0)
        {
            rankParticleSystemObject.SetActive(true);
            shineIconObject.SetActive(true);
            shineTextObject.SetActive(true);
        }
    }

    private void EnableHolo()
    {
        if (holo)
        {
            holoParent.SetActive(true);
            cardIconOutlineObject.SetActive(true);
        }
    }

    private bool ClickIsNotOnPack()
    {
        Vector3 mousePosition = Input.mousePosition; // Get mouse position in screen space
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition); // Convert to world space

        worldPosition.z = gameObject.transform.position.z; // Ensure the z-coordinate is set as the same as the pack to compare

        float distance = Vector3.Distance(worldPosition, gameObject.transform.position); // get distance

        // If the distance is less than or equal to 1 unit, the click is on or near the pack
        if (distance > gameObject.transform.localScale.x * 1.6f)
        {
            return true; // Click is not on pack
        }
        else
        {
            return false;
        }
    }

    private void AddToInventory(int cardIndex)
    {
        string cardIDString = PowerupCardData.GetCardName(cardIndex);
        string rankString = "";

        switch (rank)
        {
            case 1:
                rankString = "Bronze";
                break;
            case 2:
                rankString = "Gold";
                break;
            case 3:
                rankString = "Diamond";
                break;
            case 4:
                rankString = "Celestial";
                break;
        }

        string holoString = holo ? "Holo" : "";

        string key = $"{cardIDString}CardOwned{rankString}{holoString}";
        int count = PlayerPrefs.GetInt(key);

        PlayerPrefs.SetInt(key, count + 1);
    }
}
