using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class PlusMinusPowerup : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private int index;
    [SerializeField] private int count;
    [SerializeField] private int maxCount;
    [SerializeField] private Sprite cardImage;
    [SerializeField] private string cardName;

    private void OnEnable()
    {
        text.text = DeckManager.Instance.GetCount(index).ToString();
        UpdateCount();
        count = PlayerPrefs.GetInt(cardName + "CardCount", 1);
        DeckManager.Instance.SetCount(index, count);
        text.text = count.ToString();
    }

    public void Plus()
    {
        if (count < maxCount)
        {
            count++;
        }
        DeckManager.Instance.SetCount(index, count);
        text.text = count.ToString();
        PlayerPrefs.SetInt(cardName + "CardCount", count);
    }
    public void Minus()
    {
        if (count > 0)
        {
            count--;
        }
        DeckManager.Instance.SetCount(index, count);
        text.text = count.ToString();
        PlayerPrefs.SetInt(cardName + "CardCount", count);
    }

    private void UpdateCount()
    {
        DeckManager.Instance.UpdateDeckCount();
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
