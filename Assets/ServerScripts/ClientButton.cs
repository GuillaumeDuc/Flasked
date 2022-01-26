using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ClientButton : MonoBehaviour
{
    public GameObject clientDisplay;
    public GameObject hostDisplay;
    public GameObject hostButton;
    public Text IPText;

    void Start()
    {
        Button clientButton = GetComponent<Button>();
        clientButton.onClick.AddListener(DisplayClient);
    }

    void DisplayClient()
    {
        if(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }

        hostButton.SetActive(true);
        hostDisplay.SetActive(false);

        clientDisplay.SetActive(true);
        this.gameObject.SetActive(false);
        clientDisplay.GetComponentInChildren<ConnectButton>().Reset();
        IPText.text = "";
    }
}
