using UnityEngine;
using System.IO;

public class FileLogger : MonoBehaviour
{
    private string logFile;

    void Awake()
    {
#if !UNITY_EDITOR
        DontDestroyOnLoad(gameObject);
        logFile = Path.Combine(Application.persistentDataPath, "build_log.txt");
        Application.logMessageReceived += HandleLog;
#endif
        Debug.Log("PersistentDataPath: " + Application.persistentDataPath);
    }

    void OnDestroy()
    {
#if !UNITY_EDITOR
        Application.logMessageReceived -= HandleLog;
#endif
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
#if !UNITY_EDITOR
        File.AppendAllText(logFile, $"[{type}] {logString}\n{stackTrace}\n");
#endif
    }
}
