using System.Net;  
using UnityEngine;
using Unity.Netcode.Transports.UNET;
using UnityEngine.UI;
using Unity.Netcode;

public class StartHostButton : MonoBehaviour
{
    public Text IPText;
    public Text portText;
    public Text ipInput;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(StartHost);
    }
    
    void StartHost()
    {
        if(!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ipInput.text;
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = int.Parse(portText.text);
            NetworkManager.Singleton.StartHost();
            IPText.text = "Waiting for connection on " + ipInput.text + " : " + portText.text + " ...";
            GetComponent<Button>().GetComponentInChildren<Text>().text = "Stop";
        }
        else 
        {
            NetworkManager.Singleton.Shutdown();
            Reset();
        }
    }

    public void Reset() 
    {
        GetComponent<Button>().GetComponentInChildren<Text>().text = "Start";
        IPText.text = "";
    }
}
