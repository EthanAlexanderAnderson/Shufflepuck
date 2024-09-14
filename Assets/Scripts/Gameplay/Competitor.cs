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
    public int puckCount = 5;
    public bool isTurn;
    public bool isShooting;
    public bool goingFirst;

    // default constructor
    public Competitor() { return; }

    // optional constructor
    public Competitor(int puckID) { SetSprite(puckID); }

    public void SetSprite(int puckID)
    {
        puckSpriteID = puckID;
        puckSprite = PuckSkinManager.Instance.ColorIDtoPuckSprite(puckID);
    }

    public void ShootActivePuck(float angleParameter, float powerParameter, float spinParameter = 50)
    {
        activePuckScript.Shoot(angleParameter, powerParameter, spinParameter);
        isShooting = false;
        puckCount--;
    }

    public void ResetProperties()
    {
        score = 0;
        puckCount = 5;
        isShooting = false;
    }
}
