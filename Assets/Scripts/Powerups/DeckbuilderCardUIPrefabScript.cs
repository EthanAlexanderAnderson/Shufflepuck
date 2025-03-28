using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class DeckbuilderCardUIPrefabScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // --- GENERAL DATA
    private GameObject deckMenuScrollView;
    [SerializeField] private GameObject background;

    private int cardIndex; // what card this is
    private int cardRarity;// what rarity this card is
    private bool anyOwned; // do we own any of these cards
    private int totalOwned; // how many we own of any variation total

    private int[] inDeckCounts;  // how many of each variation do we have in deck currently, parallel to ownedCardVariationList
    private int maxCount;  // how many are we allowed to have in deck

    private int[] rankToCraftCosts = new int[] { 10, 100, 200, 400, int.MaxValue };


    // --- HEADER
    [SerializeField] private GameObject header;
    [SerializeField] private GameObject minusButtonObject;
    [SerializeField] private Button minusButton;
    [SerializeField] private GameObject plusButtonObject;
    [SerializeField] private Button plusButton;

    [SerializeField] private Image cardIcon;
    [SerializeField] private TMP_Text cardNameText;

    [SerializeField] private TMP_Text countText;


    // --- BODY
    [SerializeField] private GameObject body;
    private void DisableBody() { body.SetActive(false); }

    // for hold preview
    private Sprite cardImageSprite;

    // card image
    [SerializeField] private GameObject cardUIPrefab;
    private CardUIPrefabScript cardUIPrefabScript;


    // --- DIVIDERS
    [SerializeField] private Sprite inDeckSprite;
    [SerializeField] private Sprite collectionSprite;
    [SerializeField] private Sprite undiscoveredSprite;


    // --- RANKS
    private List<CardVariation> ownedCardVariationList; // how many we own of each rank/holo
    int selectedCardVariationIndex;

    [SerializeField] private Button previousRankButton;
    [SerializeField] private Button nextRankButton;


    // --- CRAFTING
    [SerializeField] private GameObject craftingParent;
    private int craftingCredits;

    [SerializeField] private Button craftMinusButton;
    [SerializeField] private Button craftConfirmButton;
    [SerializeField] private Button craftPlusButton;

    [SerializeField] private TMP_Text ownedText1;
    [SerializeField] private TMP_Text ownedCount1;
    [SerializeField] private Image craftArrow1;
    [SerializeField] private TMP_Text craftCount1;

    [SerializeField] private TMP_Text ownedText2;
    [SerializeField] private TMP_Text ownedCount2;
    [SerializeField] private Image craftArrow2;
    [SerializeField] private TMP_Text craftCount2;
    [SerializeField] private Image craftCreditsImage2;

    private int toCraftCount;


    // --- EXPAND / COLLASPSE BUTTONS
    [SerializeField] private GameObject expandCollapseObject;
    [SerializeField] private Sprite expandSprite;
    [SerializeField] private Sprite collapseSprite;
    private bool expanded = false;


    public void InitializeDeckbuilderCardUI(int cardIndex, GameObject deckMenuScrollView, bool anyOwned = false)
    {
        this.cardIndex = cardIndex;
        this.deckMenuScrollView = deckMenuScrollView;
        this.anyOwned = anyOwned;

        // Divider
        // note: everything checking index has to be under the divider stuff to prevent negative index errors
        if (cardIndex < 0)
        {
            switch (cardIndex)
            {
                case -1:
                    cardNameText.text = "in deck:";
                    cardIcon.sprite = inDeckSprite;
                    break;
                case -2:
                    cardNameText.text = "collection:";
                    cardIcon.sprite = collectionSprite;
                    break;
                case -3:
                    cardNameText.text = "undiscovered:";
                    cardIcon.sprite = undiscoveredSprite;
                    break;
                default:
                    cardNameText.text = "ERROR";
                    break;
            }

            cardNameText.color = UIManagerScript.Instance.GetDarkMode() ? new Color(0f, 0f, 0f, 1f) : new Color(1f, 1f, 1f, 1f);
            cardNameText.gameObject.tag = "Untagged";
            cardIcon.color = UIManagerScript.Instance.GetDarkMode() ? new Color(0f, 0f, 0f, 1f) : new Color(1f, 1f, 1f, 1f);
            cardIcon.gameObject.tag = "Untagged";
            minusButtonObject.SetActive(false);
            plusButtonObject.SetActive(false);
            expandCollapseObject.SetActive(false);
            countText.text = "";
            background.GetComponent<Image>().color = UIManagerScript.Instance.GetDarkMode() ? new Color(1f, 1f, 1f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f);
            return;
        }

        // BACKGROUND color for light/dark mode
        background.GetComponent<Image>().color = UIManagerScript.Instance.GetDarkMode() ? new Color(0.2f, 0.2f, 0.2f, 1f) : new Color(1f, 1f, 1f, 1f);


        // HEADER data load
        cardIcon.sprite = PowerupManager.Instance.powerupIcons[cardIndex];
        cardNameText.text = PowerupManager.Instance.powerupTexts[cardIndex];
        cardImageSprite = PowerupManager.Instance.powerupSprites[cardIndex];

        // TODO: make crafting re-enable these
        if (!anyOwned)
        {
            minusButtonObject.SetActive(false);
            plusButtonObject.SetActive(false);
            countText.text = "";
            craftingParent.SetActive(false);
        }


        // BODY instantiate card UI prefab as child of body, intialize it as base card
        GameObject cardUIObject = Instantiate(cardUIPrefab, body.transform);
        cardUIPrefabScript = cardUIObject.GetComponent<CardUIPrefabScript>();
        cardUIPrefabScript.InitializeCardUI(cardIndex);

        cardRarity = PowerupCardData.GetCardRarity(cardIndex);
        maxCount = 5 - cardRarity;

        // load owned card data
        ownedCardVariationList = PowerupCardData.GetAllVariations(cardIndex);
        if (ownedCardVariationList.Count <= 0)
        {
            ownedCardVariationList = new();
            ownedCardVariationList.Add(new CardVariation(0, false, 0));
        }
        // TODO: crafting should update totalOwned
        totalOwned = ownedCardVariationList.Sum(card => card.count);
        inDeckCounts = new int[ownedCardVariationList.Count];

        // load crafting data
        craftingCredits = PlayerPrefs.GetInt("CraftingCredits");
        ownedCount2.text = craftingCredits.ToString();

        // load indeck card data 
        if (anyOwned)
        {
            for (int i = 0; i < ownedCardVariationList.Count; i++)
            {
                inDeckCounts[i] = DeckManager.Instance.GetInDeckCount(cardIndex, ownedCardVariationList[i].rank, ownedCardVariationList[i].holo);
            }

            int sum = inDeckCounts.Sum();
            countText.text = sum.ToString();
            countText.alpha = sum > 0 ? 1f : 0.3f;

            UpdateSelectedCardVariationIndex();
            UpdateRankUI();
            UpdateMinusAndPlusUIButtonInteractability();
        }
    }

    public void Minus()
    {
        // remove one from the deck
        if (inDeckCounts[selectedCardVariationIndex] > 0)
        {
            inDeckCounts[selectedCardVariationIndex]--;
        }
        // if UI is collapsed & currently selected has zero count & we have a card still in deck to remove, try to decrement from lowest in-deck rarity
        else if (!expanded && inDeckCounts[selectedCardVariationIndex] <= 0 && inDeckCounts.Sum() > 0)
        {
            for (int i = 0; i < inDeckCounts.Length; i++)
            {
                if (inDeckCounts[i] > 0)
                {
                    selectedCardVariationIndex = i;
                    inDeckCounts[selectedCardVariationIndex]--;
                    break;
                }
            }
        }
        SaveCardCountToDeckManager();
        UpdateCardCountUI();
        UpdateMinusAndPlusUIButtonInteractability();
        SoundManagerScript.Instance.PlayClickSFX(5);
    }

    public void Plus()
    {
        // add one to the deck
        if (inDeckCounts.Sum() < maxCount && inDeckCounts[selectedCardVariationIndex] < ownedCardVariationList[selectedCardVariationIndex].count)
        {
            inDeckCounts[selectedCardVariationIndex]++;
        }
        // if UI is collapsed & currently selected has all owned equipped & we have a card still owned to add, swap selected variation so we add the highest addable rank
        else if (!expanded && inDeckCounts[selectedCardVariationIndex] >= ownedCardVariationList[selectedCardVariationIndex].count && inDeckCounts[selectedCardVariationIndex] < totalOwned)
        {
            for (int i = (inDeckCounts.Length - 1); i >= 0; i--)
            {
                if (ownedCardVariationList[i].count > 0 && inDeckCounts[i] < ownedCardVariationList[i].count)
                {
                    selectedCardVariationIndex = i;
                    inDeckCounts[selectedCardVariationIndex]++;
                    break;
                }
            }
        }
        SaveCardCountToDeckManager();
        UpdateCardCountUI();
        UpdateMinusAndPlusUIButtonInteractability();
        SoundManagerScript.Instance.PlayClickSFX(4);
    }

    private void UpdateCardCountUI()
    {
        if (!anyOwned) { return; }

        if (!expanded)
        {
            var sum = inDeckCounts.Sum();
            countText.text = sum.ToString();
            // if zero cards, grey out
            countText.alpha = sum > 0 ? 1f : 0.3f;
        }
        else
        {
            countText.text = inDeckCounts[selectedCardVariationIndex].ToString();
            // if zero cards, grey out
            countText.alpha = inDeckCounts[selectedCardVariationIndex] > 0 ? 1f : 0.3f;
        }
    }

    private void SaveCardCountToDeckManager()
    {
        DeckManager.Instance.SetCardCount(cardIndex, ownedCardVariationList[selectedCardVariationIndex].rank, ownedCardVariationList[selectedCardVariationIndex].holo, inDeckCounts[selectedCardVariationIndex]);
    }

    private void UpdateMinusAndPlusUIButtonInteractability()
    {
        // if collapsed, check total owned, not per variation
        if (!expanded)
        {
            minusButton.interactable = (inDeckCounts.Sum() > 0);
            plusButton.interactable = (inDeckCounts.Sum() < maxCount && inDeckCounts.Sum() < totalOwned);
        }
        else
        {
            minusButton.interactable = (inDeckCounts[selectedCardVariationIndex] > 0);
            plusButton.interactable = (inDeckCounts.Sum() < maxCount && inDeckCounts[selectedCardVariationIndex] < ownedCardVariationList[selectedCardVariationIndex].count);
        }
    }

    // Called when the user presses down on the image
    public void OnPointerDown(PointerEventData eventData)
    {
        if (cardImageSprite != null)
        {
            DeckManager.Instance.SetCardPreviewImage(cardImageSprite);
        }
    }

    // Called when the user releases their touch
    public void OnPointerUp(PointerEventData eventData)
    {
        if (cardImageSprite != null)
        {
            DeckManager.Instance.SetCardPreviewImage(null);
        }
    }

    private float expandAnimationTime = 0.5f;

    private float expandedlayoutElementHeight = 750f;
    private float expandedBackgroundScale = 4f;
    private float expandedHeaderYPosition = 375f;
    private float collapsedCardImageYPosition = -125f;

    private float expandedlayoutElementHeightUnowned = 500f;
    private float expandedBackgroundScaleUnowned = 3f;
    private float expandedHeaderYPositionUnowned = 250f;
    private float expandedBodyYPositionUnowned = -100f;

    // Toggle expanded UI
    public void ToggleExpand()
    {
        // EXPAND UI
        if (!expanded)
        {
            // swap arrow to X
            expandCollapseObject.GetComponent<Image>().sprite = collapseSprite;
            // Expand background & container
            LeanTween.scaleY(background, anyOwned ? expandedBackgroundScale : expandedBackgroundScaleUnowned, expandAnimationTime).setEaseOutQuint();
            LayoutElement layoutElement = GetComponent<LayoutElement>();
            LeanTween.value(gameObject, layoutElement.preferredHeight, anyOwned ? expandedlayoutElementHeight : expandedlayoutElementHeightUnowned, expandAnimationTime)
            .setOnUpdate((float value) =>
            {
                layoutElement.preferredHeight = value;
            }).setEaseOutQuint();
            // Slide header up
            LeanTween.moveLocalY(header, anyOwned ? expandedHeaderYPosition : expandedHeaderYPositionUnowned, expandAnimationTime).setEaseOutQuint();
            // Expand body / card image
            LeanTween.cancel(body);
            body.SetActive(true);
            UIManagerScript.Instance.ApplyDarkMode(body);
            LeanTween.scaleY(body, 1f, expandAnimationTime).setEaseOutQuint();
            LeanTween.moveLocalY(body, anyOwned ? 0f : expandedBodyYPositionUnowned, expandAnimationTime).setEaseOutQuint();
            // slide scrollview up
            var deckMenuScrollViewCurrentYPosition = deckMenuScrollView.transform.position.y;
            var deckMenuScrollViewTargetYPosition = deckMenuScrollView.transform.position.y - (anyOwned ? 3.5f : 2.3f);
            LeanTween.value(deckMenuScrollView, deckMenuScrollViewCurrentYPosition, deckMenuScrollViewTargetYPosition, expandAnimationTime)
            .setOnUpdate((float value) =>
            {
                deckMenuScrollView.transform.position = new Vector3(deckMenuScrollView.transform.position.x, value);
            }).setEaseOutQuint();
            expanded = true;
        }
        // COLLAPSE UI
        else
        {
            // swap X to arrow
            expandCollapseObject.GetComponent<Image>().sprite = expandSprite;
            // Collapse background & container
            LeanTween.scaleY(background, 1, expandAnimationTime).setEaseOutQuint();
            LayoutElement layoutElement = GetComponent<LayoutElement>();
            LeanTween.value(gameObject, layoutElement.preferredHeight, 0f, expandAnimationTime)
            .setOnUpdate((float value) =>
            {
                layoutElement.preferredHeight = value;
            }).setEaseOutQuint();
            // Slide header down
            LeanTween.moveLocalY(header, 0f, expandAnimationTime).setEaseOutQuint();
            // Collapse card image
            LeanTween.scaleY(body, 0f, expandAnimationTime).setEaseOutQuint().setOnComplete(DisableBody);
            LeanTween.moveLocalY(body, collapsedCardImageYPosition, expandAnimationTime).setEaseOutQuint();
            // slide scrollview down
            var deckMenuScrollViewCurrentYPosition = deckMenuScrollView.transform.position.y;
            var deckMenuScrollViewTargetYPosition = deckMenuScrollView.transform.position.y + (anyOwned ? 3.5f : 2.3f);
            LeanTween.value(deckMenuScrollView, deckMenuScrollViewCurrentYPosition, deckMenuScrollViewTargetYPosition, expandAnimationTime)
            .setOnUpdate((float value) =>
            {
                deckMenuScrollView.transform.position = new Vector3(deckMenuScrollView.transform.position.x, value);
            }).setEaseOutQuint();
            // reset rank & craft UI
            UpdateSelectedCardVariationIndex();
            toCraftCount = 0;
            UpdateCraftUI();
            expanded = false;
        }
        UpdateRankUI();
        UpdateCardCountUI();
        UpdateMinusAndPlusUIButtonInteractability();
        SoundManagerScript.Instance.PlayClickSFX(0);
    }

    // RANKS
    public void PreviousRank()
    {
        if (selectedCardVariationIndex > 0)
        {
            selectedCardVariationIndex--;
        }
        UpdateRankUI();
        UpdateCardCountUI();
        UpdateMinusAndPlusUIButtonInteractability();
        SoundManagerScript.Instance.PlayClickSFX(5);
    }

    public void NextRank()
    {
        if (selectedCardVariationIndex < ownedCardVariationList.Count - 1) // TODO: set upper limit based on credits
        {
            selectedCardVariationIndex++;
        }
        UpdateRankUI();
        UpdateCardCountUI();
        UpdateMinusAndPlusUIButtonInteractability();
        SoundManagerScript.Instance.PlayClickSFX(4);
    }

    private void UpdateRankUI()
    {
        if (cardIndex < 0) return;
        if (ownedCardVariationList == null) return;
        if (selectedCardVariationIndex > ownedCardVariationList.Count - 1 || ownedCardVariationList[selectedCardVariationIndex] == null) UpdateSelectedCardVariationIndex();


        // INITIALIZE (cardIndex, rank, holo)
        cardUIPrefabScript.InitializeCardUI(cardIndex, ownedCardVariationList[selectedCardVariationIndex].rank, ownedCardVariationList[selectedCardVariationIndex].holo);

        // COUNT
        ownedCount1.text = ownedCardVariationList[selectedCardVariationIndex].count.ToString();

        // update UI arrows
        previousRankButton.image.enabled = (selectedCardVariationIndex > 0);
        previousRankButton.interactable = (selectedCardVariationIndex > 0);
        nextRankButton.image.enabled = (selectedCardVariationIndex < ownedCardVariationList.Count - 1);
        nextRankButton.interactable = (selectedCardVariationIndex < ownedCardVariationList.Count - 1);

        // reset crafting
        toCraftCount = 0;
    }

    // CRAFTING
    public void MinusCraft()
    {
        if (toCraftCount > 0)
        {
            toCraftCount--;
        }
        UpdateCraftUI();
        SoundManagerScript.Instance.PlayClickSFX(5);
    }

    public void PlusCraft()
    {
        toCraftCount++;
        UpdateCraftUI();
        SoundManagerScript.Instance.PlayClickSFX(4);
    }

    private void UpdateCraftUI()
    {
        // holos are uncraftable
        if (ownedCardVariationList[selectedCardVariationIndex].holo)
        {
            craftArrow1.enabled = false;
            craftArrow2.enabled = false;
            craftCount1.text = "";
            craftCount2.text = "";

            // buttons
            craftMinusButton.interactable = false;
            craftConfirmButton.interactable = false;
            craftPlusButton.interactable = false;
            return;
        }

        int craftCost = rankToCraftCosts[ownedCardVariationList[selectedCardVariationIndex].rank];

        // text
        craftArrow1.enabled = (toCraftCount > 0);
        craftArrow2.enabled = (toCraftCount > 0);
        craftCount1.text = (toCraftCount > 0) ? (ownedCardVariationList[selectedCardVariationIndex].count + toCraftCount).ToString() : "";
        int remainingCredits = (craftingCredits - craftCost * toCraftCount);
        craftCount2.text = (toCraftCount > 0) ? remainingCredits.ToString() : "";

        // buttons
        craftMinusButton.interactable = (toCraftCount > 0);
        craftConfirmButton.interactable = (toCraftCount > 0 && remainingCredits > 0);
        craftPlusButton.interactable = ((remainingCredits - craftCost) > 0 && craftCost > 0);
    }

    private void UpdateSelectedCardVariationIndex()
    {
        // default to highest owned variation
        selectedCardVariationIndex = ownedCardVariationList.Count - 1;
        // if deck is non-empty, default to highest in-deck variation
        for (int i = inDeckCounts.Length - 1; i >= 0; i--)
        {
            if (inDeckCounts[i] > 0)
            {
                selectedCardVariationIndex = i;
                break;
            }
        }
    }
}
