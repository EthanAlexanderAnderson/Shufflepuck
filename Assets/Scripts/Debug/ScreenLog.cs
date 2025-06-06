// Enabled with debug mode, shows the logs on screen

using UnityEngine;
using System.Collections;
using System.Linq;

public class ScreenLog : MonoBehaviour
{
    public static ScreenLog Instance;

    uint qsize = 20;  // number of messages to keep
    int maxTotalCharCount = 1000;  // maximum total character count of all logs
    int maxLogLength = 500;  // maximum length of an individual log

    Queue myLogQueue = new Queue();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (PlayerPrefs.GetInt("puck") != -3 && PlayerPrefs.GetInt("debug", 0) != 1)
        {
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (logString.Contains("Google.Logger") || logString.Contains("manifest") || logString.Contains("Manifest") || logString.Contains("External")) return;

        // Ensure the log entry is not longer than the maxLogLength
        string logEntry = "[" + type + "] : " + TrimLog(logString);

        myLogQueue.Enqueue(logEntry);

        if (type == LogType.Exception)
            myLogQueue.Enqueue(TrimLog(stackTrace));

        TrimQueue();
    }

    string TrimLog(string log)
    {
        // Trims any log to the maxLogLength if it exceeds that length
        if (log.Length > maxLogLength)
        {
            return log.Substring(0, maxLogLength) + "...";  // Add ellipsis to indicate truncation
        }
        return log;
    }

    void TrimQueue()
    {
        while (myLogQueue.Count > qsize || TotalCharCount() > maxTotalCharCount)
        {
            // Ensure at least one log remains in the queue
            if (myLogQueue.Count <= 3)
                break;

            myLogQueue.Dequeue();
        }
    }

    int TotalCharCount()
    {
        return myLogQueue.Cast<string>().Sum(log => log.Length);
    }

    void OnGUI()
    {
        GUIStyle style = new();
        style.fontSize = Screen.width / 25;
        style.normal.textColor = Color.red;
        style.wordWrap = true;
        GUILayout.BeginArea(new Rect(Screen.width / 20, Screen.height/20, Screen.width - (Screen.width / 12), Screen.height - (Screen.height / 20)));
        GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()), style);
        GUILayout.EndArea();
    }
}