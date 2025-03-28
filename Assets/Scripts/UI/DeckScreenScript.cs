using System.Collections.Generic;
using UnityEngine;

public class DeckScreenScript : MonoBehaviour
{
    [SerializeField] GameObject deckMenuScrollView;
    [SerializeField] GameObject cardUIPrefab;

    private void OnEnable()
    {
        var count = PowerupCardData.GetCardCount();

        bool[] inDeck = new bool[count];
        bool[] owned = new bool[count];

        // in deck divider
        GameObject cardUIInDeckDivider = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
        cardUIInDeckDivider.GetComponent<DeckbuilderCardUIPrefabScript>().InitializeDeckbuilderCardUI(-1, deckMenuScrollView);

        // check in deck
        List<int> deck = DeckManager.Instance.GetDecklist();
        foreach (var encodedCard in deck)
        {
            var decodedCard = PowerupCardData.DecodeCard(encodedCard);
            inDeck[decodedCard.cardIndex] = true;
        }

        // load in deck
        for (int i = 0; i < count; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            if (!inDeck[i]) continue;
            GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
            cardUI.GetComponent<DeckbuilderCardUIPrefabScript>().InitializeDeckbuilderCardUI(i, deckMenuScrollView, true);
        }

        // collection divider
        GameObject cardUICollectionDivider = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
        cardUICollectionDivider.GetComponent<DeckbuilderCardUIPrefabScript>().InitializeDeckbuilderCardUI(-2, deckMenuScrollView);

        var sums = PowerupCardData.GetAllCardsOwnedSums();
        for (int i = 0; i < sums.Length; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            owned[i] = sums[i] > 0;
        }

        // load owned
        for (int i = 0; i < count; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            if (!owned[i] || inDeck[i]) continue;
            GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
            cardUI.GetComponent<DeckbuilderCardUIPrefabScript>().InitializeDeckbuilderCardUI(i, deckMenuScrollView, true);
        }

        // undiscovered divider
        GameObject cardUIUndiscoveredDivider = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
        cardUIUndiscoveredDivider.GetComponent<DeckbuilderCardUIPrefabScript>().InitializeDeckbuilderCardUI(-3, deckMenuScrollView);

        // load not owned
        for (int i = 0; i < count; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            if (owned[i]) continue;
            GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
            cardUI.GetComponent<DeckbuilderCardUIPrefabScript>().InitializeDeckbuilderCardUI(i, deckMenuScrollView, false);
        }

        // no idea why the y pos here is that magic number, it works tho
        deckMenuScrollView.transform.localPosition = new Vector3(0, -4100, 0);
        DeckManager.Instance.UpdateTotalDeckCountUI();
        UIManagerScript.Instance.ApplyDarkMode();
    }

    private void OnDisable()
    {
        foreach (Transform child in deckMenuScrollView.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
