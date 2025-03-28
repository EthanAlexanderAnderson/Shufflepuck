using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardVariation
{
    public int rank;
    public bool holo;
    public int count;
    public CardVariation(int rank, bool holo, int count) => (this.rank, this.holo, this.count) = (rank, holo, count);
    public int GetEncoded(int cardIndex) { return PowerupCardData.EncodeCard(cardIndex, rank, holo, count); }
}

public static class PowerupCardData
{
    private static string[] cardImportNames = { "PlusOne", "Foresight", "Block", "Bolt", "ForceField", "Phase", "Cull", "Growth", "Lock", "Explosion", "Fog", "Hydra", "Factory", "Shield", "Shuffle", "Chaos", "TimesTwo", "Resurrect", "Mill", "Research", "Insanity", "Triple", "Exponent", "Laser", "Aura", "Push", "Erratic", "Deny", "Investment", "Omniscience", null };
    public static string GetCardName(int cardIndex) { return cardImportNames[cardIndex]; }

    // TODO: rename this function
    public static int GetCardCount() { return cardImportNames.Length; }

    public static int GetCardOwnedCount(int cardIndex, int rank = 0, bool holo = false)
    {
        // Get the card collection
        string collectionString = PlayerPrefs.GetString("CardCollection", "");

        // assert value
        if (string.IsNullOrEmpty(collectionString)) return 0;

        // Split the collection string into individual encoded card values
        string[] cardEncoded = collectionString.Split(',');

        // Search for the card in the collection and return quantity
        for (int i = 0; i < cardEncoded.Length; i++)
        {
            if (int.TryParse(cardEncoded[i], out int encodedCard))
            {
                var decodedCard = DecodeCard(encodedCard);
                if (decodedCard.cardIndex == cardIndex && decodedCard.rank == rank && decodedCard.holo == holo)
                {
                    // Add the count to the current quantity
                    if (decodedCard.quantity > 0)
                    {
                        return decodedCard.quantity;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
        return 0;
    }
    public static int GetCardOwnedSum(int cardIndex)
    {
        // Get the card collection
        string collectionString = PlayerPrefs.GetString("CardCollection", "");

        // assert value
        if (string.IsNullOrEmpty(collectionString)) return 0;

        // iterate through collection
        string[] cardEncoded = collectionString.Split(',');
        int sum = 0;

        // Search for the card in the collection and return quantity
        for (int i = 0; i < cardEncoded.Length; i++)
        {
            if (int.TryParse(cardEncoded[i], out int encodedCard))
            {
                var decodedCard = DecodeCard(encodedCard);
                if (decodedCard.cardIndex == cardIndex)
                {
                    // Add the count to the current quantity
                    if (decodedCard.quantity > 0)
                    {
                        sum += decodedCard.quantity;
                    }
                }
            }
        }

        return sum;
    }
    public static bool CheckIfCardIsOwned(int cardIndex)
    {
        return GetCardOwnedSum(cardIndex) > 0;
    }
    public static int[] GetAllCardsOwnedSums()
    {
        int[] sums = new int[GetCardCount()];

        // Get the card collection
        string collectionString = PlayerPrefs.GetString("CardCollection", "");

        // assert value
        if (string.IsNullOrEmpty(collectionString)) return sums;

        // iterate through collection
        string[] cardEncoded = collectionString.Split(',');

        // Search for the card in the collection and return quantity
        for (int i = 0; i < cardEncoded.Length; i++)
        {
            if (int.TryParse(cardEncoded[i], out int encodedCard))
            {
                var decodedCard = DecodeCard(encodedCard);

                sums[decodedCard.cardIndex] += decodedCard.quantity;
            }
        }

        return sums;
    }

    public static List<CardVariation> GetAllVariations(int cardIndex)
    {
        List<CardVariation> cardVariationList = new List<CardVariation>();

        // Get the card collection
        string collectionString = PlayerPrefs.GetString("CardCollection", "");

        // assert value
        if (string.IsNullOrEmpty(collectionString)) return cardVariationList;

        // iterate through collection
        string[] cardEncoded = collectionString.Split(',');

        // Search for the card in the collection and return quantity
        for (int i = 0; i < cardEncoded.Length; i++)
        {
            if (int.TryParse(cardEncoded[i], out int encodedCard))
            {
                var decodedCard = DecodeCard(encodedCard);
                if (decodedCard.cardIndex == cardIndex)
                {
                    // Add the card variation to the list
                    cardVariationList.Add(new CardVariation(decodedCard.rank, decodedCard.holo, decodedCard.quantity));
                }
            }
        }

        // sort by rank, then holo
        cardVariationList = cardVariationList.OrderBy(card => card.rank).ThenBy(card => card.holo).ToList();

        return cardVariationList;
    }

    // return success (if fail because over max count, reimburse with more credits)
    public static void AddCardToCollection(int cardIndex, int rank = 0, bool holo = false, int count = 1)
    {
        if (cardIndex < 0) { return; }

        // Get the current collection string
        string collectionString = PlayerPrefs.GetString("CardCollection", "");

        // Split the collection string into individual encoded card values
        string[] cardEncoded = string.IsNullOrEmpty(collectionString) ? new string[0] : collectionString.Split(',');

        // Encode the card information for the given index
        bool cardFound = false;

        // Search for the card in the collection and update its quantity
        for (int i = 0; i < cardEncoded.Length; i++)
        {
            if (int.TryParse(cardEncoded[i], out int encodedCard))
            {
                var decodedCard = DecodeCard(encodedCard);
                if (decodedCard.cardIndex == cardIndex && decodedCard.rank == rank && decodedCard.holo == holo)
                {
                    // Add the count to the current quantity
                    var updatedQuantity = decodedCard.quantity + count;

                    // If the quantity is valid, re-encode and update the card
                    if (updatedQuantity >= 0)
                    {
                        cardEncoded[i] = EncodeCard(cardIndex, rank, holo, updatedQuantity).ToString();
                        cardFound = true;
                    }
                    break;
                }
            }
        }

        // Join the updated card collection and save it back to PlayerPrefs
        string updatedCollectionString = string.Join(",", cardEncoded);

        // If the card wasn't found, add it as a new card with the given quantity
        if (!cardFound && count > 0)
        {
            int newEncodedCard = EncodeCard(cardIndex, rank, holo, count);
            updatedCollectionString += "," + newEncodedCard.ToString();
        }

        PlayerPrefs.SetString("CardCollection", updatedCollectionString);
        PlayerPrefs.Save();
    }

    private static int[] cardRarities = { 0, 0, 0, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 0, 2, 2, 2, 1, 2, 2, 3, 3, 3, 1, 0, 0, 0, 1, 4, -1 };
    public static int GetCardRarity(int index) { return cardRarities[index]; }
    public static int GetRandomCardOfRarity(int rarity)
    {
        // Create a list to store indices that match the requested rarity
        List<int> matchingIndices = new List<int>();

        // Loop through the cardRarities array to find matching indices
        for (int i = 0; i < cardRarities.Length; i++)
        {
            if (cardRarities[i] == rarity)
            {
                matchingIndices.Add(i);
            }
        }

        // If no matching indices were found, return 0 (plus one)
        if (matchingIndices.Count == 0)
        {
            return 0;
        }

        // Return a random index from the matching indices list
        int rand = Random.Range(0, matchingIndices.Count);
        return matchingIndices[rand];
    }

    public static int EncodeCard(int cardIndex, int rank, bool holo, int quantity)
    {
        if (quantity < 0 || quantity > 4095) // 2^12 - 1
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be between 0 and 4095.");
        if (rank < 0 || rank > 7) // 2^3 - 1
            throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be between 0 and 7.");
        if (cardIndex < 0 || cardIndex > 255) // 2^8 - 1
            throw new ArgumentOutOfRangeException(nameof(cardIndex), "CardIndex must be between 0 and 255.");

        int encodedValue = 0;

        // Encode quantity (12 bits)
        encodedValue |= quantity;

        // Encode holo (1 bit)
        encodedValue |= (holo ? 1 : 0) << 12;

        // Encode rank (3 bits)
        encodedValue |= rank << 13;

        // Encode cardIndex (8 bits)
        encodedValue |= cardIndex << 16;

        return encodedValue;
    }

    public static (int cardIndex, int rank, bool holo, int quantity) DecodeCard(int encodedValue)
    {
        // Extract quantity (12 bits)
        int quantity = encodedValue & 0xFFF; // 0xFFF = 4095

        // Extract holo (1 bit)
        bool holo = ((encodedValue >> 12) & 1) == 1;

        // Extract rank (3 bits)
        int rank = (encodedValue >> 13) & 0x7; // 0x7 = 7

        // Extract cardIndex (8 bits)
        int cardIndex = (encodedValue >> 16) & 0xFF; // 0xFF = 255

        return (cardIndex, rank, holo, quantity);
    }

    public static void EncodeDecodeTest()
    {
        int[] IDs = { 0, 1, 2, 3, 4, 0 };
        int[] ranks = { 0, 1, 2, 3, 4, 1 };
        bool[] holos = { false, true, false, true, true, false };
        int[] quantities = { 0, 1, 2, 3, 4, 2 };

        for (int i = 0; i < IDs.Length; i++)
        {
            var encoded = EncodeCard(IDs[i], ranks[i], holos[i], quantities[i]);
            string encodedString = encoded.ToString();

            if (int.TryParse(encodedString, out int encodedCard))
            {
                (int effectID, int rank, bool holo, int quantity) = DecodeCard(encodedCard);
                if (effectID != IDs[i] || rank != ranks[i] || holo != holos[i] || quantity != quantities[i])
                {
                    Debug.LogError("ENCODE/DECODE FAIL");
                }
            }
        }
    }
}
