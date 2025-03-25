using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerupPopupPrefabScript : MonoBehaviour
{
    // TODO: change "card" to "popup"
    [SerializeField] private GameObject cardIconOutlineObject;
    [SerializeField] private Image cardIconOutline;
    [SerializeField] private GameObject cardIconObject;
    [SerializeField] private Image cardIcon;
    [SerializeField] private GameObject cardTextObject;
    [SerializeField] private TMP_Text cardText;

    [SerializeField] private GameObject holoParent;

    [SerializeField] private GameObject rankParticleSystemObject;
    [SerializeField] private GameObject shineIconObject;
    [SerializeField] private GameObject shineTextObject;

    [SerializeField] private GameObject cardRarityObject;
    [SerializeField] private Image cardRarityIcon;
    [SerializeField] private TMP_Text cardRarityText;

    private string[] rarityTexts = { "common", "uncomon", "rare", "epic", "legendary" };
    [SerializeField] private Sprite[] rarityIcons = new Sprite[5];

    public void InitializePowerupPopup(int cardIndex, int rank, bool holo)
    {
        cardIcon.sprite = PowerupManager.Instance.powerupIcons[cardIndex];
        cardIconOutline.sprite = PowerupManager.Instance.powerupIcons[cardIndex];
        cardText.text = PowerupManager.Instance.powerupTexts[cardIndex];

        // Rarity
        int rarity = PowerupCardData.GetCardRarity(cardIndex);
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

        // Rank & Holo
        SetRank(rank);
        holoParent.SetActive(holo);
        cardIconOutlineObject.SetActive(holo);
    }

    // TODO: Bool:AnimateOut Float:Speed
    public void Animate()
    {
        LeanTween.scale(cardIconOutlineObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic);
        LeanTween.scale(cardIconObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic);
        LeanTween.scale(cardTextObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.2f);
        LeanTween.scale(cardRarityObject, new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.4f);
    }

    private void SetRank(int rank)
    {
        switch (rank)
        {
            case 1:
                cardIcon.color = new Color(0.9f, 0.4f, 0f, 1f);
                cardText.color = new Color(0.9f, 0.4f, 0f, 1f);
                break;
            case 2:
                cardIcon.color = new Color(0.95f, 0.7f, 0f, 1f);
                cardText.color = new Color(0.95f, 0.7f, 0f, 1f);
                break;
            case 3:
                cardIcon.color = new Color(0f, 1f, 1f, 1f);
                cardText.color = new Color(0f, 1f, 1f, 1f);
                break;
            case 4:
                cardIcon.color = new Color(1f, 0f, 1f, 1f);
                cardText.color = new Color(1f, 0f, 1f, 1f);
                break;
            default:
                cardIcon.color = UIManagerScript.Instance.GetDarkMode() ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 1f);
                cardText.color = UIManagerScript.Instance.GetDarkMode() ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 1f);
                break;
        }
        if (rank > 0)
        {
            rankParticleSystemObject.SetActive(true);
            shineIconObject.SetActive(true);
            shineTextObject.SetActive(true);
        }
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

    public (GameObject, GameObject, GameObject, GameObject) GetObjects()
    {
        return (cardIconObject, cardIconOutlineObject, cardTextObject, cardRarityObject);
    }
}
