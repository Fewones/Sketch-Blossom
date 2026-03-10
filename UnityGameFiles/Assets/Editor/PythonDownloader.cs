using UnityEngine;
using UnityEngine.Networking;
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

        Debug.Log("[PythonDownloader] === Starting Python environment setup ===");

        string platformFolder = "";

        #if UNITY_EDITOR_WIN
            platformFolder = "windows-latest";
        #elif UNITY_EDITOR_OSX
            platformFolder = "macos-latest";
        #elif UNITY_EDITOR_LINUX
            platformFolder = "ubuntu-latest";
        #endif

        Debug.Log("[PythonDownloader] Detected platform: " + platformFolder);

        string fullPath = Path.Combine(pythonFolder, platformFolder);
        fullPath = Path.GetFullPath(fullPath);

        Debug.Log("[PythonDownloader] Python environment path: " + fullPath);

        string pythonExe = GetPythonExePath(fullPath);

        Debug.Log("[PythonDownloader] Expected Python executable: " + pythonExe);
        Debug.Log("[PythonDownloader] Directory exists: " + Directory.Exists(fullPath));
        Debug.Log("[PythonDownloader] Python exe exists: " + File.Exists(pythonExe));

        if (!Directory.Exists(fullPath) || !File.Exists(pythonExe))
        {
            Debug.Log("[PythonDownloader] Python not found, starting download...");
            await DownloadAndExtractPython(platformFolder, fullPath);
        }
        else
        {
            Debug.Log("[PythonDownloader] Python already installed, skipping download.");
        }

        // If download failed or zip didn't contain a working Python,
        // fall back to creating a venv from system Python (macOS/Linux)
        #if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
        pythonExe = GetPythonExePath(fullPath);
        if (!File.Exists(pythonExe))
        {
            Debug.Log("[PythonDownloader] Download failed or incomplete. Creating Python venv from system python3...");
            // Remove any partial download to avoid duplicate native libraries
            if (Directory.Exists(fullPath))
                Directory.Delete(fullPath, true);
            await CreateVenv(fullPath);
            pythonExe = GetPythonExePath(fullPath);
        }

        // Ensure python binary is executable (zip extraction doesn't preserve Unix permissions)
        if (File.Exists(pythonExe))
        {
            Debug.Log("[PythonDownloader] Setting executable permission on: " + pythonExe);
            await RunProcess("chmod", "+x \"" + pythonExe + "\"");
        }
        #endif

        // Windows-specific: remove bad DLL that causes conflicts
        #if UNITY_EDITOR_WIN
        string bad_dll = Path.Combine(fullPath, "Lib/site-packages/torchvision/python311.dll");
        if (File.Exists(bad_dll))
        {
            File.Delete(bad_dll);
            Debug.Log("[PythonDownloader] Removed conflicting DLL: " + bad_dll);
        }
        #endif

        // Install/fix Python packages
        string requirementsPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "requirements.txt"));
        // Use versioned marker so old markers from failed attempts don't block us
        string depsMarker = Path.Combine(fullPath, ".deps_v6");

        Debug.Log("[PythonDownloader] Requirements file: " + requirementsPath + " (exists: " + File.Exists(requirementsPath) + ")");
        Debug.Log("[PythonDownloader] Deps marker: " + depsMarker + " (exists: " + File.Exists(depsMarker) + ")");

        // Clean up old markers
        foreach (string old in new[] { ".deps_installed", ".deps_v3", ".deps_v4", ".deps_v5" })
        {
            string oldPath = Path.Combine(fullPath, old);
            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
                Debug.Log("[PythonDownloader] Cleaned up old marker: " + old);
            }
        }

        pythonExe = GetPythonExePath(fullPath);

        if (File.Exists(pythonExe) && !File.Exists(depsMarker))
        {
            Debug.Log("[PythonDownloader] Python found but deps not installed yet, setting up packages...");

            string sitePackages = GetSitePackagesPath(fullPath);
            Debug.Log("[PythonDownloader] Site-packages path: " + sitePackages);

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
                        Debug.Log("[PythonDownloader] Removed stale package: " + pkgDir);
                    }
                    foreach (string distInfo in Directory.GetDirectories(sitePackages, pkg + "*dist-info"))
                    {
                        Directory.Delete(distInfo, true);
                        Debug.Log("[PythonDownloader] Removed stale dist-info: " + distInfo);
                    }
                }
            }

            // Upgrade pip first (old pip versions can't find newer packages)
            Debug.Log("[PythonDownloader] Upgrading pip...");
            await RunPipInstall(pythonExe, "-m pip install --upgrade pip");

            // Install packages from requirements.txt
            // On fresh venvs (macOS/Linux) this installs everything;
            // on Windows zips this fixes stale/missing packages
            Debug.Log("[PythonDownloader] Installing Python packages from requirements.txt...");
            int exitCode = -1;

            if (File.Exists(requirementsPath))
                exitCode = await RunPipInstall(pythonExe, "-m pip install -r \"" + requirementsPath + "\"");
            else
                Debug.LogWarning("[PythonDownloader] requirements.txt not found at: " + requirementsPath);

            // If requirements.txt failed (e.g. version pins incompatible with this
            // Python version), fall back to installing core packages without pins
            if (exitCode != 0)
            {
                Debug.LogWarning("[PythonDownloader] requirements.txt install failed (exit code " + exitCode + "). Installing core packages without version pins...");
                exitCode = await RunPipInstall(pythonExe,
                    "-m pip install torch torchvision transformers huggingface-hub fastapi uvicorn pillow");
            }

            if (exitCode == 0)
            {
                File.WriteAllText(depsMarker, DateTime.UtcNow.ToString());
                Debug.Log("[PythonDownloader] Dependencies installed successfully, marker written.");
            }
            else
            {
                Debug.LogError("[PythonDownloader] Failed to install dependencies (exit code " + exitCode + ")");
            }
        }
        else if (!File.Exists(pythonExe))
        {
            Debug.LogError("[PythonDownloader] Python executable not found after all setup attempts: " + pythonExe);
        }
        else
        {
            Debug.Log("[PythonDownloader] Dependencies already installed (marker exists), skipping pip install.");
        }

        downloadComplete = true;
        Debug.Log("[PythonDownloader] === Python environment setup complete ===");
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

        Debug.Log("[PythonDownloader] Download URL: " + url);
        Debug.Log("[PythonDownloader] Temp zip path: " + tempZip);

        int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            Debug.Log("[PythonDownloader] Downloading Python (attempt " + attempt + "/" + maxRetries + ")...");

            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 600; // 10 minutes
                request.downloadHandler = new DownloadHandlerFile(tempZip) { removeFileOnAbort = true };

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    float progress = request.downloadProgress;
                    if (progress >= 0)
                    {
                        int percent = (int)(progress * 100);
                        string info = "Downloading Python... " + percent + "%";
                        EditorUtility.DisplayProgressBar("[PythonDownloader] Downloading", info, progress);
                    }
                    await Task.Delay(200);
                }

                EditorUtility.ClearProgressBar();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    long fileSize = new FileInfo(tempZip).Length;
                    Debug.Log("[PythonDownloader] Download complete, file size: " + (fileSize / 1024 / 1024) + " MB");
                    break;
                }
                else
                {
                    string error = request.error;
                    if (attempt == maxRetries)
                    {
                        Debug.LogError("[PythonDownloader] Download failed after " + maxRetries + " attempts: " + error);
                        return;
                    }
                    int delaySeconds = (int)Math.Pow(2, attempt);
                    Debug.LogWarning("[PythonDownloader] Attempt " + attempt + " failed, retrying in " + delaySeconds + "s: " + error);
                    await Task.Delay(delaySeconds * 1000);
                }
            }
        }

        Debug.Log("[PythonDownloader] Extracting zip to: " + targetPath);
        EditorUtility.DisplayProgressBar("[PythonDownloader] Extracting", "Extracting Python environment...", 0.5f);

        if (Directory.Exists(targetPath))
            Directory.Delete(targetPath, true);

        ZipFile.ExtractToDirectory(tempZip, targetPath);
        File.Delete(tempZip);
        EditorUtility.ClearProgressBar();

        Debug.Log("[PythonDownloader] Python downloaded and extracted to: " + targetPath);
    }
}
