using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PythonServerManager
{
    private Process pythonProcess;

    public void Start()
    {
        string pythonPath = "";
        #if UNITY_STANDALONE_WIN
            pythonPath = Path.Combine(Application.dataPath, "Python/win/python-3.13.9-embed-amd64/python.exe");
        #elif UNITY_STANDALONE_LINUX
            pythonPath = Path.Combine(Application.dataPath, "/Python/linux/bin/python3");
        #elif UNITY_STANDALONE_OSX
            pythonPath = Path.Combine(Application.dataPath, "/Python/mac/bin/python3");
        #endif
        string scriptPath = Path.Combine(Application.dataPath, "Python/shared/TinyCLIP.py");

        pythonProcess = new Process();
        pythonProcess.StartInfo.FileName = pythonPath;
        pythonProcess.StartInfo.Arguments = scriptPath;
        pythonProcess.StartInfo.UseShellExecute = true;
        pythonProcess.StartInfo.CreateNoWindow = false;
        pythonProcess.Start();

        UnityEngine.Debug.Log("Python server started.");
    }

    public void OnApplicationQuit()
    {
        if (!pythonProcess.HasExited)
            pythonProcess.Kill();
    }
}