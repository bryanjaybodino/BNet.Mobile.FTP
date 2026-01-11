using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.FTP.Services.NetworkServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Mobile.FTP
{
    internal class HtmlElement
    {
        public static string Label_Connection = "Label_Connection";
        public static string Icon_RunService = "Icon_RunService";
        public static string Label_RunService = "Label_RunService";
        public static void Update(ScriptContext scriptContext)
        {
            scriptContext.UpdateInnerHtml(Label_Connection, NetworkChecker.Connection());
        }

    }
}