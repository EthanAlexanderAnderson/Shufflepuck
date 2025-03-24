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
        cardUIInDeckDivider.GetComponent<CardUIPrefabScript>().InitializeCardUI(-1, deckMenuScrollView.gameObject);

        // check in deck
        for (int i = 0; i < count; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            inDeck[i] = PlayerPrefs.GetInt($"{ PowerupCardData.GetCardName(i)}CardCount") > 0;
        }

        // load in deck
        for (int i = 0; i < count; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            if (!inDeck[i]) continue;
            GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
            cardUI.GetComponent<CardUIPrefabScript>().InitializeCardUI(i, deckMenuScrollView.gameObject, true);
        }

        // collection divider
        GameObject cardUICollectionDivider = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
        cardUICollectionDivider.GetComponent<CardUIPrefabScript>().InitializeCardUI(-2, deckMenuScrollView.gameObject);

        // check owned
        for (int i = 0; i < count; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            owned[i] = PowerupCardData.CheckIfCardIsOwned(i);
        }

        // load owned
        for (int i = 0; i < count; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            if (!owned[i] || inDeck[i]) continue;
            GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
            cardUI.GetComponent<CardUIPrefabScript>().InitializeCardUI(i, deckMenuScrollView.gameObject, true);
        }

        // undiscovered divider
        GameObject cardUIUndiscoveredDivider = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
        cardUIUndiscoveredDivider.GetComponent<CardUIPrefabScript>().InitializeCardUI(-3, deckMenuScrollView.gameObject);

        // load not owned
        for (int i = 0; i < count; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            if (owned[i]) continue;
            GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
            cardUI.GetComponent<CardUIPrefabScript>().InitializeCardUI(i, deckMenuScrollView.gameObject, false);
        }

        // no idea why the y pos here is that magic number, it works tho
        deckMenuScrollView.transform.localPosition = new Vector3(deckMenuScrollView.transform.localPosition.x, -4100, deckMenuScrollView.transform.localPosition.z);
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
