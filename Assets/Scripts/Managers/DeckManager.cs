using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    // self
    public static DeckManager Instance;

    [SerializeField] private TMP_Text deckCount; // total number of cards in deck
    private List<int> deck = new(); // Deck in encoded decklist format (NOT playdeck format)
    public List<int> GetDecklist() { return deck; }

    [SerializeField] private GameObject cardPreviewImage;

    [SerializeField] private GameObject deckMenu;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void Start()
    {
        // Load deck from file
        int activeDeckProfile = PlayerPrefs.GetInt("ActiveDeckProfile", 1);
        string deckKey = "Deck" + activeDeckProfile.ToString();

        // Get the decklist
        string deckString = PlayerPrefs.GetString(deckKey, "");

        // assert value
        if (string.IsNullOrEmpty(deckString)) return;

        // iterate through decklist
        string[] cardEncoded = deckString.Split(',');

        // add each card in the decklist to the deck
        for (int i = 0; i < cardEncoded.Length; i++)
        {
            if (int.TryParse(cardEncoded[i], out int encodedCard))
            {
                deck.Add(encodedCard);
            }
        }
    }

    // Set the count of a specific card in the deck
    public void SetCardCount(int cardIndex, int rank, bool holo, int count)
    {
        bool updated = false;

        // if card IS in deck (currently count above zero) update it's count
        for (int i = 0; i < deck.Count; i++)
        {
            var decodedCard = PowerupCardData.DecodeCard(deck[i]);
            if (decodedCard.cardIndex == cardIndex && decodedCard.rank == rank && decodedCard.holo == holo)
            {
                if (count == 0)
                {
                    deck.RemoveAt(i); // only store cards above 0 count
                    UpdateTotalDeckCountUI();
                    return;
                }
                deck[i] = PowerupCardData.EncodeCard(cardIndex, rank, holo, count);
                updated = true;
            }
        }

        // if card NOT in deck (currently count zero) add it
        if (!updated)
        {
            deck.Add(PowerupCardData.EncodeCard(cardIndex, rank, holo, count));
        }

        UpdateTotalDeckCountUI();
    }

    // Get the count of a specific card in the deck
    public int GetInDeckCount(int cardIndex, int rank, bool holo)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            var decodedCard = PowerupCardData.DecodeCard(deck[i]);
            if (decodedCard.cardIndex == cardIndex && decodedCard.rank == rank && decodedCard.holo == holo)
            {
                return decodedCard.quantity;
            }
        }
        return 0;
    }

    // Update the UI to show how many cards are currently in the deck
    public void UpdateTotalDeckCountUI()
    {
        var sum = 0;
        for (int i = 0; i < deck.Count; i++)
        {
            var decodedCard = PowerupCardData.DecodeCard(deck[i]);
            sum += decodedCard.quantity;
        }
        deckCount.text = sum.ToString();
    }

    public List<int> GetPlayDeck()
    {
        // convert from a decklist to a playdeck
        List<int> playDeck = new List<int>();
        for (int i = 0; i < deck.Count; i++)
        {
            var decodedCard = PowerupCardData.DecodeCard(deck[i]);
            for (int j = 0; j < decodedCard.quantity; j++) // take the number of each card in the decklist
            {
                playDeck.Add(PowerupCardData.EncodeCard(decodedCard.cardIndex, decodedCard.rank, decodedCard.holo, 1)); // and add one int of it's ID to the playdeck
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
    public void ExportDeckList()
    {
        int[] cardIndexes = new int[PowerupCardData.GetCardCount()];

        // Convert the decklist to string
        var stringDeckList = "";
        foreach (var encodedCard in deck)
        {
            var decodedCard = PowerupCardData.DecodeCard(encodedCard);
            int cardIndex = decodedCard.cardIndex;
            if (cardIndex >= 0 && cardIndex < PowerupCardData.GetCardCount())
            {
                cardIndexes[cardIndex]++;
            }
        }

        for (int i = 0; i < cardIndexes.Length; i++)
        {
            if (cardIndexes[i] > 0)
            {
                stringDeckList += PowerupCardData.GetCardName(i) + ": " + cardIndexes[i] + "\n";
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
        deck = new();

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
            for (int i = 0; i < PowerupCardData.GetCardCount(); i++)
            {
                if (!string.IsNullOrEmpty(PowerupCardData.GetCardName(i)) && PowerupCardData.GetCardName(i) == split[0].Trim())
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
                    // if we are importing more than we own or above maxCount, don't lol
                    int owned = PowerupCardData.GetCardOwnedSum(cardIndex + 5);
                    int maxCount = 5 - PowerupCardData.GetCardRarity(cardIndex);
                    if (count > owned || count > maxCount)
                    {
                        count = Mathf.Min(owned, maxCount);
                    }
                    // set new imported count
                    deck.Add(PowerupCardData.EncodeCard(cardIndex, 0, false, count));
                }
            }
        }

        // Update the deck count
        UpdateTotalDeckCountUI();

        // finally, blink the UI to re-trigger the OnEnable to read from player prefs and set UI text
        deckMenu.SetActive(false);
        deckMenu.SetActive(true);
    }

    public void SaveDeckToFile()
    {
        string deckString = "";

        // create string format
        foreach (var encodedCard in deck)
        {
            deckString += deckString == "" ? encodedCard.ToString() : "," + encodedCard.ToString();
        }

        // save to file
        int activeDeckProfile = PlayerPrefs.GetInt("ActiveDeckProfile", 1);
        string deckKey = "Deck" + activeDeckProfile.ToString();
        PlayerPrefs.SetString(deckKey, deckString);
        PlayerPrefs.Save();
    }
}
