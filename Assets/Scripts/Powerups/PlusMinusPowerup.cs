using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlusMinusPowerup : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;

    [SerializeField] private int index;     // which card is this
    [SerializeField] private int count;     // how many do we have in deck currently
    [SerializeField] private int maxCount;  // how many are we allowed to have in deck
    [SerializeField] private Sprite cardImage;
    [SerializeField] private string cardName;

    private void OnEnable()
    {
        count = PlayerPrefs.GetInt(cardName + "CardCount", 0);
        if (!Application.isEditor && count > maxCount) // in editor we have no max per card
        {
            count = maxCount;
        }
        DeckManager.Instance.SetCardCount(index, count);
        text.text = count.ToString();
        UpdateMinusAndPlusUIButtonInteractability();
    }
    public void Minus()
    {
        if (count > 0)
        {
            count--;
        }
        UpdateMinusAndPlusUIButtonInteractability();
        UpdateAndSaveCardCount();
    }

    public void Plus()
    {
        if (Application.isEditor || count < maxCount) // in editor we have no max per card
        {
            count++;
        }
        UpdateMinusAndPlusUIButtonInteractability();
        UpdateAndSaveCardCount();
    }

    // Add/Remove card from the deck, update the UI, save the new count to playerprefs
    private void UpdateAndSaveCardCount()
    {
        DeckManager.Instance.SetCardCount(index, count);
        text.text = count.ToString();
        PlayerPrefs.SetInt(cardName + "CardCount", count);
    }

    private void UpdateMinusAndPlusUIButtonInteractability()
    {
        minusButton.interactable = !(count <= 0 && !Application.isEditor);
        plusButton.interactable = !(count >= maxCount && !Application.isEditor);
    }

    // Called when the user presses down on the image
    public void OnPointerDown(PointerEventData eventData)
    {
        if (cardImage != null)
        {
            DeckManager.Instance.SetCardPreviewImage(cardImage);
        }
    }

    // Called when the user releases their touch
    public void OnPointerUp(PointerEventData eventData)
    {
        if (cardImage != null)
        {
            DeckManager.Instance.SetCardPreviewImage(null);
        }
    }
}
