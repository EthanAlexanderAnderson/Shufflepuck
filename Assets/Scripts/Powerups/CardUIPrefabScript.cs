using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUIPrefabScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameObject deckMenuScrollView;
    //private float deckMenuScrollViewTargetYPosition;

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject header;
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject craftingParent;
    [SerializeField] private GameObject minusButtonObject;
    [SerializeField] private Button minusButton;
    [SerializeField] private GameObject plusButtonObject;
    [SerializeField] private Button plusButton;

    private int index;     // which card is this

    [SerializeField] private Image cardIcon;
    private string cardName;
    [SerializeField] private TMP_Text cardNameText;

    private int count;     // how many do we have in deck currently
    [SerializeField] private TMP_Text countText;
    private int maxCount;  // how many are we allowed to have in deck

    private Sprite cardImageSprite;
    [SerializeField] private Image cardImage;

    // --- DIVIDERS
    [SerializeField] private Sprite inDeckSprite;
    [SerializeField] private Sprite collectionSprite;
    [SerializeField] private Sprite undiscoveredSprite;

    // --- RANKS
    private int selectedRankIndex;
    private int[] rankToCraftCosts = new int[] { 10, 0, 100, 0, 200, 0, 400, 0, 0, 0 };
    private Color[] rankColors = new Color[] { new Color(0.9f, 0.9f, 0.9f, 1f), new Color(0.9f, 0.9f, 0.9f, 1f), new Color(0.9f, 0.4f, 0f, 1f), new Color(0.9f, 0.4f, 0f, 1f), new Color(0.95f, 0.7f, 0f, 1f), new Color(0.95f, 0.7f, 0f, 1f), new Color(0f, 1f, 1f, 1f), new Color(0f, 1f, 1f, 1f), new Color(1f, 0f, 1f, 1f), new Color(1f, 0f, 1f, 1f) };

    private bool anyOwned;
    private List<int> ownedRanks = new();

    [SerializeField] private Button previousRankButton;
    [SerializeField] private Button nextRankButton;
    // for shiny effect
    [SerializeField] private GameObject particleParent;
    [SerializeField] private GameObject shine;
    // for holo effect
    [SerializeField] private GameObject holoParent;

    // --- CRAFTING
    private int craftingCredits;
    private int[] ownedCounts = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private int sumOwned;

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

    // Expand / Collapse

    [SerializeField] private GameObject expandCollapseObject;
    [SerializeField] private Sprite expandSprite;
    [SerializeField] private Sprite collapseSprite;
    private bool expanded = false;

    public void InitializeCardUI(int index, GameObject deckMenuScrollView, bool anyOwned = false)
    {
        this.index = index;
        this.deckMenuScrollView = deckMenuScrollView;
        this.anyOwned = anyOwned;

        // auto-collapse
        body.transform.localScale = new Vector3(body.transform.localScale.x, 0, body.transform.localScale.z);

        // Divider
        if (index < 0)
        {
            switch (index)
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
            // swap BG color
            background.GetComponent<Image>().color = UIManagerScript.Instance.GetDarkMode() ? new Color(1f, 1f, 1f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f);
            return;
        }

        maxCount = 5 - PowerupCardData.GetCardRarity(index); // (has to be after divider check to avoid negative index error)

        // load card data
        cardIcon.sprite = PowerupManager.Instance.powerupIcons[index];

        cardName = PowerupCardData.GetCardName(index);
        cardNameText.text = PowerupManager.Instance.powerupTexts[index];

        cardImageSprite = PowerupManager.Instance.powerupSprites[index];
        cardImage.sprite = cardImageSprite;

        // load deck data
        count = PlayerPrefs.GetInt(cardName + "CardCount", 0);
        if (!Application.isEditor && count > maxCount) // in editor we have no max per card
        {
            count = maxCount;
        }
        DeckManager.Instance.SetCardCount(index, count);
        countText.text = count.ToString();
        countText.alpha = count > 0 ? 1f : 0.3f;

        // load crafting data
        craftingCredits = PlayerPrefs.GetInt("CraftingCredits");
        ownedCount2.text = craftingCredits.ToString();

        // set background color for light/dark mode
        background.GetComponent<Image>().color = UIManagerScript.Instance.GetDarkMode() ? new Color(0.2f, 0.2f, 0.2f, 1f) : new Color(1f, 1f, 1f, 1f);

        // TODO: make crafting re-enable these
        if (!anyOwned)
        {
            minusButtonObject.SetActive(false);
            plusButtonObject.SetActive(false);
            countText.text = "";
            craftingParent.SetActive(false);
        }
        // check owned
            string cardIDString = PowerupCardData.GetCardName(index);
        // add base card
        ownedRanks.Add(0);
        ownedCounts[0] = PlayerPrefs.GetInt($"{cardIDString}CardOwned");
        // add other ranks / holos
        int len = rankColors.Length;
        bool holo = true;
        string[] rankStrings = { "", "", "Bronze", "Bronze", "Gold", "Gold", "Diamond", "Diamond", "Celestial", "Celestial" };
        for (int i = 1; i < 10 ; i++)
        {
            string holoString = holo ? "Holo" : "";
            int count = PlayerPrefs.GetInt($"{cardIDString}CardOwned{rankStrings[i]}{holoString}");
            if (count > 0)
            {
                ownedRanks.Add(i);
                ownedCounts[i] = count;
            }
            holo = !holo;
        }

        // calc sum owned
        for (int i = 0; i < ownedCounts.Length; i++)
        {
            sumOwned += ownedCounts[i];
        }

        UpdateRankUI();
        UpdateMinusAndPlusUIButtonInteractability();
    }

    public void Minus()
    {
        if (count > 0)
        {
            count--;
        }
        UpdateMinusAndPlusUIButtonInteractability();
        UpdateAndSaveCardCount();
    }

    public void Plus()
    {
        //if (Application.isEditor || (count < maxCount && count < sumOwned)) // in editor we have no max per card
        if (count < maxCount && count < sumOwned)
        {
            count++;
        }
        UpdateMinusAndPlusUIButtonInteractability();
        UpdateAndSaveCardCount();
    }

    // Add/Remove card from the deck, update the UI, save the new count to playerprefs
    private void UpdateAndSaveCardCount()
    {
        DeckManager.Instance.SetCardCount(index, count);
        countText.text = count.ToString();

        // if zero cards, grey out
        countText.alpha = count > 0 ? 1f : 0.3f;

        PlayerPrefs.SetInt(cardName + "CardCount", count);
    }

    private void UpdateMinusAndPlusUIButtonInteractability()
    {
        minusButton.interactable = (count > 0);
        //plusButton.interactable = !(count < maxCount && count < sumOwned && !Application.isEditor);
        plusButton.interactable = (count < maxCount && count < sumOwned);
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
    private float expandedHeaderYPositionUnowned = 230f;
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
            LeanTween.scaleY(body, 0f, expandAnimationTime).setEaseOutQuint();
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
            selectedRankIndex = 0;
            selectedOwnedIndex = 0;
            UpdateRankUI();
            toCraftCount = 0;
            UpdateCraftCountUI();
            expanded = false;
        }
    }

    int selectedOwnedIndex = 0;
    // RANKS
    public void PreviousRank()
    {
        if (selectedOwnedIndex > 0)
        {
            selectedOwnedIndex--;
            selectedRankIndex = ownedRanks[selectedOwnedIndex];
        }
        UpdateRankUI();
    }

    public void NextRank()
    {
        if (selectedOwnedIndex < ownedRanks.Count - 1) // TODO: set upper limit based on credits
        {
            selectedOwnedIndex++;
            selectedRankIndex = ownedRanks[selectedOwnedIndex];
        }
        UpdateRankUI();
    }

    private void UpdateRankUI()
    {
        // owned count
        ownedCount1.text = ownedCounts[selectedRankIndex].ToString();

        // rank color (standard, bronze, gold, diamond)
        cardImage.color = rankColors[selectedRankIndex];

        // turn shine & sparkle off then on to retrigger effect
        shine.SetActive(false);
        particleParent.SetActive(false);
        shine.SetActive(selectedRankIndex > 1);
        particleParent.SetActive(selectedRankIndex > 1);

        holoParent.SetActive(selectedRankIndex > 0 && selectedRankIndex%2 == 1);

        // update UI arrows
        previousRankButton.image.enabled = (selectedRankIndex > 0);
        previousRankButton.interactable = (selectedRankIndex > 0);
        nextRankButton.image.enabled = (selectedRankIndex < ownedRanks.Count - 1);
        nextRankButton.interactable = (selectedRankIndex < ownedRanks.Count - 1);

        // reset crafting
        toCraftCount = 0;
        UpdateCraftCountUI();
    }

    // CRAFTING
    public void MinusCraft()
    {
        if (toCraftCount > 0)
        {
            toCraftCount--;
        }
        //UpdateMinusAndPlusCraftUIButtonInteractability();
        UpdateCraftCountUI();
    }

    public void PlusCraft()
    {
        toCraftCount++;
        //UpdateMinusAndPlusCraftUIButtonInteractability();
        UpdateCraftCountUI();
    }

    private void UpdateCraftCountUI()
    {
        // text
        craftArrow1.enabled = (toCraftCount > 0);
        craftArrow2.enabled = (toCraftCount > 0);
        craftCount1.text = (toCraftCount > 0) ? (ownedCounts[selectedRankIndex] + toCraftCount).ToString() : "";
        int remainingCredits = (craftingCredits - rankToCraftCosts[selectedRankIndex] * toCraftCount);
        craftCount2.text = (toCraftCount > 0) ? remainingCredits.ToString() : "";

        // buttons
        craftMinusButton.interactable = (toCraftCount > 0);
        craftConfirmButton.interactable = (toCraftCount > 0 && remainingCredits > 0);
        craftPlusButton.interactable = ((remainingCredits - rankToCraftCosts[selectedRankIndex]) > 0 && rankToCraftCosts[selectedRankIndex] > 0);
    }
}
