using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    // self
    public static DeckManager Instance;

    [SerializeField] private TMP_Text deckCount; // total number of cards in deck
    private int[] deck; // Deck in decklist format (NOT playdeck format)

    [SerializeField] private GameObject cardPreviewImage;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        deck = new int[PowerupManager.Instance.GetMethodArrayLength()];
        // default deck is one of each card
        for (int i = 0; i < PowerupManager.Instance.GetMethodArrayLength(); i++)
        {
            deck[i] = 1;
        }
    }

    // Set the count of a specific card in the deck
    public void SetCardCount(int index, int count)
    {
        deck[index] = count;
        UpdateDeckCount();
    }

    // Get the count of a specific card in the deck
    public int GetCount(int index)
    {
        return deck[index];
    }

    // Update the UI to show how many cards are currently in the deck
    public void UpdateDeckCount()
    {
        var sum = deck.Sum();
        deckCount.text = sum.ToString();
        if (sum < 10)
        {
            deckCount.color = new Color(0.9490197f, 0.4235294f, 0.3098039f); // red
        }
        else
        {
            deckCount.color = new Color(0.4862745f, 0.7725491f, 0.4627451f); // green
        }
    }

    public List<int> GetDeck()
    {
        // make sure our decklist is valid
        var sum = deck.Sum();
        if (sum < 10)
        {
            // if our deck is invalid, return the default deck
            for (int i = 0; i < deck.Length; i++)
            {
                deck[i] = 1;
            }
        }

        // convert from a decklist to a playdeck
        List<int> playDeck = new List<int>();
        for (int i = 0; i < deck.Length; i++)
        {
            for (int j = 0; j < deck[i]; j++) // take the number of each card in the decklist
            {
                playDeck.Add(i); // and add one int of it's ID to the playdeck
            }
        }

        return playDeck;
    }

    // For the deckbuilding menu, holding down on a card will reveal the card description. This method is a helper for that
    public void SetCardPreviewImage(Sprite img)
    {
        cardPreviewImage.SetActive(img != null);
        cardPreviewImage.GetComponent<Image>().sprite = img;
    }

    string[] cardNames = { "plus one", "foresight", "block", "bolt", "force field", "phase", "cull", "growth", "lock", "explosion", "fog", "hydra" };
    public void ExportDeckList()
    {
        // Convert the decklist to string
        var stringDeckList = "";
        for (int i = 0; i < cardNames.Length; i++)
        {
            if (deck[i] > 0)
            {
                stringDeckList += cardNames[i] + ": " + deck[i] + "\n";
            }
        }

        // Remove the final newline, if it exists
        if (stringDeckList.EndsWith("\n"))
        {
            stringDeckList = stringDeckList.Substring(0, stringDeckList.Length - 1);
        }

        // Export to clipboard
        GUIUtility.systemCopyBuffer = stringDeckList;
    }
    public void ImportDeckList()
    {
        UIManagerScript.Instance.SetErrorMessage("Import deck feature coming soon");
    }
}
