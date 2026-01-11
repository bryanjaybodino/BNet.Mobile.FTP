using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using AndroidX.AppCompat.App;
using BNet.Mobile.FTP.Server;
using BNet.Mobile.FTP.Services.NetworkServices;
using BNet.Mobile.FTP.Services.PermissionServices;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using static Google.Android.Material.Tabs.TabLayout;

namespace BNet.Mobile.FTP
{

    [Activity(Label = "FTP Server",
              Icon = "@drawable/icon",
              Theme = "@style/AppTheme",
              MainLauncher = true,
              HardwareAccelerated = true,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    [MetaData("android:largeHeap", Value = "true")]
    public class MainActivity : AppCompatActivity
    {

        public static FTP.Server.Commands ftpServer = new Server.Commands();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Copy the HTML file from assets to internal storage
            CopyAssetsToInternalStorage();
            //CopyAssetsToExternalStorage();

            //Full Screen Application
            FullScreen();

            //Check Permission
            ManageStorage.RequestExternalPermission();

            //Run Webvview
            var webView = FindViewById<WebView>(Resource.Id.webview);
            webView.Settings.JavaScriptEnabled = true;                      // Enable JavaScript in WebView
            webView.Settings.DomStorageEnabled = true;                      // Enable local storage if used
            webView.Settings.AllowUniversalAccessFromFileURLs = true;       // Allow access to files from file URLs
            webView.Settings.AllowFileAccessFromFileURLs = true;            // Allow file access from file URLs
            webView.Settings.AllowContentAccess = true;                     // Allow access to content
            var scriptContext = new ScriptContext(this, webView);
            webView.AddJavascriptInterface(scriptContext, "ScriptContext");
            WebView.SetWebContentsDebuggingEnabled(true);                   // Enable debugging (Logcat or Chrome DevTools)
            webView.LoadUrl($"file:///android_asset/BNet.Mobile.FTP.html"); // Default Landing Page    file:///android_asset/ -> ASSETS FOLDER


            var rootFolder = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            ftpServer.Setup(rootFolder, NetworkChecker.PortNumber());

            StartTimer(scriptContext);

            FTPLogger.OnLog += log =>
            {
                FTPLogs(webView, log);
            };
        }


        public void FTPLogs(WebView webView, string log)
        {
            if (string.IsNullOrEmpty(log)) return;

            // Offload heavy processing to a background task
            Task.Run(() =>
            {
                string upper = log.ToUpperInvariant();
                string color = "#ffffff"; // default

                if (upper.Contains("USER")) color = "#00ffff";
                else if (upper.Contains("PASS")) color = "#ff5555";
                else if (upper.Contains("CWD")) color = "#ffaa00";
                else if (upper.Contains("PWD")) color = "#ffaa00";
                else if (upper.Contains("LIST")) color = "#55ff55";
                else if (upper.Contains("RETR")) color = "#00ff00";
                else if (upper.Contains("STOR")) color = "#00ff00";
                else if (upper.Contains("DELE")) color = "#ff0000";
                else if (upper.Contains("MKD")) color = "#8888ff";
                else if (upper.Contains("RMD")) color = "#8888ff";
                else if (upper.Contains("QUIT")) color = "#aaaaaa";
                else if (upper.Length >= 3 && char.IsDigit(upper[0]) && char.IsDigit(upper[1]) && char.IsDigit(upper[2]))
                    color = "#ffff00"; // FTP response codes


                string escapedValue = log
                    .Replace("'", "\\'")
                    .Replace("USER", "USER", StringComparison.OrdinalIgnoreCase)       // keep as USER                 
                    .Replace("CWD", "", StringComparison.OrdinalIgnoreCase)      // change working directory
                    .Replace("PWD", "", StringComparison.OrdinalIgnoreCase)         // print working directory
                    .Replace("STOR", "UPLOAD", StringComparison.OrdinalIgnoreCase)    // STOR → UPLOAD
                    .Replace("RETR", "DOWNLOAD", StringComparison.OrdinalIgnoreCase)  // RETR → DOWNLOAD
                    .Replace("LIST", "", StringComparison.OrdinalIgnoreCase)   // LIST → DIRLIST
                    .Replace("DELE", "DELETE", StringComparison.OrdinalIgnoreCase)    // DELE → DELETE
                    .Replace("MKD", "", StringComparison.OrdinalIgnoreCase)      // MKD → MKDIR
                    .Replace("RMD", "", StringComparison.OrdinalIgnoreCase)      // RMD → RMDIR
                    .Replace("QUIT", "QUIT", StringComparison.OrdinalIgnoreCase)    // keep QUIT
                    .Trim();


                string js =
                       $"var logDiv = document.getElementById('log');" +
                       $"logDiv.innerHTML = " +
                       $"\"<span style='color:{color}; font-family: monospace;'>{escapedValue}</span><br>\" + logDiv.innerHTML;";

                // Only switch to UI thread for JS evaluation
                webView.Post(() =>
                {
                    webView.EvaluateJavascript(js, null);
                });
            });
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async void StartTimer(ScriptContext scriptContext)
        {
            await Task.Run(() =>
            {
                // create a timer
                Timer timer = new Timer(1000); // 1000ms = 1 second
                timer.Elapsed += (sender, e) =>
                {
                    // Switch to UI thread
                    RunOnUiThread(() =>
                    {
                        HtmlElement.Update(scriptContext);
                    });
                };
                timer.Start();
            });
        }

        // Function to copy file from Assets to internal storage
        private void CopyAssetsToInternalStorage()
        {
            try
            {
                void CopyRecursive(string assetPath, string internalPath)
                {
                    string[] items = Assets.List(assetPath);

                    foreach (string item in items)
                    {
                        string nextAssetPath = string.IsNullOrEmpty(assetPath)
                            ? item
                            : $"{assetPath}/{item}";

                        string nextInternalPath = Path.Combine(internalPath, item);

                        // Directory
                        if (Assets.List(nextAssetPath).Length > 0)
                        {
                            Directory.CreateDirectory(nextInternalPath);
                            CopyRecursive(nextAssetPath, nextInternalPath);
                        }
                        // File
                        else
                        {
                            using (var assetStream = Assets.Open(nextAssetPath))
                            using (var fileStream = new FileStream(nextInternalPath, FileMode.Create))
                            {
                                assetStream.CopyTo(fileStream);
                            }
                        }
                    }
                }

                // Start from asset root → internal storage root
                CopyRecursive("", Application.Context.FilesDir.AbsolutePath);
            }
            catch
            {

            }
        }

        void CopyAssetFolder(string assetDir, string targetDir)
        {
            var assets = Assets;
            var files = assets.List(assetDir);

            if (files.Length == 0)
            {
                // It's a file
                using var assetStream = assets.Open(assetDir);
                using var fileStream = new FileStream(targetDir, FileMode.Create);
                assetStream.CopyTo(fileStream);
                return;
            }

            // It's a directory
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            foreach (var file in files)
            {
                CopyAssetFolder(
                    Path.Combine(assetDir, file),
                    Path.Combine(targetDir, file)
                );
            }
        }
        private void FullScreen()
        {
            var decorView = Window.DecorView;
            int uiOptions = (int)SystemUiFlags.Fullscreen | (int)SystemUiFlags.HideNavigation | (int)SystemUiFlags.ImmersiveSticky;
            decorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            SupportActionBar?.Hide();
        }
    }
}