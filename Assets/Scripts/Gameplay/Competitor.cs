// Object Oriented style object to be used by the logic scripts 

using UnityEngine;

public class Competitor
{
    // Offline only
    public bool isPlayer;
    // Online only
    public ulong clientID;

    public GameObject activePuckObject;
    public PuckScript activePuckScript;
    public Sprite puckSprite;
    public int puckSpriteID;
    public int score;
    public int puckCount;
    public bool isTurn;
    public bool isShooting;

    // default constructor
    public Competitor() {
        puckCount = 5;
        return; 
    }

    // optional constructor
    public Competitor(int puckID)
    {
        puckCount = 5;
        SetSprite(puckID);
    }

    public void SetSprite(int puckID)
    {
        puckSpriteID = puckID;
        puckSprite = PuckSkinManager.Instance.ColorIDtoPuckSprite(puckID);
    }
}
