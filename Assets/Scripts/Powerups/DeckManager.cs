using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    // self
    public static DeckManager Instance;

    [SerializeField] private TMP_Text deckCount;
    private int[] deck;

    [SerializeField] private GameObject cardPreviewImage;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void Start()
    {
        deck = new int[PowerupManager.Instance.GetMethodArrayLength()];
        // load deck from prefs here
        for (int i = 0; i < PowerupManager.Instance.GetMethodArrayLength(); i++)
        {
            deck[i] = 1;
        }
    }

    public void SetCount(int index, int count)
    {
        deck[index] = count;
        UpdateDeckCount();
    }

    public int GetCount(int index)
    {
        return deck[index];
    }

    public void UpdateDeckCount()
    {
        var sum = deck.Sum();
        deckCount.text = sum.ToString() + "/10";
        if (sum < 10)
        {
            deckCount.color = new Color(0.9490197f, 0.4235294f, 0.3098039f);
        }
        else
        {
            deckCount.color = new Color(0.4862745f, 0.7725491f, 0.4627451f);
        }
    }

    public List<int> GetDeck()
    {
        // make sure our decklist is valid
        var sum = deck.Sum();
        if (sum < 10)
        {
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

    public void SetCardPreviewImage(Sprite img)
    {
        cardPreviewImage.SetActive(img != null);
        cardPreviewImage.GetComponent<Image>().sprite = img;
    }
}
