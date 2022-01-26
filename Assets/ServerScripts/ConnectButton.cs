using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UNET;
using Unity.Netcode;

public class ConnectButton : MonoBehaviour
{
    public Text IPtext;
    public Text portText;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Connect);
    }

    void Connect()
    {
        if (NetworkManager.Singleton.IsClient) 
        {
            NetworkManager.Singleton.Shutdown();
            GetComponent<Button>().GetComponentInChildren<Text>().text = "Connect";
        }
        else if (!NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = IPtext.text;
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = int.Parse(portText.text);
            NetworkManager.Singleton.StartClient();
            GetComponent<Button>().GetComponentInChildren<Text>().text = "Searching";
        }
    }

    public void Reset()
    {
        GetComponent<Button>().GetComponentInChildren<Text>().text = "Connect";
    }
}
