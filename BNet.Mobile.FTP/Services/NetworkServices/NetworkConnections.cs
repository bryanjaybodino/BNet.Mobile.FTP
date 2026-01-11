using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace BNet.Mobile.FTP.Services.NetworkServices
{
    internal class NetworkConnections
    {
        public static string GetHotspotIpAddress()
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var netInterface in interfaces)
                {
                    var addressInfoList = netInterface.GetIPProperties().UnicastAddresses;
                    foreach (var addressInfo in addressInfoList)
                    {
                        string ip = addressInfo.Address.ToString();

                        if (addressInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            // Return the first IPv4 address found
                            return addressInfo.Address.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }

            return "Not connected to hotspot";
        }

        public static string GetWifiIpAddress()
        {
            try
            {
                WifiManager wifiMgr = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
                if (wifiMgr != null && wifiMgr.ConnectionInfo != null)
                {
                    int ipAddress = wifiMgr.ConnectionInfo.IpAddress;
                    return Android.Text.Format.Formatter.FormatIpAddress(ipAddress);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }

            return "Not connected to Wi-Fi";
        }
    }
}