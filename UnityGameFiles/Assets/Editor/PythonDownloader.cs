using UnityEngine;
using UnityEditor;
using System.IO;
using System.Net;
using System.IO.Compression;

[InitializeOnLoad]
public class PythonDownloader
{
    static string baseUrl = "https://github.com/Fewones/Sketch-Blossom/releases/download/sketchblossom-python/";
    static string pythonFolder = "../Python/";

    static PythonDownloader() {
            EditorApplication.update += CheckAndDownloadPython;
        }

    static void CheckAndDownloadPython() {
        EditorApplication.update -= CheckAndDownloadPython;

        string platformFolder = "";

        #if UNITY_EDITOR_WIN
            platformFolder = "windows-latest";
        #elif UNITY_EDITOR_OSX
            platformFolder = "macos-latest";
        #elif UNITY_EDITOR_LINUX
            platformFolder = "ubuntu-latest";
        #endif

        string fullPath = Path.Combine(pythonFolder, platformFolder);

        if (!Directory.Exists(fullPath) || Directory.GetFiles(fullPath).Length == 0)
        {
            Debug.Log("Python not found, downloading...");
            DownloadAndExtractPython(platformFolder, fullPath);
        }
    }

    static void DownloadAndExtractPython(string platform, string targetPath) {
        string zipName = platform + ".zip";
        string url = baseUrl + zipName;
        string tempZip = Path.Combine(Path.GetTempPath(), zipName);

        using (var client = new WebClient()) {
            client.DownloadFile(url, tempZip);
        }

        if (Directory.Exists(targetPath))
            Directory.Delete(targetPath, true);

        ZipFile.ExtractToDirectory(tempZip, targetPath);
        File.Delete(tempZip);

        Debug.Log("Python downloaded and extracted to: " + targetPath);
    }
}