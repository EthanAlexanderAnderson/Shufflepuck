// Object Oriented style object to be used by the logic scripts 

using Unity.Collections;
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
    public FixedString32Bytes username;
    public int score;
    public int scoreBonus;
    public int wins;
    public int puckCount = 5;
    public bool isTurn;
    public bool isShooting;
    public bool goingFirst;

    // powerups
    public bool isOmniscient;

    // default constructor
    public Competitor() { return; }

    // puck ID constructor
    public Competitor(int puckID) { SetSprite(puckID); }

    // puck ID and username constructor
    public Competitor(int puckID, FixedString32Bytes username) { SetSprite(puckID); this.username = username; }

    public void SetSprite(int puckID)
    {
        puckSpriteID = puckID;
        puckSprite = PuckSkinManager.Instance.ColorIDtoPuckSprite(puckID);
    }

    public void ShootActivePuck(float angleParameter, float powerParameter, float spinParameter = 50, bool decrementPuckCount = true)
    {
        activePuckScript.Shoot(angleParameter, powerParameter, spinParameter);
        isShooting = false;
        if (decrementPuckCount) { puckCount--; } // this is necessary for Triple powerup
    }

    public void ResetProperties()
    {
        score = 0;
        scoreBonus = 0;
        puckCount = 5;
        isShooting = false;
    }

    public int GetScore()
    {
        return score + scoreBonus;
    }
}
