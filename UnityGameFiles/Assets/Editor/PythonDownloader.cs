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

        string pythonExe = GetPythonExePath(fullPath);

        if (!Directory.Exists(fullPath) || !File.Exists(pythonExe))
        {
            Debug.Log("Python not found, downloading...");
            await DownloadAndExtractPython(platformFolder, fullPath);
        }

        // If download failed or zip didn't contain a working Python,
        // fall back to creating a venv from system Python (macOS/Linux)
        #if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
        pythonExe = GetPythonExePath(fullPath);
        if (!File.Exists(pythonExe))
        {
            Debug.Log("Download failed or incomplete. Creating Python venv from system python3...");
            // Remove any partial download to avoid duplicate native libraries
            if (Directory.Exists(fullPath))
                Directory.Delete(fullPath, true);
            await CreateVenv(fullPath);
            pythonExe = GetPythonExePath(fullPath);
        }

        // Ensure python binary is executable (zip extraction doesn't preserve Unix permissions)
        if (File.Exists(pythonExe))
        {
            await RunProcess("chmod", "+x \"" + pythonExe + "\"");
        }
        #endif

        // Windows-specific: remove bad DLL that causes conflicts
        #if UNITY_EDITOR_WIN
        string bad_dll = Path.Combine(fullPath, "Lib/site-packages/torchvision/python311.dll");
        if (File.Exists(bad_dll))
        {
            File.Delete(bad_dll);
            Debug.Log("Datei gelöscht: " + bad_dll);
        }
        #endif

        // Install/fix Python packages
        string requirementsPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "requirements.txt"));
        // Use versioned marker so old markers from failed attempts don't block us
        string depsMarker = Path.Combine(fullPath, ".deps_v6");
        // Clean up old markers
        foreach (string old in new[] { ".deps_installed", ".deps_v3", ".deps_v4", ".deps_v5" })
        {
            string oldPath = Path.Combine(fullPath, old);
            if (File.Exists(oldPath)) File.Delete(oldPath);
        }

        pythonExe = GetPythonExePath(fullPath);

        if (File.Exists(pythonExe) && !File.Exists(depsMarker))
        {
            string sitePackages = GetSitePackagesPath(fullPath);

            // Delete stale package directories that lack RECORD files (Windows zip issue)
            if (!string.IsNullOrEmpty(sitePackages) && Directory.Exists(sitePackages))
            {
                string[] staleDirs = { "transformers", "huggingface_hub" };
                foreach (string pkg in staleDirs)
                {
                    string pkgDir = Path.Combine(sitePackages, pkg);
                    if (Directory.Exists(pkgDir))
                    {
                        Directory.Delete(pkgDir, true);
                        Debug.Log("Removed stale package: " + pkgDir);
                    }
                    foreach (string distInfo in Directory.GetDirectories(sitePackages, pkg + "*dist-info"))
                    {
                        Directory.Delete(distInfo, true);
                        Debug.Log("Removed stale dist-info: " + distInfo);
                    }
                }
            }

            // Upgrade pip first (old pip versions can't find newer packages)
            Debug.Log("Upgrading pip...");
            await RunPipInstall(pythonExe, "-m pip install --upgrade pip");

            // Install packages from requirements.txt
            // On fresh venvs (macOS/Linux) this installs everything;
            // on Windows zips this fixes stale/missing packages
            Debug.Log("Installing Python packages...");
            int exitCode = -1;

            if (File.Exists(requirementsPath))
                exitCode = await RunPipInstall(pythonExe, "-m pip install -r \"" + requirementsPath + "\"");

            // If requirements.txt failed (e.g. version pins incompatible with this
            // Python version), fall back to installing core packages without pins
            if (exitCode != 0)
            {
                Debug.LogWarning("requirements.txt install failed. Installing core packages without version pins...");
                exitCode = await RunPipInstall(pythonExe,
                    "-m pip install torch torchvision transformers huggingface-hub fastapi uvicorn pillow");
            }

            if (exitCode == 0)
                File.WriteAllText(depsMarker, DateTime.UtcNow.ToString());
        }

        downloadComplete = true;
    }

    static string GetPythonExePath(string envPath)
    {
        #if UNITY_EDITOR_WIN
            return Path.Combine(envPath, "python.exe");
        #else
            return Path.Combine(envPath, "bin", "python3");
        #endif
    }

    static string GetSitePackagesPath(string envPath)
    {
        #if UNITY_EDITOR_WIN
            return Path.Combine(envPath, "Lib", "site-packages");
        #else
            // macOS/Linux venvs use lib/python3.x/site-packages
            string libDir = Path.Combine(envPath, "lib");
            if (Directory.Exists(libDir))
            {
                foreach (string pyDir in Directory.GetDirectories(libDir, "python3*"))
                {
                    string sp = Path.Combine(pyDir, "site-packages");
                    if (Directory.Exists(sp))
                        return sp;
                }
            }
            return Path.Combine(envPath, "lib", "site-packages");
        #endif
    }

    static async Task CreateVenv(string targetPath)
    {
        await Task.Run(() => {
            var venv = new System.Diagnostics.Process();
            venv.StartInfo.FileName = "python3";
            venv.StartInfo.Arguments = "-m venv --copies \"" + targetPath + "\"";
            venv.StartInfo.UseShellExecute = false;
            venv.StartInfo.RedirectStandardOutput = true;
            venv.StartInfo.RedirectStandardError = true;
            venv.StartInfo.CreateNoWindow = true;
            venv.OutputDataReceived += (sender, args) => {
                if (!string.IsNullOrEmpty(args.Data))
                    Debug.Log("[venv] " + args.Data);
            };
            venv.ErrorDataReceived += (sender, args) => {
                if (!string.IsNullOrEmpty(args.Data))
                    Debug.LogWarning("[venv] " + args.Data);
            };
            venv.Start();
            venv.BeginOutputReadLine();
            venv.BeginErrorReadLine();
            venv.WaitForExit();
            if (venv.ExitCode == 0)
                Debug.Log("Python venv created at: " + targetPath);
            else
                Debug.LogError("Failed to create Python venv (exit code " + venv.ExitCode + "). Ensure python3 is installed.");
        });
    }

    static async Task<int> RunPipInstall(string pythonExe, string arguments)
    {
        int exitCode = -1;
        await Task.Run(() => {
            var pip = new System.Diagnostics.Process();
            pip.StartInfo.FileName = pythonExe;
            pip.StartInfo.Arguments = arguments;
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
            exitCode = pip.ExitCode;
            if (exitCode == 0)
                Debug.Log("Python packages installed successfully.");
            else
                Debug.LogError("pip install failed (exit code " + exitCode + ")");
        });
        return exitCode;
    }

    static async Task RunProcess(string fileName, string arguments)
    {
        await Task.Run(() => {
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            proc.WaitForExit();
        });
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
            http.Timeout = TimeSpan.FromMinutes(10);
            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try {
                    Debug.Log("Downloading Python (attempt " + attempt + "/" + maxRetries + ")...");
                    var response = await http.GetAsync(url, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    Debug.Log("Status Code: " + response.StatusCode);
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                    break;
                }
                catch (Exception ex) {
                    if (attempt == maxRetries)
                    {
                        Debug.LogError("Download failed after " + maxRetries + " attempts: " + ex);
                        return;
                    }
                    int delaySeconds = (int)Math.Pow(2, attempt);
                    Debug.LogWarning("Download attempt " + attempt + " failed, retrying in " + delaySeconds + "s: " + ex.Message);
                    await Task.Delay(delaySeconds * 1000);
                }
            }
        }

        if (Directory.Exists(targetPath))
            Directory.Delete(targetPath, true);

        ZipFile.ExtractToDirectory(tempZip, targetPath);
        File.Delete(tempZip);

        Debug.Log("Python downloaded and extracted to: " + targetPath);
    }
}
