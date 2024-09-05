using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuckSkinManager : MonoBehaviour
{
    public static PuckSkinManager Instance;
    private LogicScript logic;
    private UIManagerScript UI;

    // sprites
    [SerializeField] Sprite puckFlower;
    [SerializeField] Sprite puckMissing;

    [SerializeField] Sprite puckBlue;
    [SerializeField] Sprite puckGreen;
    [SerializeField] Sprite puckGrey;
    [SerializeField] Sprite puckOrange;
    [SerializeField] Sprite puckPink;
    [SerializeField] Sprite puckPurple;
    [SerializeField] Sprite puckRed;
    [SerializeField] Sprite puckYellow;

    [SerializeField] Sprite puckRainbow;
    [SerializeField] Sprite puckCanada;
    [SerializeField] Sprite puckDonut;
    [SerializeField] Sprite puckCaptain;
    [SerializeField] Sprite puckNuke;
    [SerializeField] Sprite puckWreath;
    [SerializeField] Sprite puckSky;
    [SerializeField] Sprite puckDragon;
    [SerializeField] Sprite puckNinja;
    [SerializeField] Sprite puckEgg;
    [SerializeField] Sprite puckMonster;
    [SerializeField] Sprite puckEye;
    [SerializeField] Sprite puckCamo;
    [SerializeField] Sprite puckYingYang;
    [SerializeField] Sprite puckCow;
    [SerializeField] Sprite puckCraft;
    [SerializeField] Sprite puckPlanet;
    [SerializeField] Sprite puckLove;
    [SerializeField] Sprite puckAura;
    [SerializeField] Sprite puckCheese;
    [SerializeField] Sprite puckScotia;

    [SerializeField] Sprite puckBlueAlt;
    [SerializeField] Sprite puckGreenAlt;
    [SerializeField] Sprite puckGreyAlt;
    [SerializeField] Sprite puckOrangeAlt;
    [SerializeField] Sprite puckPinkAlt;
    [SerializeField] Sprite puckPurpleAlt;
    [SerializeField] Sprite puckRedAlt;
    [SerializeField] Sprite puckYellowAlt;

    [SerializeField] Sprite puckRainbowAlt;
    [SerializeField] Sprite puckCanadaAlt;
    [SerializeField] Sprite puckDonutAlt;
    [SerializeField] Sprite puckCaptainAlt;
    [SerializeField] Sprite puckNukeAlt;
    [SerializeField] Sprite puckWreathAlt;
    [SerializeField] Sprite puckSkyAlt;
    [SerializeField] Sprite puckDragonAlt;
    [SerializeField] Sprite puckNinjaAlt;
    [SerializeField] Sprite puckEggAlt;
    [SerializeField] Sprite puckMonsterAlt;
    [SerializeField] Sprite puckEyeAlt;
    [SerializeField] Sprite puckCamoAlt;
    [SerializeField] Sprite puckYingYangAlt;
    [SerializeField] Sprite puckCowAlt;
    [SerializeField] Sprite puckCraftAlt;
    [SerializeField] Sprite puckPlanetAlt;
    [SerializeField] Sprite puckLoveAlt;
    [SerializeField] Sprite puckAuraAlt;
    [SerializeField] Sprite puckCheeseAlt;
    [SerializeField] Sprite puckScotiaAlt;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private List<int> unlockedPuckIDs = new() { -8, -7, -6, -5, -4, -3, -2, -1, 1, 2, 3, 4, 5, 6, 7, 8 };

    private void OnEnable()
    {
        logic = GameObject.FindGameObjectWithTag("logic").GetComponent<LogicScript>();
        UI = GameObject.FindGameObjectWithTag("ui").GetComponent<UIManagerScript>();
    }

    public Sprite ColorIDtoPuckSprite(int id)
    {
        Sprite[] puckSprites = { puckFlower, puckBlue, puckGreen, puckGrey, puckOrange, puckPink, puckPurple, puckRed, puckYellow, puckRainbow, puckCanada, puckDonut, puckCaptain, puckNuke, puckWreath, puckSky, puckDragon, puckNinja, puckEgg, puckMonster, puckEye, puckCamo, puckYingYang, puckCow, puckCraft, puckPlanet, puckLove, puckAura, puckCheese, puckScotia };
        Sprite[] puckAltSprites = { null, puckBlueAlt, puckGreenAlt, puckGreyAlt, puckOrangeAlt, puckPinkAlt, puckPurpleAlt, puckRedAlt, puckYellowAlt, puckRainbowAlt, puckCanadaAlt, puckDonutAlt, puckCaptainAlt, puckNukeAlt, puckWreathAlt, puckSkyAlt, puckDragonAlt, puckNinjaAlt, puckEggAlt, puckMonsterAlt, puckEyeAlt, puckCamoAlt, puckYingYangAlt, puckCowAlt, puckCraftAlt, puckPlanetAlt, puckLoveAlt, puckAuraAlt, puckCheeseAlt, puckScotiaAlt };

        // if out of range, return missing
        if ((id >= puckSprites.Length) || (id <= puckSprites.Length * -1))
        {
            return puckMissing;
        }

        // if postitive, return regular, else return alt
        try
        {
            if (id >= 0)
            {
                return (puckSprites[id]);
            }
            else
            {
                return (puckAltSprites[id * -1]);
            }
        }
        catch (System.IndexOutOfRangeException)
        {
            return puckMissing;
        }
    }

    public void SelectPlayerPuckSprite(int id)
    {
        logic.player.puckSpriteID = id;
        logic.player.puckSprite = ColorIDtoPuckSprite(id);
        UI.SetPlayerPuckIcon(logic.player.puckSprite);
        PlayerPrefs.SetInt("puck", id);
        RandomizeCPUPuckSprite();
        PlayerPrefs.SetInt("ShowNewSkinAlert", 0);
    }

    public void UnlockPuckID(int id)
    {
        // if not already unlocked, add to list
        if (!unlockedPuckIDs.Contains(id))
        {
            unlockedPuckIDs.Add(id);
            if (logic.playedAGame)
            {
                PlayerPrefs.SetInt("ShowNewSkinAlert", 1);
            }
        }
    }

    public void RandomizePlayerPuckSprite()
    {
        var prev = logic.player.puckSprite;
        int rng;
        do
        {
            rng = Random.Range(0, unlockedPuckIDs.Count);

            logic.player.puckSpriteID = unlockedPuckIDs[rng];
            logic.player.puckSprite = ColorIDtoPuckSprite(unlockedPuckIDs[rng]);
        } while (prev == logic.player.puckSprite);
        UI.SetPlayerPuckIcon(logic.player.puckSprite);
        PlayerPrefs.SetInt("puck", rng);
        RandomizeCPUPuckSprite();
    }

    public void RandomizeCPUPuckSprite()
    {
        do
        {
            logic.opponent.puckSprite = ColorIDtoPuckSprite(Random.Range(1, 9));
        } while (logic.opponent.puckSprite == logic.player.puckSprite);
        UI.SetOpponentPuckIcon(logic.opponent.puckSprite);
    }

    // called by puck select menu pucks
    public void SwapToAltSkin(GameObject gameObject)
    {
        if (gameObject.CompareTag("alt"))
        {
            SelectPlayerPuckSprite(logic.player.puckSpriteID * -1);
            gameObject.tag = "Untagged";

        }
        else { gameObject.tag = "alt"; }

        gameObject.GetComponent<Image>().sprite = ColorIDtoPuckSprite(logic.player.puckSpriteID * -1);
    }

    // helper variables for easter egg button
    int easterEggCounter = 0;
    [SerializeField] Transform easterEggBox;
    [SerializeField] Transform antiEasterEggBox;

    // called by easter egg object
    public void EasterEgg()
    {
        easterEggCounter++;
        easterEggBox.position += transform.right * 1.4f;
        antiEasterEggBox.position += transform.right * 1.4f;
        if (easterEggCounter == 11)
        {
            SelectPlayerPuckSprite(0);
            ResetEasterEgg();
        }
    }

    // called by anti easter egg object
    public void ResetEasterEgg()
    {
        easterEggCounter = 0;
        easterEggBox.position = new Vector3(-7.10f, 14.40f, 0f);
        antiEasterEggBox.position = new Vector3(2.50f, 14.40f, 0f);
    }
}
