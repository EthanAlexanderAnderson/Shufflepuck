using System;
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
    [SerializeField] private Sprite puckHexagon;
    [SerializeField] private Sprite puckShuriken;
    [SerializeField] private Sprite puckLifesaver;
    [SerializeField] private Sprite puckAtom;
    [SerializeField] private Sprite puckMatrix;
    [SerializeField] private Sprite puckLucky;
    [SerializeField] private Sprite puckSkull;
    [SerializeField] private Sprite puckButterfly;

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
    [SerializeField] private Sprite puckHexagonAlt;
    [SerializeField] private Sprite puckShurikenAlt;
    [SerializeField] private Sprite puckLifesaverAlt;
    [SerializeField] private Sprite puckAtomAlt;
    [SerializeField] private Sprite puckMatrixAlt;
    [SerializeField] private Sprite puckLuckyAlt;
    [SerializeField] private Sprite puckSkullAlt;
    [SerializeField] private Sprite puckButterflyAlt;

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

    private void Start()
    {
        SelectPlayerPuckSprite(PlayerPrefs.GetInt("puck") == 0 && PlayerPrefs.GetInt("hardHighscore") <= 5 ? 1 : PlayerPrefs.GetInt("puck"));
    }

    public Sprite ColorIDtoPuckSprite(int id)
    {
        Sprite[] puckSprites = { puckFlower, puckBlue, puckGreen, puckGrey, puckOrange, puckPink, puckPurple, puckRed, puckYellow, puckRainbow, puckCanada, puckDonut, puckCaptain, puckNuke, puckWreath, puckSky, puckDragon, puckNinja, puckEgg, puckMonster, puckEye, puckCamo, puckYingYang, puckCow, puckCraft, puckPlanet, puckLove, puckAura, puckCheese, puckScotia, puckPoker, puckPumpkin, puckWeb, puckCoin, puckMagic, puckStar, puckSnake, puckHexagon, puckShuriken, puckLifesaver, puckAtom, puckMatrix, puckLucky, puckSkull, puckButterfly };
        Sprite[] puckAltSprites = { puckMissing, puckBlueAlt, puckGreenAlt, puckGreyAlt, puckOrangeAlt, puckPinkAlt, puckPurpleAlt, puckRedAlt, puckYellowAlt, puckRainbowAlt, puckCanadaAlt, puckDonutAlt, puckCaptainAlt, puckNukeAlt, puckWreathAlt, puckSkyAlt, puckDragonAlt, puckNinjaAlt, puckEggAlt, puckMonsterAlt, puckEyeAlt, puckCamoAlt, puckYingYangAlt, puckCowAlt, puckCraftAlt, puckPlanetAlt, puckLoveAlt, puckAuraAlt, puckCheeseAlt, puckScotiaAlt, puckPokerAlt, puckPumpkinAlt, puckWebAlt, puckCoinAlt, puckMagicAlt, puckStarAlt, puckSnakeAlt, puckHexagonAlt, puckShurikenAlt, puckLifesaverAlt, puckAtomAlt, puckMatrixAlt, puckLuckyAlt, puckSkullAlt, puckButterflyAlt };

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

    public Color[] ColorIDtoColor(int id)
    {
        // if out of range, return grey
        if ((id >= puckColors.Length) || (id <= puckAltColors.Length * -1))
        {
            return new Color[] { new Color(0.5f, 0.5f, 0.5f) };
        }

        // if postitive, return regular, else return alt
        try
        {
            if (id >= 0)
            {
                return puckColors[id];
            }
            else
            {
                return puckAltColors[id * -1];
            }
        }
        catch (System.IndexOutOfRangeException)
        {
            return new Color[] { new Color(0.5f, 0.5f, 0.5f) };
        }
    }

    public void SelectPlayerPuckSprite(int id)
    {
        logic.player.puckSpriteID = id;
        logic.player.puckSprite = ColorIDtoPuckSprite(id);
        UI.SetPlayerPuckIcon(logic.player.puckSprite, Math.Abs(logic.player.puckSpriteID) == 40);
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
            PlayerPrefs.SetInt("ShowNewSkinAlert", 1);
        }
    }

    public void RandomizePlayerPuckSprite()
    {
        var prev = logic.player.puckSprite;
        int rng;
        do
        {
            rng = UnityEngine.Random.Range(0, unlockedPuckIDs.Count);

            logic.player.puckSpriteID = unlockedPuckIDs[rng];
            logic.player.puckSprite = ColorIDtoPuckSprite(unlockedPuckIDs[rng]);
        } while (prev == logic.player.puckSprite);
        UI.SetPlayerPuckIcon(logic.player.puckSprite, Math.Abs(logic.player.puckSpriteID) == 40);
        PlayerPrefs.SetInt("puck", rng);
        RandomizeCPUPuckSprite();
    }

    public void RandomizeCPUPuckSprite()
    {
        do
        {
            int rand = UnityEngine.Random.Range(1, 9);
            logic.opponent.puckSprite = ColorIDtoPuckSprite(rand);
            logic.opponent.puckSpriteID = rand;
        } while (logic.opponent.puckSprite == logic.player.puckSprite);
        UI.SetOpponentPuckIcon(logic.opponent.puckSprite, Math.Abs(logic.opponent.puckSpriteID) == 40);
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

    // yep. y'know im putting the big variables at the bottom. idgaf
    // Maximum colors is 8 btw
    Color[][] puckColors = {
        new Color[] { new Color(0.5215687f, 0.3764706f, 0.6588235f), new Color(0.9411765f, 0.4313726f, 0.6666667f), new Color(0.9647059f, 0.5568628f, 0.3372549f), new Color(0.4862745f, 0.7725491f, 0.4627451f), new Color(0f, 0.7490196f, 0.9529412f), new Color(1f, 0.9607844f, 0.4078432f), new Color(0.9490197f, 0.4235294f, 0.3098039f) }, // flower
        new Color[] { new Color(0.0000000f, 0.7490196f, 0.9529412f) }, // blue
        new Color[] { new Color(0.4862745f, 0.7725491f, 0.4627451f) }, // green
        new Color[] { new Color(0.5019608f, 0.5019608f, 0.5019608f) }, // grey
        new Color[] { new Color(0.9647059f, 0.5568628f, 0.3372549f) }, // orange
        new Color[] { new Color(0.9411765f, 0.4313726f, 0.6666667f) }, // pink
        new Color[] { new Color(0.5215687f, 0.3764706f, 0.6588235f) }, // purple
        new Color[] { new Color(0.9490197f, 0.4235294f, 0.3098039f) }, // red
        new Color[] { new Color(1.0000000f, 0.9607844f, 0.4078432f) },  // yellow
        new Color[] { new Color(0.5215687f, 0.3764706f, 0.6588235f), new Color(0.9411765f, 0.4313726f, 0.6666667f), new Color(0.9647059f, 0.5568628f, 0.3372549f), new Color(0.4862745f, 0.7725491f, 0.4627451f), new Color(0f, 0.7490196f, 0.9529412f), new Color(1f, 0.9607844f, 0.4078432f), new Color(0.9490197f, 0.4235294f, 0.3098039f) }, // rainbow
        new Color[] { new Color(0.9215686f, 0.1764706f, 0.2156863f) }, // canada
        new Color[] { new Color(0.9490196f, 0.4274510f, 0.4901961f) }, // donut
        new Color[] { new Color(0.8901961f, 0.1607843f, 0.1647059f) }, // captain
        new Color[] { new Color(0.2666667f, 0.9333333f, 0.0000000f) }, // nuke
        new Color[] { new Color(0.4862745f, 0.7725490f, 0.4627451f) }, // wreath
        new Color[] { new Color(0.6000000f, 0.9137255f, 1.0000000f) }, // sky
        new Color[] { new Color(1.0000000f, 0.5372549f, 0.0078431f) }, // dragon
        new Color[] { new Color(0.6549020f, 0.0000000f, 0.0000000f) }, // ninja
        new Color[] { new Color(0.5019608f, 0.5019608f, 0.5019608f) }, // egg
        new Color[] { new Color(0.9372549f, 0.2509804f, 0.2078431f) }, // monster
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // eye
        new Color[] { new Color(0.6784314f, 0.6274510f, 0.4196078f), new Color(0.3647059f, 0.2274510f, 0.0705882f),  new Color(0.2196078f, 0.2941176f, 0.0784314f),  new Color(0.1725490f, 0.1215686f, 0.0588235f) }, // camo
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f), new Color(0.0000000f, 0.0000000f, 0.0000000f) }, // ying yang
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f), new Color(0.0000000f, 0.0000000f, 0.0000000f) }, // cow
        new Color[] { new Color(0.5725490f, 0.1921569f, 0.2274509f), new Color(0.5686275f, 0.8117647f, 0.3450980f) }, // craft
        new Color[] { new Color(0.9294118f, 0.8235294f, 0.4196078f) }, // planet
        new Color[] { new Color(0.9137255f, 0.6509804f, 0.7450980f) }, // love
        new Color[] { new Color(0.6549020f, 0.3921569f, 0.6627451f), new Color(0.3764706f, 0.3607843f, 0.6588235f), new Color(0.2666667f, 0.5490196f, 0.8039216f), new Color(0.1019608f, 0.7411765f, 0.6980392f) }, // aura
        new Color[] { new Color(0.9882353f, 0.8039216f, 0.5294118f) }, // cheese
        new Color[] { new Color(0.2078431f, 0.2509804f, 0.4196078f) }, // scotia
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // poker
        new Color[] { new Color(1.0000000f, 0.5058824f, 0.1813725f) }, // pumpkin
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // web
        new Color[] { new Color(0.8000000f, 0.5647059f, 0.0000000f) }, // coin
        new Color[] { new Color(1.0000000f, 0.9607843f, 0.4078431f), new Color(0.0000000f, 0.7490196f, 0.9529412f), new Color(0.9490196f, 0.4235294f, 0.3098039f), new Color(0.2352941f, 0.7215686f, 0.4705882f) }, // magic
        new Color[] { new Color(0.0000000f, 0.7490196f, 0.9529412f) }, // star
        new Color[] { new Color(1.0000000f, 0.1372549f, 0.2313725f) }, // snake
        new Color[] { new Color(1.0000000f, 0.9607843f, 0.4196078f), new Color(0.4862745f, 0.7607843f, 0.4754902f), new Color(0.1215686f, 0.7333333f, 0.7058824f), new Color(0.9411765f, 0.4313725f, 0.6666667f) }, // hexagon
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // shuriken
        new Color[] { new Color(0.9490196f, 0.4235294f, 0.3098039f), new Color(1.0000000f, 1.0000000f, 1.0000000f), new Color(1.0000000f, 1.0000000f, 1.0000000f), new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // lifesaver
        new Color[] { new Color(0.0117647f, 0.4039216f, 0.5215686f) }, // atom
        new Color[] { new Color(0.0000000f, 1.0000000f, 0.0000000f) }, // matrix
        new Color[] { new Color(1.0000000f, 0.9607844f, 0.4078432f), new Color(0.4862745f, 0.7725491f, 0.4627451f) }, // lucky
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // skull
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f), new Color(0.0000000f, 0.7490196f, 0.9529412f), new Color(0.9411765f, 0.4313726f, 0.6666667f) }, // butterfly
    };

    Color[][] puckAltColors = {
        new Color[] { new Color(1f, 1f, 1f) }, // negative zero is unreachable
        new Color[] { new Color(0.0000000f, 0.7490196f, 0.9529412f) }, // blue alt
        new Color[] { new Color(0.4862745f, 0.7725491f, 0.4627451f) }, // green alt
        new Color[] { new Color(0.5019608f, 0.5019608f, 0.5019608f) }, // grey alt
        new Color[] { new Color(0.9647059f, 0.5568628f, 0.3372549f) }, // orange alt
        new Color[] { new Color(0.9411765f, 0.4313726f, 0.6666667f) }, // pink alt
        new Color[] { new Color(0.5215687f, 0.3764706f, 0.6588235f) }, // purple alt
        new Color[] { new Color(0.9490197f, 0.4235294f, 0.3098039f) }, // red alt
        new Color[] { new Color(1.0000000f, 0.9607844f, 0.4078432f) },  // yellow alt
        new Color[] { new Color(0.0000000f, 1.0000000f, 0.7058824f), new Color(1.0000000f, 0.8313726f, 0.3686275f), new Color(0.9411765f, 0.6000000f, 1.0000000f) }, // rainbow alt
        new Color[] { new Color(0.9215686f, 0.1764706f, 0.2156863f) }, // canada alt
        new Color[] { new Color(0.4274510f, 0.7058824f, 0.9490196f) }, // donut alt
        new Color[] { new Color(0.0313726f, 0.3176471f, 0.5333334f) }, // captain alt
        new Color[] { new Color(0.2666667f, 0.9333333f, 0.0000000f) }, // nuke alt
        new Color[] { new Color(0.9490196f, 0.4235294f, 0.3098039f) }, // wreath alt
        new Color[] { new Color(0.3372549f, 0.4549020f, 0.7254902f) }, // sky alt
        new Color[] { new Color(0.9921569f, 0.8431373f, 0.0000000f) }, // dragon alt
        new Color[] { new Color(0.6862745f, 0.3733333f, 1.0000000f) }, // ninja alt
        new Color[] { new Color(0.5019608f, 0.5019608f, 0.5019608f) }, // egg alt
        new Color[] { new Color(0.9372549f, 0.2509804f, 0.2078431f) }, // monster alt
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // eye alt
        new Color[] { new Color(0.4980392f, 0.4196078f, 0.6784314f), new Color(0.0823529f, 0.0705882f, 0.3647059f),  new Color(0.2588235f, 0.0784314f, 0.2941176f),  new Color(0.0627451f, 0.0549019f, 0.1725490f) }, // camo alt
        new Color[] { new Color(0.0000000f, 0.0000000f, 0.0000000f), new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // ying yang alt
        new Color[] { new Color(0.5490196f, 0.3176471f, 0.1921569f), new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // cow alt
        new Color[] { new Color(0.3725490f, 0.1882353f, 0.1803922f), new Color(0.0509804f, 0.5186275f, 0.4156863f) }, // craft alt
        new Color[] { new Color(0.4627451f, 0.3960784f, 0.9450980f) }, // planet alt
        new Color[] { new Color(1.0000000f, 0.9725490f, 0.6000000f) }, // love alt
        new Color[] { new Color(0.1137255f, 0.7372549f, 0.7058824f), new Color(0.4862745f, 0.7725490f, 0.4627451f), new Color(0.9882353f, 0.9686275f, 0.4066667f), new Color(0.9686275f, 0.5568628f, 0.3333333f) }, // aura alt
        new Color[] { new Color(0.9450980f, 0.6901961f, 0.1921569f) }, // cheese alt
        new Color[] { new Color(0.7843137f, 0.0000000f, 0.0000000f) }, // scotia alt
        new Color[] { new Color(1.0000000f, 0.2000000f, 0.2000000f) }, // poker alt
        new Color[] { new Color(1.0000000f, 0.5058824f, 0.1813725f) }, // pumpkin alt
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // web alt
        new Color[] { new Color(0.7529412f, 0.7529412f, 0.7529412f) }, // coin alt
        new Color[] { new Color(1.0000000f, 0.9607843f, 0.4078431f), new Color(0.0000000f, 0.7490196f, 0.9529412f), new Color(0.9490196f, 0.4235294f, 0.3098039f), new Color(0.2352941f, 0.7215686f, 0.4705882f) }, // magic alt
        new Color[] { new Color(0.5019608f, 0.5019608f, 0.5019608f) }, // star alt
        new Color[] { new Color(0.6117647f, 0.1372549f, 1.0000000f) }, // snake alt
        new Color[] { new Color(0.9686275f, 0.5568628f, 0.3411765f), new Color(1.0000000f, 0.9607843f, 0.4078431f), new Color(0.3372549f, 0.4549020f, 0.7254902f), new Color(0.5215686f, 0.3764706f, 0.6568628f) }, // hexagon alt
        new Color[] { new Color(0.2000000f, 0.2000000f, 0.2000000f) }, // shuriken alt
        new Color[] { new Color(0.0117647f, 0.7490196f, 0.9450980f), new Color(1.0000000f, 1.0000000f, 1.0000000f), new Color(1.0000000f, 1.0000000f, 1.0000000f), new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // lifesaver alt
        new Color[] { new Color(0.4941176f, 0.2313725f, 0.3607843f) }, // atom alt
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f) }, // matrix alt
        new Color[] { new Color(1.0000000f, 0.9607844f, 0.4078432f), new Color(0.0000000f, 0.7490196f, 0.9529412f) }, // lucky alt
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f), new Color(0.9490197f, 0.4235294f, 0.3098039f) }, // skull alt
        new Color[] { new Color(1.0000000f, 1.0000000f, 1.0000000f), new Color(0.0000000f, 0.7490196f, 0.9529412f), new Color(0.9411765f, 0.4313726f, 0.6666667f) }, // butterfly alt
    };
}
