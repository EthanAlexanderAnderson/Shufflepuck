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

    [SerializeField] private GameObject deckMenu;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        deck = new int[PowerupManager.Instance.GetMethodArrayLength()];
        // default deck is empty
        for (int i = 0; i < PowerupManager.Instance.GetMethodArrayLength(); i++)
        {
            deck[i] = 0;
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
    }

    public List<int> GetDeck()
    {
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

    // Export the decklist to the clipboard
    string[] cardNames = { "PlusOne", "Foresight", "Block", "Bolt", "ForceField", "Phase", "Cull", "Growth", "Lock", "Explosion", "Fog", "Hydra", "Factory", "Shield", "Shuffle", "Chaos", "TimesTwo", "Resurrect", "Mill", "Research", "Insanity", "Triple", "Exponent", "Laser", "Aura", "Push", "Erratic", "Deny", "Investment", "Omniscience", null };
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

    // Import a decklist from the clipboard
    public void ImportDeckList()
    {
        // Get the clipboard contents
        var stringDeckList = GUIUtility.systemCopyBuffer;

        // Split the string into lines
        var lines = stringDeckList.Split('\n');

        // Reset the deck
        for (int i = 0; i < deck.Length; i++)
        {
            deck[i] = 0;
            PlayerPrefs.SetInt(cardNames[i] + "CardCount", 0);
        }

        // Parse the lines
        foreach (var line in lines)
        {
            // Split the line into card name and count
            var split = line.Split(':');
            if (split.Length != 2)
            {
                continue;
            }

            // Find the card index
            var cardIndex = -1;
            for (int i = 0; i < cardNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(cardNames[i]) && cardNames[i] == split[0].Trim())
                {
                    cardIndex = i;
                    break;
                }
            }

            // If the card was found, set the count and save to playerprefs
            if (cardIndex != -1)
            {
                int count;
                if (int.TryParse(split[1].Trim(), out count))
                {
                    deck[cardIndex] = count;
                    PlayerPrefs.SetInt(cardNames[cardIndex] + "CardCount", count);
                }
            }
        }

        // Update the deck count
        UpdateDeckCount();

        // finally, blink the UI to re-trigger the OnEnable to read from player prefs and set UI text
        deckMenu.SetActive(false);
        deckMenu.SetActive(true);
    }
}
