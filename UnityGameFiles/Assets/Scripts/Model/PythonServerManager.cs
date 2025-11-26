using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PythonServerManager
{
    private Process pythonProcess;

    public void Start()
    {
        string pythonExe = Path.Combine(Application.dataPath, "Python/win/python-3.13.9-embed-amd64/python.exe");
        string scriptPath = Path.Combine(Application.dataPath, "Python/shared/TinyCLIP.py");

        pythonProcess = new Process();
        pythonProcess.StartInfo.FileName = pythonExe;
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