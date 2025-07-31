using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerupPopupPrefabScript : MonoBehaviour
{
    public GameObject floatingTextPrefab;

    // TODO: change "card" to "popup"
    [SerializeField] private GameObject cardIconOutlineObject;
    [SerializeField] private Image cardIconOutline;
    [SerializeField] private GameObject cardIconObject;
    [SerializeField] private Image cardIcon;
    [SerializeField] private GameObject cardTextObject;
    [SerializeField] private TMP_Text cardText;

    [SerializeField] private GameObject holoParent;

    [SerializeField] private GameObject rankParticleSystemObject;
    [SerializeField] private ParticleSystem rankParticleSystemOngoing;
    [SerializeField] private ParticleSystem rankParticleSystemBurst;
    private ParticleSystem.MainModule rankParticleSystemOngoingMain;
    private ParticleSystem.MainModule rankParticleSystemBurstMain;
    [SerializeField] private GameObject shineIconObject;
    [SerializeField] private GameObject shineTextObject;

    [SerializeField] private GameObject cardRarityObject;
    [SerializeField] private Image cardRarityIcon;
    [SerializeField] private TMP_Text cardRarityText;

    [SerializeField] private GameObject cardRankObject;
    [SerializeField] private TMP_Text cardRankText;

    private string[] rarityTexts = { "common", "uncommon", "rare", "epic", "legendary" };
    [SerializeField] private Sprite[] rarityIcons = new Sprite[5];

    private string[] rankTexts = { "", "bronze", "gold", "diamond", "celestial" };

    [SerializeField] private Sprite questionMarkIconSprite;
    private string[] boosterTexts = { "any holo", "any bronze", "any gold", "any diamond", "any celestial" };

    private bool isNew = false;
    [SerializeField] private float newTextSize = 20;
    [SerializeField] private float newTextUpRate = 0f;
    [SerializeField] private float newTextShrinkRate = 0.01f;
    [SerializeField] private float newTextFadeRate = 0.1f;

    public void InitializePowerupPopup(int cardIndex, int rank, bool holo, bool isNew = false)
    {
        // for normal cards
        if (cardIndex >= 0)
        {
            cardIcon.sprite = PowerupManager.Instance.powerupIcons[cardIndex];
            cardIconOutline.sprite = PowerupManager.Instance.powerupIcons[cardIndex];
            cardText.text = PowerupManager.Instance.powerupTexts[cardIndex];

            // Rarity
            cardRarityObject.SetActive(true);
            int rarity = PowerupCardData.GetCardRarity(cardIndex);
            if (rarity >= 0 && rarity < rarityTexts.Length) // this is needed for Plus Three
            {
                Color rarityColor = GetRarityColor(rarity);
                cardRarityIcon.color = rarityColor;
                cardRarityText.color = rarityColor;
                cardRarityText.text = rarityTexts[rarity];
                cardRarityIcon.sprite = rarityIcons[rarity];
                if (rarity > 2)
                {
                    Transform icon = cardRarityIcon.gameObject.transform;
                    icon.localPosition = new Vector3(icon.localPosition.x + (rarity - 2) * 10, icon.localPosition.y);
                }
            }
            else
            {
                cardRarityText.text = "";
                cardRarityIcon.enabled = false;
            }
        }
        // for pack boosters
        else if (cardIndex >= -5)
        {
            cardIndex *= -1;
            cardIcon.sprite = questionMarkIconSprite;
            cardIconOutline.sprite = questionMarkIconSprite;
            cardText.text = boosterTexts[cardIndex - 1];
            cardRarityObject.SetActive(false);
        }

        // Rank & Holo
        rankParticleSystemOngoingMain = rankParticleSystemOngoing.main;
        rankParticleSystemBurstMain = rankParticleSystemBurst.main;
        SetRank(rank);
        if (rank > 0 && rank < rankTexts.Length)
        {
            cardRankText.text = rankTexts[rank];
        }
        holoParent.SetActive(holo);
        cardIconOutlineObject.SetActive(holo);

        this.isNew = isNew;
    }

    // TODO: Bool:AnimateOut Float:Speed
    public void Animate()
    {
        LeanTween.scale(cardIconOutlineObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic);
        LeanTween.scale(cardIconObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic);
        LeanTween.scale(cardTextObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.2f);
        LeanTween.scale(cardRarityObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.4f);
        LeanTween.scale(cardRankObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.6f);

        if (isNew)
        {
            GameObject floatingText = Instantiate(floatingTextPrefab, this.transform);
            floatingText.GetComponent<FloatingTextScript>().Initialize("NEW!", newTextUpRate, newTextShrinkRate, newTextFadeRate, Vector2.zero, newTextSize);
        }
    }

    // TODO: get feedback on rank colors
    private void SetRank(int rank)
    {
        switch (rank)
        {
            case 1:
                cardIcon.color = new Color(0.9f, 0.4f, 0f, 1f);
                cardText.color = new Color(0.9f, 0.4f, 0f, 1f);
                rankParticleSystemOngoingMain.startColor = new Color(1f, 0.35f, 0f, 1f);
                rankParticleSystemBurstMain.startColor = new Color(1f, 0.35f, 0f, 1f);
                cardRankText.color = new Color(0.9f, 0.4f, 0f, 1f);
                break;
            case 2:
                cardIcon.color = new Color(0.95f, 0.7f, 0f, 1f);
                cardText.color = new Color(0.95f, 0.7f, 0f, 1f);
                rankParticleSystemOngoingMain.startColor = new Color(0.9f, 0.7f, 0f, 1f);
                rankParticleSystemBurstMain.startColor = new Color(0.9f, 0.7f, 0f, 1f);
                cardRankText.color = new Color(0.9f, 0.7f, 0f, 1f);
                break;
            case 3:
                cardIcon.color = new Color(0f, 1f, 1f, 1f);
                cardText.color = new Color(0f, 1f, 1f, 1f);
                rankParticleSystemOngoingMain.startColor = new Color(0f, 1f, 1f, 1f);
                rankParticleSystemBurstMain.startColor = new Color(0f, 1f, 1f, 1f);
                cardRankText.color = new Color(0f, 1f, 1f, 1f);
                break;
            case 4:
                cardIcon.color = new Color(1f, 0f, 1f, 1f);
                cardText.color = new Color(1f, 0f, 1f, 1f);
                rankParticleSystemOngoingMain.startColor = new Color(1f, 0f, 1f, 1f);
                rankParticleSystemBurstMain.startColor = new Color(1f, 0f, 1f, 1f);
                cardRankText.color = new Color(1f, 0f, 1f, 1f);
                break;
            default:
                //cardIcon.color = new Color(1f, 1f, 1f, 1f);
                //cardText.color = new Color(1f, 1f, 1f, 1f);
                cardIcon.color = UIManagerScript.Instance.GetDarkMode() ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 1f);
                cardText.color = UIManagerScript.Instance.GetDarkMode() ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 1f);
                break;
        }

        rankParticleSystemObject.SetActive(rank > 0);
        shineIconObject.SetActive(rank > 0);
        shineTextObject.SetActive(rank > 0);
    }

    private Color GetRarityColor(int rarity)
    {
        switch (rarity)
        {
            case 1:
                return new Color(0.4862745f, 0.7725491f, 0.4627451f);
            case 2:
                return new Color(0.0000000f, 0.7490196f, 0.9529412f);
            case 3:
                return new Color(0.5215687f, 0.3764706f, 0.6588235f);
            case 4:
                return new Color(1.0000000f, 0.9607844f, 0.4078432f);
            default:
                return UIManagerScript.Instance.GetDarkMode() ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 1f);
        }
    }

    public (GameObject, GameObject, GameObject, GameObject, GameObject) GetObjects()
    {
        return (cardIconObject, cardIconOutlineObject, cardTextObject, cardRarityObject, cardRankObject);
    }
}
