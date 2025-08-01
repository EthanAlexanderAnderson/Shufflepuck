using UnityEngine;

public class PackOpenPrefabScript : MonoBehaviour
{
    [SerializeField] private GameObject PowerupPopupPrefab;

    [SerializeField] private GameObject cardParent;

    [SerializeField] private GameObject puckImageObject;
    [SerializeField] private ParticleSystem rarityParticleSystem;

    int targetClicks;
    int clicks;
    int storedClicks;

    int cardIndex;
    int rank;
    bool holo;

    private void Update()
    {
        if ((AnyTouchOnPack() || storedClicks > clicks) && puckImageObject.transform.localScale == Vector3.one && clicks < targetClicks)
        {
            LeanTween.cancel(puckImageObject);

            ParticleSystem.MainModule main = rarityParticleSystem.main;
            Color rarityColor = GetRarityColor();
            main.startColor = rarityColor;

            puckImageObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            LeanTween.scale(puckImageObject, Vector3.one, 0.5f).setEaseOutElastic();
            rarityParticleSystem.Play();
            SoundManagerScript.Instance.PlayClickSFXPitch(3, clicks/10f);
            clicks++;
        }
        else if ((AnyTouchOnPack() || storedClicks > clicks) && puckImageObject.transform.localScale == Vector3.one && clicks == targetClicks)
        {
            LeanTween.cancel(puckImageObject);

            ParticleSystem.MainModule main = rarityParticleSystem.main;
            Color rarityColor = GetRarityColor();
            main.startColor = rarityColor;

            puckImageObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            LeanTween.scale(puckImageObject, Vector3.one, 0.5f).setEaseOutElastic();
            rarityParticleSystem.Play();

            LeanTween.scale(puckImageObject, Vector3.zero, 0.5f).setEaseInOutQuint().setDelay(0.45f).setOnComplete(CreatePowerupPopupPrefab);

            SoundManagerScript.Instance.PlayClickSFXPitch(3, clicks/10f);
            clicks++;
        }

#if UNITY_EDITOR
        // In the editor, use mouse input fallback
        if (Input.GetMouseButtonDown(0))
        {
            storedClicks++;
        }
#else
        if (AnyTouchOnPack())
        {
            storedClicks++;
        }
#endif
    }

    private void CreatePowerupPopupPrefab()
    {
        GameObject powerupPopupObject = Instantiate(PowerupPopupPrefab, cardParent.transform);
        PowerupPopupPrefabScript powerupPopupScript = powerupPopupObject.GetComponent<PowerupPopupPrefabScript>();
        powerupPopupScript.InitializePowerupPopup(cardIndex, rank, holo, PowerupCardData.GetCardOwnedCount(cardIndex, rank, holo) <= 1);
        powerupPopupScript.Animate();
        SoundManagerScript.Instance.PlayPowerupPopup(true);
        if (holo) SoundManagerScript.Instance.PlayPowerupPopup(true, 1);
        if (rank > 0) SoundManagerScript.Instance.PlayPowerupPopup(true, 2);
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
                return new Color(0.4862745f, 0.7725491f, 0.4627451f);
            case 2:
                return new Color(0.0000000f, 0.7490196f, 0.9529412f);
            case 3:
                return new Color(0.5215687f, 0.3764706f, 0.6588235f);
            case 4:
                return new Color(1.0000000f, 0.9607844f, 0.4078432f);
            default:
                //return new Color(1f, 1f, 1f, 1f);
                return UIManagerScript.Instance.GetDarkMode() ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 1f);
        }
    }

    private bool AnyTouchOnPack()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                touchPos.z = gameObject.transform.position.z;

                float distance = Vector3.Distance(touchPos, gameObject.transform.position);
                if (distance <= gameObject.transform.localScale.x * 1.6f)
                {
                    return true; // At least one touch is on the pack
                }
            }
        }
        return false; // No touches on pack
    }
}
