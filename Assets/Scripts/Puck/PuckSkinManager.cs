using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuckSkinManager : MonoBehaviour
{
    public static PuckSkinManager Instance;
    private LogicScript logic;
    private UIManagerScript UI;

    // sprites
    [SerializeField] private Sprite puckFlower;
    [SerializeField] private Sprite puckMissing;

    [SerializeField] private Sprite puckBlue;
    [SerializeField] private Sprite puckGreen;
    [SerializeField] private Sprite puckGrey;
    [SerializeField] private Sprite puckOrange;
    [SerializeField] private Sprite puckPink;
    [SerializeField] private Sprite puckPurple;
    [SerializeField] private Sprite puckRed;
    [SerializeField] private Sprite puckYellow;

    [SerializeField] private Sprite puckRainbow;
    [SerializeField] private Sprite puckCanada;
    [SerializeField] private Sprite puckDonut;
    [SerializeField] private Sprite puckCaptain;
    [SerializeField] private Sprite puckNuke;
    [SerializeField] private Sprite puckWreath;
    [SerializeField] private Sprite puckSky;
    [SerializeField] private Sprite puckDragon;
    [SerializeField] private Sprite puckNinja;
    [SerializeField] private Sprite puckEgg;
    [SerializeField] private Sprite puckMonster;
    [SerializeField] private Sprite puckEye;
    [SerializeField] private Sprite puckCamo;
    [SerializeField] private Sprite puckYingYang;
    [SerializeField] private Sprite puckCow;
    [SerializeField] private Sprite puckCraft;
    [SerializeField] private Sprite puckPlanet;
    [SerializeField] private Sprite puckLove;
    [SerializeField] private Sprite puckAura;
    [SerializeField] private Sprite puckCheese;
    [SerializeField] private Sprite puckScotia;
    [SerializeField] private Sprite puckPoker;
    [SerializeField] private Sprite puckPumpkin;
    [SerializeField] private Sprite puckWeb;
    [SerializeField] private Sprite puckCoin;
    [SerializeField] private Sprite puckMagic;
    [SerializeField] private Sprite puckStar;
    [SerializeField] private Sprite puckSnake;

    [SerializeField] private Sprite puckBlueAlt;
    [SerializeField] private Sprite puckGreenAlt;
    [SerializeField] private Sprite puckGreyAlt;
    [SerializeField] private Sprite puckOrangeAlt;
    [SerializeField] private Sprite puckPinkAlt;
    [SerializeField] private Sprite puckPurpleAlt;
    [SerializeField] private Sprite puckRedAlt;
    [SerializeField] private Sprite puckYellowAlt;

    [SerializeField] private Sprite puckRainbowAlt;
    [SerializeField] private Sprite puckCanadaAlt;
    [SerializeField] private Sprite puckDonutAlt;
    [SerializeField] private Sprite puckCaptainAlt;
    [SerializeField] private Sprite puckNukeAlt;
    [SerializeField] private Sprite puckWreathAlt;
    [SerializeField] private Sprite puckSkyAlt;
    [SerializeField] private Sprite puckDragonAlt;
    [SerializeField] private Sprite puckNinjaAlt;
    [SerializeField] private Sprite puckEggAlt;
    [SerializeField] private Sprite puckMonsterAlt;
    [SerializeField] private Sprite puckEyeAlt;
    [SerializeField] private Sprite puckCamoAlt;
    [SerializeField] private Sprite puckYingYangAlt;
    [SerializeField] private Sprite puckCowAlt;
    [SerializeField] private Sprite puckCraftAlt;
    [SerializeField] private Sprite puckPlanetAlt;
    [SerializeField] private Sprite puckLoveAlt;
    [SerializeField] private Sprite puckAuraAlt;
    [SerializeField] private Sprite puckCheeseAlt;
    [SerializeField] private Sprite puckScotiaAlt;
    [SerializeField] private Sprite puckPokerAlt;
    [SerializeField] private Sprite puckPumpkinAlt;
    [SerializeField] private Sprite puckWebAlt;
    [SerializeField] private Sprite puckCoinAlt;
    [SerializeField] private Sprite puckMagicAlt;
    [SerializeField] private Sprite puckStarAlt;
    [SerializeField] private Sprite puckSnakeAlt;

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
        logic = LogicScript.Instance;
        UI = UIManagerScript.Instance;
    }

    public Sprite ColorIDtoPuckSprite(int id)
    {
        Sprite[] puckSprites = { puckFlower, puckBlue, puckGreen, puckGrey, puckOrange, puckPink, puckPurple, puckRed, puckYellow, puckRainbow, puckCanada, puckDonut, puckCaptain, puckNuke, puckWreath, puckSky, puckDragon, puckNinja, puckEgg, puckMonster, puckEye, puckCamo, puckYingYang, puckCow, puckCraft, puckPlanet, puckLove, puckAura, puckCheese, puckScotia, puckPoker, puckPumpkin, puckWeb, puckCoin, puckMagic, puckStar, puckSnake };
        Sprite[] puckAltSprites = { puckMissing, puckBlueAlt, puckGreenAlt, puckGreyAlt, puckOrangeAlt, puckPinkAlt, puckPurpleAlt, puckRedAlt, puckYellowAlt, puckRainbowAlt, puckCanadaAlt, puckDonutAlt, puckCaptainAlt, puckNukeAlt, puckWreathAlt, puckSkyAlt, puckDragonAlt, puckNinjaAlt, puckEggAlt, puckMonsterAlt, puckEyeAlt, puckCamoAlt, puckYingYangAlt, puckCowAlt, puckCraftAlt, puckPlanetAlt, puckLoveAlt, puckAuraAlt, puckCheeseAlt, puckScotiaAlt, puckPokerAlt, puckPumpkinAlt, puckWebAlt, puckCoinAlt, puckMagicAlt, puckStarAlt, puckSnakeAlt };

        // if out of range, return missing
        if ((id >= puckSprites.Length) || (id <= puckAltSprites.Length * -1))
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

    public Color ColorIDtoColor(int id)
    {
        Color[] puckColors = { new Color(0f, 0f, 0f), new Color(0f, 0.7490196f, 0.9529412f), new Color(0.4862745f, 0.7725491f, 0.4627451f), new Color(0.5019608f, 0.5019608f, 0.5019608f), new Color(0.9647059f, 0.5568628f, 0.3372549f), new Color(0.9411765f, 0.4313726f, 0.6666667f), new Color(0.5215687f, 0.3764706f, 0.6588235f), new Color(0.9490197f, 0.4235294f, 0.3098039f), new Color(1f, 0.9607844f, 0.4078432f) };
        Color[] puckAltColors = { new Color(0f, 0f, 0f), new Color(0f, 0.7490196f, 0.9529412f), new Color(0.4862745f, 0.7725491f, 0.4627451f), new Color(0.5019608f, 0.5019608f, 0.5019608f), new Color(0.9647059f, 0.5568628f, 0.3372549f), new Color(0.9411765f, 0.4313726f, 0.6666667f), new Color(0.5215687f, 0.3764706f, 0.6588235f), new Color(0.9490197f, 0.4235294f, 0.3098039f), new Color(1f, 0.9607844f, 0.4078432f) };

        // if out of range, return grey
        if ((id >= puckColors.Length) || (id <= puckAltColors.Length * -1))
        {
            return new Color(0.5f, 0.5f, 0.5f);
        }

        // if postitive, return regular, else return alt
        try
        {
            if (id >= 0)
            {
                return (puckColors[id]);
            }
            else
            {
                return (puckAltColors[id * -1]);
            }
        }
        catch (System.IndexOutOfRangeException)
        {
            return new Color(0.5f, 0.5f, 0.5f);
        }
    }

    public void SelectPlayerPuckSprite(int id)
    {
        logic.player.puckSpriteID = id;
        logic.player.puckSprite = ColorIDtoPuckSprite(id);
        UI.SetPlayerPuckIcon(logic.player.puckSprite);
        PlayerPrefs.SetInt("puck", id);
        RandomizeCPUPuckSprite();
        if (logic.activeCompetitor != null)
        {
            PlayerPrefs.SetInt("ShowNewSkinAlert", 0);
        }
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
            int rand = Random.Range(1, 9);
            logic.opponent.puckSprite = ColorIDtoPuckSprite(rand);
            logic.opponent.puckSpriteID = rand;
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
    [SerializeField] private Transform easterEggBox;
    [SerializeField] private Transform antiEasterEggBox;

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
