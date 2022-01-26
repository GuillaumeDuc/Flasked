using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LoadMultiplayer : MonoBehaviour
{
    public Text foundText;
    public GameObject loadNextLevelButton;


    void Start() 
    {
        loadNextLevelButton.GetComponent<Button>().onClick.AddListener(LoadMultiplayerScene);
    }

    void Update()
    {
        if (NetworkManager.Singleton.IsHost) 
        {
            // First client is host
            if (NetworkManager.Singleton.ConnectedClientsIds.Count > 1) 
            {
                foundText.text = "Client Connected";
                if (!loadNextLevelButton.activeInHierarchy) 
                {
                    loadNextLevelButton.SetActive(true);
                }
            }
            else if (foundText.text.Length > 0)
            {
                foundText.text = "";
                loadNextLevelButton.SetActive(false);
            }
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                foundText.text = "Connected";
            }
        }
        else if (foundText.text.Length > 0)
        {
            foundText.text = "";
        }
    }

    void LoadMultiplayerScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(Scenes.MultiplayerScene.ToString(), LoadSceneMode.Single);
    }
}
