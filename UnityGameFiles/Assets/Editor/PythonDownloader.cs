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
    private static bool downloadComplete = false;
    static string pythonFolder = "Assets/Python/";

    static PythonDownloader() {
            EditorApplication.update += CheckAndDownloadPython;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (!downloadComplete)
            {
                // Stop Playmode
                Debug.LogWarning("Download läuft noch! Playmode gestoppt.");
                EditorApplication.isPlaying = false;
            }
        }
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

        // Sync packages with requirements.txt to fix version mismatches
        // requirements.txt is in the repo root, one level above the Unity project folder
        string requirementsPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "requirements.txt"));
        string depsMarker = Path.Combine(fullPath, ".deps_installed");

        if (File.Exists(requirementsPath) && !File.Exists(depsMarker))
        {
            string pythonExe = "";
            #if UNITY_EDITOR_WIN
                pythonExe = Path.Combine(fullPath, "python.exe");
            #elif UNITY_EDITOR_OSX
                pythonExe = Path.Combine(fullPath, "bin", "python3");
            #elif UNITY_EDITOR_LINUX
                pythonExe = Path.Combine(fullPath, "bin", "python3");
            #endif

            if (File.Exists(pythonExe))
            {
                // Delete stale package directories that lack RECORD files,
                // which prevents pip from uninstalling them cleanly
                string sitePackages = Path.Combine(fullPath, "Lib", "site-packages");
                string[] staleDirs = { "transformers", "huggingface_hub" };
                foreach (string pkg in staleDirs)
                {
                    string pkgDir = Path.Combine(sitePackages, pkg);
                    if (Directory.Exists(pkgDir))
                    {
                        Directory.Delete(pkgDir, true);
                        Debug.Log("Removed stale package: " + pkgDir);
                    }
                    // Also remove any .dist-info directories for this package
                    foreach (string distInfo in Directory.GetDirectories(sitePackages, pkg + "*dist-info"))
                    {
                        Directory.Delete(distInfo, true);
                        Debug.Log("Removed stale dist-info: " + distInfo);
                    }
                }

                Debug.Log("Installing transformers and huggingface-hub...");
                await Task.Run(() => {
                    var pip = new System.Diagnostics.Process();
                    pip.StartInfo.FileName = pythonExe;
                    pip.StartInfo.Arguments = "-m pip install --no-deps transformers==4.57.3 huggingface-hub==0.36.0";
                    pip.StartInfo.UseShellExecute = false;
                    pip.StartInfo.RedirectStandardOutput = true;
                    pip.StartInfo.RedirectStandardError = true;
                    pip.StartInfo.CreateNoWindow = true;
                    pip.OutputDataReceived += (sender, args) => {
                        if (!string.IsNullOrEmpty(args.Data))
                            Debug.Log("[pip] " + args.Data);
                    };
                    pip.ErrorDataReceived += (sender, args) => {
                        if (!string.IsNullOrEmpty(args.Data))
                            Debug.LogWarning("[pip] " + args.Data);
                    };
                    pip.Start();
                    pip.BeginOutputReadLine();
                    pip.BeginErrorReadLine();
                    pip.WaitForExit();
                    if (pip.ExitCode == 0)
                        Debug.Log("Python packages updated successfully.");
                    else
                        Debug.LogError("pip install failed (exit code " + pip.ExitCode + ")");
                });
                File.WriteAllText(depsMarker, DateTime.UtcNow.ToString());
            }
        }

        downloadComplete = true;
    }

    static async Task DownloadAndExtractPython(string platform, string targetPath) {
        string zipName = platform + ".zip";

        string baseUrl = "https://github.com/Fewones/Sketch-Blossom/releases/download/sketchblossom-python/";
        #if UNITY_EDITOR_WIN
        baseUrl = "https://github.com/Fewones/Sketch-Blossom/releases/download/sketchblossom-python-win/";
        #endif

        string url = baseUrl + zipName;
        string tempZip = Path.Combine(Path.GetTempPath(), zipName);

        using (var http = new System.Net.Http.HttpClient()) {    
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