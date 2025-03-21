using UnityEngine;

public class DeckScreenScript : MonoBehaviour
{
    [SerializeField] GameObject deckMenuScrollView;
    [SerializeField] GameObject cardUIPrefab;

    private void OnEnable()
    {
        var count = PowerupCardData.GetCardCount();

        bool[] owned = new bool[count];
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
            if (!owned[i]) continue;
            GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
            cardUI.GetComponent<CardUIPrefabScript>().InitializeCardUI(i, deckMenuScrollView.gameObject, true);
        }

        // divider
        GameObject cardUIDivider = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
        cardUIDivider.GetComponent<CardUIPrefabScript>().InitializeCardUI(-1, deckMenuScrollView.gameObject);

        // load not owned
        for (int i = 0; i < count; i++)
        {
            if (PowerupCardData.GetCardName(i) == null) continue;
            if (owned[i]) continue;
            GameObject cardUI = Instantiate(cardUIPrefab, deckMenuScrollView.transform);
            cardUI.GetComponent<CardUIPrefabScript>().InitializeCardUI(i, deckMenuScrollView.gameObject, false);
        }

        deckMenuScrollView.transform.position = new Vector3(deckMenuScrollView.transform.position.x, -100 * count, deckMenuScrollView.transform.position.z);
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
