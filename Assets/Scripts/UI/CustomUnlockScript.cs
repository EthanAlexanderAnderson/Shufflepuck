using UnityEngine;

public class CustomUnlockScript : MonoBehaviour
{
    [SerializeField] private int ID;

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
    public int Unlock()
    {
        if 
        (
            (
                // " Win {count} online matches to unlock. "
                ( !online || PlayerPrefs.GetInt( "onlineWin" ) >= count ) &&
                // " Win {count} easy matches to unlock. "
                ( !easy || PlayerPrefs.GetInt( "easyWin" ) >= count ) &&
                // " Win {count} medium matches to unlock. "
                ( !medium || PlayerPrefs.GetInt("mediumWin") >= count ) &&
                // " Win {count} hard matches to unlock. "
                ( !hard || PlayerPrefs.GetInt("hardWin") >= count ) 
            ) 
            ||
            // " Win {count} matches to unlock. "
            ( online && easy && medium && hard && ( PlayerPrefs.GetInt("onlineWin") + PlayerPrefs.GetInt("easyWin") + PlayerPrefs.GetInt("mediumWin") + PlayerPrefs.GetInt("hardWin") ) >= count)
        )
        {
            gameObject.SetActive(false);
            return ID;
        }
        return 0;
    }
}