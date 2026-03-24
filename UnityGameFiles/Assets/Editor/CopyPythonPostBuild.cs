using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class CopyPythonPostBuild : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        string buildPath = report.summary.outputPath;
        string buildDir = Path.GetDirectoryName(buildPath);
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string sourcePythonDir = Path.Combine(projectRoot, "Python");
        string destPythonDir = Path.Combine(buildDir, "Python");

        if (!Directory.Exists(sourcePythonDir))
        {
            Debug.LogWarning("[CopyPythonPostBuild] Python folder not found at: " + sourcePythonDir);
            return;
        }

        Debug.Log("[CopyPythonPostBuild] Copying Python folder to build output: " + destPythonDir);

        CopyDirectory(sourcePythonDir, destPythonDir);

        Debug.Log("[CopyPythonPostBuild] Python folder copied successfully.");
    }

    static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
    }
}
