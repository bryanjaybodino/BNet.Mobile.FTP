using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;

using System.Threading.Tasks;

namespace BNet.Mobile.FTP.Services.PermissionServices
{
    public class ManageStorage
    {
        public static void RequestExternalPermission()
        {
            if (!Android.OS.Environment.IsExternalStorageManager)
            {
                var intent = new Android.Content.Intent(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
                intent.SetData(Android.Net.Uri.Parse("package:" + Application.Context.PackageName));
                intent.AddFlags(ActivityFlags.NewTask);
                Application.Context.StartActivity(intent);
            }
        }
        public static bool GetExternalPermissions()
        {
            return Android.OS.Environment.IsExternalStorageManager;
        }
    }
}