using UnityEngine;

public class PackOpenPrefabScript : MonoBehaviour
{
    [SerializeField] private GameObject PowerupPopupPrefab;

    [SerializeField] private GameObject cardParent;

    [SerializeField] private GameObject puckImageObject;
    [SerializeField] private ParticleSystem rarityParticleSystem;

    int targetClicks;
    int clicks;

    int cardIndex;
    int rank;
    bool holo;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && puckImageObject.transform.localScale == Vector3.one && clicks < targetClicks)
        {
            if (ClickIsNotOnPack() && !Application.isEditor) return;

            LeanTween.cancel(puckImageObject);

            ParticleSystem.MainModule main = rarityParticleSystem.main;
            Color rarityColor = GetRarityColor();
            main.startColor = rarityColor;

            puckImageObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            LeanTween.scale(puckImageObject, Vector3.one, 0.5f).setEaseOutElastic();
            rarityParticleSystem.Play();
            clicks++;
        }
        else if (Input.GetMouseButtonDown(0) && puckImageObject.transform.localScale == Vector3.one && clicks == targetClicks)
        {
            if (ClickIsNotOnPack() && !Application.isEditor) return;

            LeanTween.cancel(puckImageObject);

            ParticleSystem.MainModule main = rarityParticleSystem.main;
            Color rarityColor = GetRarityColor();
            main.startColor = rarityColor;

            puckImageObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            LeanTween.scale(puckImageObject, Vector3.one, 0.5f).setEaseOutElastic();
            rarityParticleSystem.Play();

            LeanTween.scale(puckImageObject, Vector3.zero, 0.5f).setEaseInOutQuint().setDelay(0.45f).setOnComplete(CreatePowerupPopupPrefab);

            clicks++;
        }
    }

    private void CreatePowerupPopupPrefab()
    {
        GameObject powerupPopupObject = Instantiate(PowerupPopupPrefab, cardParent.transform);
        PowerupPopupPrefabScript powerupPopupScript = powerupPopupObject.GetComponent<PowerupPopupPrefabScript>();
        powerupPopupScript.InitializePowerupPopup(cardIndex, rank, holo);
        powerupPopupScript.Animate();
        PackManager.Instance.ShowBottomText();
    }

    private int[] rarityBaseDupeCreditReward = new int[] { 1, 2, 5, 10, 25 };
    private int[] rankMultDupeCreditReward = new int[] { 1, 25, 50, 100, 1000 };
    public void InitializePackOpen(int cardIndex, int rank, bool holo)
    {
        targetClicks = PowerupCardData.GetCardRarity(cardIndex);
        this.cardIndex = cardIndex;
        this.rank = rank;
        this.holo = holo;
        if (PowerupCardData.AddCardToCollection(cardIndex, rank, holo))
        {
            int creditReward = rarityBaseDupeCreditReward[PowerupCardData.GetCardRarity(cardIndex)] * rankMultDupeCreditReward[rank] * (holo ? 10 : 1);
            PackManager.Instance.RewardCraftingCredits(creditReward);
        }
    }

    private Color GetRarityColor()
    {
        switch (clicks)
        {
            case 1:
                return(new Color(0.4862745f, 0.7725491f, 0.4627451f));
            case 2:
                return (new Color(0.0000000f, 0.7490196f, 0.9529412f));
            case 3:
                return (new Color(0.5215687f, 0.3764706f, 0.6588235f));
            case 4:
                return (new Color(1.0000000f, 0.9607844f, 0.4078432f));
            default:
                return (new Color(1f, 1f, 1f, 1f));
        }
    }

    private bool ClickIsNotOnPack()
    {
        Vector3 mousePosition = Input.mousePosition; // Get mouse position in screen space
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition); // Convert to world space

        worldPosition.z = gameObject.transform.position.z; // Ensure the z-coordinate is set as the same as the pack to compare

        float distance = Vector3.Distance(worldPosition, gameObject.transform.position); // get distance

        // If the distance is less than or equal to 1 unit, the click is on or near the pack
        if (distance > gameObject.transform.localScale.x * 1.6f)
        {
            return true; // Click is not on pack
        }
        else
        {
            return false;
        }
    }
}
