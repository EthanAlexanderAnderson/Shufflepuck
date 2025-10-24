using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckScreenScript : MonoBehaviour
{
    [SerializeField] GameObject deckMenuScrollView;
    [SerializeField] GameObject cardUIPrefab;

    [SerializeField] GameObject[] deckProfileButtons;

    [SerializeField] Sprite toggleOnSprite;
    [SerializeField] Sprite toggleOffSprite;

    [SerializeField] float y;

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

        // load in deck, sorted by rarity ascending
        for (int r = 0; r < 5; r++)
        {
            for (int i = 0; i < count; i++)
            {
                if (PowerupCardData.GetCardName(i) == null || PowerupCardData.GetCardRarity(i) != r) continue;
                if (!inDeck[i]) continue;
                GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
                cardUI.GetComponent<DeckbuilderCardUIPrefabScript>().InitializeDeckbuilderCardUI(i, deckMenuScrollView, true);
            }
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

        // load owned, sorted by rarity ascending
        for (int r = 0; r < 5; r++)
        {
            for (int i = 0; i < count; i++)
            {
                if (PowerupCardData.GetCardName(i) == null || PowerupCardData.GetCardRarity(i) != r) continue;
                if (!owned[i] || inDeck[i]) continue;
                GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
                cardUI.GetComponent<DeckbuilderCardUIPrefabScript>().InitializeDeckbuilderCardUI(i, deckMenuScrollView, true);
            }
        }

        // undiscovered divider
        GameObject cardUIUndiscoveredDivider = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
        cardUIUndiscoveredDivider.GetComponent<DeckbuilderCardUIPrefabScript>().InitializeDeckbuilderCardUI(-3, deckMenuScrollView);

        // load not owned, sorted by rarity ascending
        for (int r = 0; r < 5; r++)
        {
            for (int i = 0; i < count; i++)
            {
                if (PowerupCardData.GetCardName(i) == null || PowerupCardData.GetCardRarity(i) != r) continue;
                if (owned[i]) continue;
                GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
                cardUI.GetComponent<DeckbuilderCardUIPrefabScript>().InitializeDeckbuilderCardUI(i, deckMenuScrollView, false);
            }
        }

        deckMenuScrollView.transform.localPosition = new Vector3(0, y, 0);
        DeckManager.Instance.UpdateTotalDeckCountUI();

        // set active deck profile button
        for (int i = 0; i < deckProfileButtons.Length; i++)
        {
            deckProfileButtons[i].GetComponent<Image>().sprite = toggleOffSprite;
        }

        deckProfileButtons[PlayerPrefs.GetInt("ActiveDeckProfile", 1) - 1].GetComponent<Image>().sprite = toggleOnSprite;

        UIManagerScript.Instance.ApplyDarkMode();
    }

    private void OnDisable()
    {
        foreach (Transform child in deckMenuScrollView.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateAllCardCraftUI()
    {
        DeckbuilderCardUIPrefabScript[] deckbuilderCardUIArray = GetComponentsInChildren<DeckbuilderCardUIPrefabScript>();

        foreach (var deckbuilderCardUI in deckbuilderCardUIArray)
        {
            deckbuilderCardUI.UpdateCraftUI();
        }
    }

    public void SwitchActiveDeckProfile(int profileID)
    {
        DeckManager.Instance.SwitchActiveDeckProfile(profileID);
    }
}
