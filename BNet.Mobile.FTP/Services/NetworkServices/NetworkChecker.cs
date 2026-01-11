using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.FTP.Services.NetworkServices
{
    internal class NetworkChecker
    {
        public static int PortNumber()
        {
            return 8040;
        }
        public static string Connection()
        {
            string Hotspot = NetworkConnections.GetHotspotIpAddress();
            string Wifi = NetworkConnections.GetWifiIpAddress();
            if (Wifi != "0.0.0.0")
            {
                return "ftp://" + Wifi + ":" + PortNumber();
            }
            else if (Hotspot != "127.0.0.1")
            {
                return "ftp://" + Hotspot + ":" + PortNumber();
            }
            else
            {
                return "No Connection Available";
            }
        }

        static bool IsMobileDataEnabled(Context context)
        {
            try
            {
                ConnectivityManager connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
                NetworkInfo mobileInfo = connectivityManager.GetNetworkInfo(ConnectivityType.Mobile);

                return mobileInfo != null && mobileInfo.IsConnected;
            }
            catch
            {
                return false;
            }
        }
        static bool IsWifiEnabled(Context context)
        {
            try
            {
                WifiManager wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);
                return wifiManager != null && wifiManager.IsWifiEnabled;
            }
            catch
            {
                return false;
            }
        }
        public static bool HasInternet()
        {
            bool isMobileDataEnabled = IsMobileDataEnabled(Android.App.Application.Context);
            bool isWifiEnabled = IsWifiEnabled(Android.App.Application.Context);
            if (isWifiEnabled)
            {
                return true;
            }
            else if (isMobileDataEnabled)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}