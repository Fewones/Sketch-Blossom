using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PythonServerManager: MonoBehaviour
{
    private static PythonServerManager _instance;
    private Process pythonProcess;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoStart()
    {
        if (_instance != null) return;

        GameObject serverObj = new GameObject("PythonServerManager");
        _instance = serverObj.AddComponent<PythonServerManager>();
        DontDestroyOnLoad(serverObj);
        UnityEngine.Debug.Log("PythonServerManager: Auto-started at game launch");
    }

    public static PythonServerManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void Start()
    {
        string pythonPath = "";
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        #if UNITY_EDITOR_WIN
            pythonPath = "Python/windows-latest/python.exe";
        #elif UNITY_EDITOR_LINUX
            pythonPath = "Python/ubuntu-latest/bin/python3";
        #elif UNITY_EDITOR_OSX
            pythonPath = "Python/macos-latest/bin/python3";
        #elif UNITY_STANDALONE_WIN
            pythonPath = "Python/windows-latest/python.exe";
        #elif UNITY_STANDALONE_LINUX
            pythonPath = "Python/ubuntu-latest/bin/python3";
        #elif UNITY_STANDALONE_OSX
            pythonPath = "Python/macos-latest/bin/python3";
        #endif
        pythonPath = Path.Combine(projectRoot, pythonPath);
        pythonPath = Path.GetFullPath(pythonPath);

        string scriptPath = Path.Combine(projectRoot, "Python/shared/TinyCLIP.py");
        string workingDir = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        UnityEngine.Debug.Log("Python path: " + pythonPath);
        UnityEngine.Debug.Log("Script path: " + scriptPath);
        UnityEngine.Debug.Log("Working dir: " + workingDir);

        if (!File.Exists(pythonPath))
        {
            UnityEngine.Debug.LogError("Python executable not found at: " + pythonPath);
            return;
        }

        pythonProcess = new Process();
        pythonProcess.StartInfo.FileName = pythonPath;
        pythonProcess.StartInfo.Arguments = "\"" + scriptPath + "\"";
        pythonProcess.StartInfo.WorkingDirectory = workingDir;
        pythonProcess.StartInfo.UseShellExecute = false;
        pythonProcess.StartInfo.RedirectStandardOutput = true;
        pythonProcess.StartInfo.RedirectStandardError = true;

        pythonProcess.OutputDataReceived += (sender, args) => {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.Log("[TinyCLIP] " + args.Data);
        };
        pythonProcess.ErrorDataReceived += (sender, args) => {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.LogWarning("[TinyCLIP] " + args.Data);
        };

        pythonProcess.Start();
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();

        UnityEngine.Debug.Log("Python server started.");
    }

    public void Cleanup()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.Kill();
            UnityEngine.Debug.Log("Python server deactivated.");
        }
    }

    public void OnDestroy()
    {
        Cleanup();
    }
}
