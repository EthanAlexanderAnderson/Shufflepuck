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
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;

    private int index;     // which card is this

    [SerializeField] private Image cardIcon;
    private string cardName;
    [SerializeField] private TMP_Text cardNameText;

    private int count;     // how many do we have in deck currently
    [SerializeField] private TMP_Text countText;
    private int maxCount = 3;  // how many are we allowed to have in deck

    private Sprite cardImageSprite;
    [SerializeField] private Image cardImage;

    // --- RANKS
    private int selectedRankIndex;
    private int[] rankToCraftCosts = new int[] { 10, 100, 200, 400 };
    private Color[] rankColors = new Color[] { new Color(1f, 1f, 1f, 1f), new Color(1f, 0.5f, 0f, 1f), new Color(1f, 1f, 0f, 1f), new Color(0f, 1f, 1f, 1f) };
    [SerializeField] private Button previousRankButton;
    [SerializeField] private Button nextRankButton;
    // for shiny effect
    [SerializeField] private GameObject particleParent;
    [SerializeField] private GameObject shine;

    // --- CRAFTING
    private int owned;
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

    [SerializeField] private GameObject expandCollapseObject;
    [SerializeField] private Sprite expandSprite;
    [SerializeField] private Sprite collapseSprite;
    private bool expanded = false;

    public void InitializeCardUI(int index, GameObject deckMenuScrollView)
    {
        this.index = index;
        this.deckMenuScrollView = deckMenuScrollView;

        cardIcon.sprite = PowerupManager.Instance.powerupIcons[index];

        cardName = PowerupCardData.GetCardName(index);
        cardNameText.text = PowerupManager.Instance.powerupTexts[index];

        cardImageSprite = PowerupManager.Instance.powerupSprites[index];
        cardImage.sprite = cardImageSprite;

        count = PlayerPrefs.GetInt(cardName + "CardCount", 0);
        if (!Application.isEditor && count > maxCount) // in editor we have no max per card
        {
            count = maxCount;
        }
        DeckManager.Instance.SetCardCount(index, count);
        countText.text = count.ToString();
        countText.alpha = count > 0 ? 1f : 0.3f;
        UpdateMinusAndPlusUIButtonInteractability();

        body.transform.localScale = new Vector3(body.transform.localScale.x, 0, body.transform.localScale.z);

        craftingCredits = PlayerPrefs.GetInt("CraftingCredits");
        ownedCount2.text = craftingCredits.ToString();
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
        if (Application.isEditor || count < maxCount) // in editor we have no max per card
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
        plusButton.interactable = !(count >= maxCount && !Application.isEditor);
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
    // Toggle expanded UI
    public void ToggleExpand()
    {
        if (!expanded)
        {
            // swap arrow to X
            expandCollapseObject.GetComponent<Image>().sprite = collapseSprite;
            // Expand background & container
            LeanTween.scaleY(background, expandedBackgroundScale, expandAnimationTime).setEaseOutQuint();
            LayoutElement layoutElement = GetComponent<LayoutElement>();
            LeanTween.value(gameObject, layoutElement.preferredHeight, expandedlayoutElementHeight, expandAnimationTime)
            .setOnUpdate((float value) =>
            {
                layoutElement.preferredHeight = value;
            }).setEaseOutQuint();
            // Slide header up
            LeanTween.moveLocalY(header, expandedHeaderYPosition, expandAnimationTime).setEaseOutQuint();
            // Expand card image
            LeanTween.scaleY(body, 1f, expandAnimationTime).setEaseOutQuint();
            LeanTween.moveLocalY(body, 0f, expandAnimationTime).setEaseOutQuint();
            // slide scrollview up
            var deckMenuScrollViewCurrentYPosition = deckMenuScrollView.transform.position.y;
            var deckMenuScrollViewTargetYPosition = deckMenuScrollView.transform.position.y - 3.5f;
            LeanTween.value(deckMenuScrollView, deckMenuScrollViewCurrentYPosition, deckMenuScrollViewTargetYPosition, expandAnimationTime)
            .setOnUpdate((float value) =>
            {
                deckMenuScrollView.transform.position = new Vector3(deckMenuScrollView.transform.position.x, value);
            }).setEaseOutQuint();
            expanded = true;
        }
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
            var deckMenuScrollViewTargetYPosition = deckMenuScrollView.transform.position.y + 3.5f;
            LeanTween.value(deckMenuScrollView, deckMenuScrollViewCurrentYPosition, deckMenuScrollViewTargetYPosition, expandAnimationTime)
            .setOnUpdate((float value) =>
            {
                deckMenuScrollView.transform.position = new Vector3(deckMenuScrollView.transform.position.x, value);
            }).setEaseOutQuint();
            // reset rank & craft UI
            selectedRankIndex = 0;
            UpdateRankUI();
            toCraftCount = 0;
            UpdateCraftCountUI();
            expanded = false;
        }
    }

    // RANKS
    public void PreviousRank()
    {
        if (selectedRankIndex > 0)
        {
            selectedRankIndex--;
        }
        UpdateRankUI();
    }

    public void NextRank()
    {
        if (selectedRankIndex < rankToCraftCosts.Length - 1) // TODO: set upper limit based on credits
        {
            selectedRankIndex++;
        }
        UpdateRankUI();
    }

    private void UpdateRankUI()
    {
        cardImage.color = rankColors[selectedRankIndex];

        shine.SetActive(selectedRankIndex > 0);
        particleParent.SetActive(selectedRankIndex > 0);

        // update UI arrows
        previousRankButton.image.enabled = (selectedRankIndex > 0);
        previousRankButton.interactable = (selectedRankIndex > 0);
        nextRankButton.image.enabled = (selectedRankIndex < rankToCraftCosts.Length - 1);
        nextRankButton.interactable = (selectedRankIndex < rankToCraftCosts.Length - 1);

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
        craftCount1.text = (toCraftCount > 0) ? (owned + toCraftCount).ToString() : "";
        int remainingCredits = (craftingCredits - rankToCraftCosts[selectedRankIndex] * toCraftCount);
        craftCount2.text = (toCraftCount > 0) ? remainingCredits.ToString() : "";

        // buttons
        craftMinusButton.interactable = (toCraftCount > 0);
        craftConfirmButton.interactable = (toCraftCount > 0 && remainingCredits > 0);
        craftPlusButton.interactable = ((remainingCredits - rankToCraftCosts[selectedRankIndex]) > 0);
    }
}
