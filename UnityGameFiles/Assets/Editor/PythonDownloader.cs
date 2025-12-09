using UnityEngine;
using UnityEditor;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Threading.Tasks;
using System;

[InitializeOnLoad]
public class PythonDownloader
{
    
    static string pythonFolder = "Assets/Python/";

    static PythonDownloader() {
            EditorApplication.update += CheckAndDownloadPython;
        }

    static async void CheckAndDownloadPython() {
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
        fullPath = Path.GetFullPath(fullPath);

        if (!Directory.Exists(fullPath) || Directory.GetFiles(fullPath).Length == 0)
        {
            Debug.Log("Python not found, downloading...");
            await DownloadAndExtractPython(platformFolder, fullPath);
        }

        string bad_dll = Path.Combine(fullPath, "Lib/site-packages/torchvision/python311.dll");

        if (File.Exists(Path.Combine(bad_dll)))
        {
            File.Delete(bad_dll);
            Debug.Log("Datei gelöscht: " + bad_dll);
        }
    }

    static async Task DownloadAndExtractPython(string platform, string targetPath) {
        string zipName = platform + ".zip";

        string baseUrl = "https://github.com/Fewones/Sketch-Blossom/releases/download/sketchblossom-python/";
        #if UNITY_EDITOR_WIN
        baseUrl = "https://github.com/Fewones/Sketch-Blossom/releases/download/sketchblossom-python-win/";
        #endif

        string url = baseUrl + zipName;
        string tempZip = Path.Combine(Path.GetTempPath(), zipName);
        Debug.Log(url);

        //using var handler = new System.Net.Http.HttpClientHandler {AllowAutoRedirect = true};
        using (var http = new System.Net.Http.HttpClient()) {    //handler) {
            //http.DefaultRequestHeaders.UserAgent.ParseAdd("UnityPythonDownloader/1.0");
            //http.DefaultRequestHeaders.Accept.ParseAdd("application/octet-stream");

            try {
                var response = await http.GetAsync(url, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
                Debug.Log("Status Code: " + response.StatusCode);
                Debug.Log("Redirect Location: " + response.Headers.Location);
                var bytes = await http.GetByteArrayAsync(url);
                File.WriteAllBytes(tempZip, bytes);
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                }
        }

        if (Directory.Exists(targetPath))
            Directory.Delete(targetPath, true);

        ZipFile.ExtractToDirectory(tempZip, targetPath);
        File.Delete(tempZip);

        Debug.Log("Python downloaded and extracted to: " + targetPath);
    }
}