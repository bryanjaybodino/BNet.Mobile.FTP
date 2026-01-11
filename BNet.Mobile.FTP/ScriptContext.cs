using Android.App;
using Android.Content;
using Android.Webkit;
using Android.Widget;
using BNet.Mobile.FTP.Services.NetworkServices;
using BNet.Mobile.FTP.Services.PermissionServices;
using Java.Interop;
using System.Threading.Tasks;
using static Android.Provider.DocumentsContract;



namespace BNet.Mobile.FTP
{
    public class ScriptContext : Java.Lang.Object
    {



        private readonly Context context;
        private readonly WebView webView;
        public ScriptContext(Context context, WebView webView)
        {
            this.context = context;
            this.webView = webView;
        }


        [JavascriptInterface]
        [Export("Service")]
        public async void Service(string IconClass)
        {
            bool hasAccess = ManageStorage.GetExternalPermissions();
            if (hasAccess)
            {
                if (IconClass == "bi bi-stop-circle fs-3 text-danger")
                {
                    UpdateElementAttribute(HtmlElement.Icon_RunService, "class", "bi bi-play-circle fs-3 text-success");
                    UpdateInnerHtml(HtmlElement.Label_RunService, "Start Service");
                    await MainActivity.ftpServer.StopAsync();
                }
                else
                {
                    UpdateElementAttribute(HtmlElement.Icon_RunService, "class", "bi bi-stop-circle fs-3 text-danger");
                    UpdateInnerHtml(HtmlElement.Label_RunService, "Service is running");
                    await MainActivity.ftpServer.StartAsync();
                }
            }
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(context);
                alert.SetTitle("Permission Required");
                alert.SetMessage("This app needs permission to storage");
                alert.SetPositiveButton("Open Settings", (senderAlert, args) =>
                {
                    ManageStorage.RequestExternalPermission();
                });
                alert.Show();
            }
        }






        public void UpdateValue(string id, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            // Offload heavy processing to a background task
            Task.Run(() =>
            {
                // Escape single quotes in the value
                string escapedValue = value.Replace("'", "\\'");
                string js = $"document.getElementById('{id}').value = '{escapedValue}';";

                // Only switch to UI thread for JS evaluation
                webView.Post(() =>
                {
                    webView.EvaluateJavascript(js, null);
                });
            });
        }
        public void UpdateInnerHtml(string id, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            // Offload heavy processing to a background task
            Task.Run(() =>
            {
                // Escape single quotes in the value
                string escapedValue = value.Replace("'", "\\'");
                string js = $"document.getElementById('{id}').innerHTML = '{escapedValue}';";

                // Only switch to UI thread for JS evaluation
                webView.Post(() =>
                {
                    webView.EvaluateJavascript(js, null);
                });
            });
        }

        public void UpdateElementAttribute(string id, string attribute, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            // Offload heavy processing to a background task
            Task.Run(() =>
            {
                // Escape single quotes in the value
                string escapedValue = value.Replace("'", "\\'");
                string js = $"document.getElementById('{id}').setAttribute('{attribute}', '{escapedValue}');";
                // Only switch to UI thread for JS evaluation
                webView.Post(() =>
                {
                    webView.EvaluateJavascript(js, null);
                });
            });
        }


        public void RemoveAttribute(string id, string attribute)
        {
            // Offload heavy processing to a background task
            Task.Run(() =>
            {
                string js = $"document.getElementById('{id}').removeAttribute('{attribute}');";
                // Only switch to UI thread for JS evaluation
                webView.Post(() =>
                {
                    webView.EvaluateJavascript(js, null);
                });
            });
        }

        public void ToastShort(string message)
        {
            Android.App.Application.SynchronizationContext.Post(_ =>
            {
                Toast.MakeText(context, message, ToastLength.Short).Show();
            },
            null);
        }
    }
}