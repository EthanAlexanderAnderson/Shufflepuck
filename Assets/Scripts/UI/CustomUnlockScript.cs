using UnityEngine;

public class CustomUnlockScript : MonoBehaviour
{
    [SerializeField] private int ID;

    [SerializeField] private int type;
    [SerializeField] private int count;

    [SerializeField] private bool online;
    [SerializeField] private bool easy;
    [SerializeField] private bool medium;
    [SerializeField] private bool hard;

    // Start is called before the first frame update

    private void Start()
    {
        Unlock();
    }
    private void OnEnable()
    {
        Unlock();
    }

    public int Unlock()
    {
        // wins
        if (type == 0)
        {
            if
            (
                (
                    // " Win {count} online matches to unlock. "
                    (!online || PlayerPrefs.GetInt("onlineWin") >= count) &&
                    // " Win {count} easy matches to unlock. "
                    (!easy || PlayerPrefs.GetInt("easyWin") >= count) &&
                    // " Win {count} medium matches to unlock. "
                    (!medium || PlayerPrefs.GetInt("mediumWin") >= count) &&
                    // " Win {count} hard matches to unlock. "
                    (!hard || PlayerPrefs.GetInt("hardWin") >= count)
                )
                ||
                // " Win {count} matches to unlock. "
                (online && easy && medium && hard && (PlayerPrefs.GetInt("onlineWin") + PlayerPrefs.GetInt("easyWin") + PlayerPrefs.GetInt("mediumWin") + PlayerPrefs.GetInt("hardWin")) >= count)
            )
            {
                gameObject.SetActive(false);
                return ID;
            }
        }
        // highscore
        else if (type == 1)
        {
            if
            (
                (
                    // " Reach an easy highscore of {count} to unlock. "
                    (!easy || PlayerPrefs.GetInt("easyHighscore") >= count) &&
                    // " Reach a medium highscore of {count} to unlock. "
                    (!medium || PlayerPrefs.GetInt("mediumHighscore") >= count) &&
                    // " Reach a hard highscore of {count} to unlock. "
                    (!hard || PlayerPrefs.GetInt("hardHighscore") >= count)
                )
                ||
                // " Reach a combined highscore of {count} to unlock. "
                (easy && medium && hard && (PlayerPrefs.GetInt("easyHighscore") + PlayerPrefs.GetInt("mediumHighscore") + PlayerPrefs.GetInt("hardHighscore")) >= count)
            )
            {
                gameObject.SetActive(false);
                return ID;
            }
        }
        // simple ID based
        else if (type == 2)
        {
            if (PlayerPrefs.GetInt("puck"+ID.ToString()+"unlocked") == 1 )
            {
                gameObject.SetActive(false);
                return ID;
            }
        }
        return 0;
    }
}