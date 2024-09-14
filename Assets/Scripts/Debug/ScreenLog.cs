// This is only for development builds on android, basically shows the unity console on screen

using UnityEngine;
using System.Collections;

public class ScreenLog : MonoBehaviour
{
    uint qsize = 15;  // number of messages to keep
    Queue myLogQueue = new Queue(); 

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
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
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