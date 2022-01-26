using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Net;

public class IPInput : MonoBehaviour
{
    public InputField inputField;
    public bool autoFill = false;

    void Start()
    {
        if (autoFill)
        {
            // inputField.text = new WebClient().DownloadString("https://ipv4.icanhazip.com/");
            inputField.text = GetLocalIPAddress();
        }

        inputField.onValueChanged.AddListener((s) => { ValueChangeCheck(s); });
    }

    public void ValueChangeCheck(string s)
    {
        bool a = Regex.IsMatch(s, "^\\d*(\\d+[\\.d]\\d*)*$");
        if (!a)
        {
            s = s.Remove(s.Length - 1);
            inputField.text = s;
        }
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
