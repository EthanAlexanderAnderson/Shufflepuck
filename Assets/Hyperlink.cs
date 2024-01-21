using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hyperlink : MonoBehaviour
{
    // Start is called before the first frame update
    public void OpenWebsite(string url)
    {
        Application.OpenURL(url);
    }
}
