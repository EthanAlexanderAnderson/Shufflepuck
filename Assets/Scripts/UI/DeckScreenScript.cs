using UnityEngine;

public class DeckScreenScript : MonoBehaviour
{
    [SerializeField] GameObject deckMenuScrollView;
    [SerializeField] GameObject cardUIPrefab;

    private void OnEnable()
    {
        var count = PowerupCardData.GetCardCount();
        for (int i = 0; i < count; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) return;
            GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
            cardUI.GetComponent<CardUIPrefabScript>().InitializeCardUI(i, deckMenuScrollView.gameObject);
        }
    }

    private void OnDisable()
    {
        foreach (Transform child in deckMenuScrollView.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
