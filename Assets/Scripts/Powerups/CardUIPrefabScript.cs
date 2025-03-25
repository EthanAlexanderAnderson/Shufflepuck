using UnityEngine;
using UnityEngine.UI;

public class CardUIPrefabScript : MonoBehaviour
{
    // image
    private Sprite cardImageSprite;
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardButton; // for gameplay in-hand
    public Button GetButton() { return cardButton; }
    // for shiny effect
    [SerializeField] private GameObject rankParticleSystemObject;
    [SerializeField] private GameObject shineObject;
    // for holo effect
    [SerializeField] private GameObject holoParent;

    public void InitializeCardUI(int index, int rank = 0, bool holo = false)
    {
        if (index < 0 || index >= PowerupManager.Instance.powerupSprites.Length)
        {
            Debug.Log($"Index: {index} out of range.");
            return;
        }
        cardImageSprite = PowerupManager.Instance.powerupSprites[index];
        cardImage.sprite = cardImageSprite;

        SetRank(rank);
        holoParent.SetActive(holo);
    }

    private void SetRank(int rank)
    {
        switch (rank)
        {
            case 0:
                cardImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                break;
            case 1:
                cardImage.color = new Color(0.9f, 0.4f, 0f, 1f);
                break;
            case 2:
                cardImage.color = new Color(0.95f, 0.7f, 0f, 1f);
                break;
            case 3:
                cardImage.color = new Color(0f, 1f, 1f, 1f);
                break;
            case 4:
                cardImage.color = new Color(1f, 0f, 1f, 1f);
                break;
            default:
                cardImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                break;
        }

        rankParticleSystemObject.SetActive(rank > 0);
        shineObject.SetActive(rank > 0);
    }
}
