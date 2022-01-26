using System.Net;  
using UnityEngine;
using Unity.Netcode.Transports.UNET;
using UnityEngine.UI;
using Unity.Netcode;

public class HostButton : MonoBehaviour
{
    public GameObject clientDisplay;
    public GameObject clientButton;
    public GameObject hostDisplay;

    void Start()
    {
        Button hostButton = GetComponent<Button>();
        hostButton.onClick.AddListener(StartHost);
    }

    void StartHost()
    {
        if(NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }

        clientDisplay.SetActive(false);
        clientButton.SetActive(true);
        hostDisplay.SetActive(true);
        hostDisplay.GetComponentInChildren<StartHostButton>().Reset();
        this.gameObject.SetActive(false);
    }
}
