using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PythonServerManager: MonoBehaviour
{
    private Process pythonProcess;

    public void Start()
    {
        string pythonPath = "";
        #if UNITY_STANDALONE_WIN
            pythonPath = "Python/windows-latest/python.exe";
        #elif UNITY_STANDALONE_LINUX
            pythonPath = "Python/ubuntu-latest/bin/python3";
        #elif UNITY_STANDALONE_OSX
            pythonPath = "Python/macos-latest/bin/python3";
        #endif
        pythonPath = Path.Combine(Application.dataPath, pythonPath);
        UnityEngine.Debug.Log(pythonPath);
        pythonPath = Path.GetFullPath(pythonPath);

        string scriptPath = Path.Combine(Application.dataPath, "Python/shared/TinyCLIP.py");

        UnityEngine.Debug.Log(pythonPath);

        pythonProcess = new Process();
        pythonProcess.StartInfo.FileName = pythonPath;
        pythonProcess.StartInfo.Arguments = scriptPath;
        pythonProcess.StartInfo.UseShellExecute = true;
        pythonProcess.StartInfo.CreateNoWindow = false;
        pythonProcess.Start();

        UnityEngine.Debug.Log("Python server started.");
    }

    public void Cleanup()
    {
        if (!pythonProcess.HasExited)
            pythonProcess.Kill();
            UnityEngine.Debug.Log("Python server deactivated.");
    }

    public void OnDestroy()
    {
        Cleanup();
    }
}