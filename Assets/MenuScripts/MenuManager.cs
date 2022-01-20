using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Button SingleplayerButton;
    public Button MultiplayerButton;

    const string SingleplayerScene = "SingleplayerScene";
    const string ServerScene = "ServerScene";

    void Start()
    {
        SingleplayerButton.onClick.AddListener(LoadSingleplayer);
        MultiplayerButton.onClick.AddListener(LoadMultiplayer);
    }

    void LoadSingleplayer()
    {
        SceneManager.LoadScene(SingleplayerScene);
    }

    void LoadMultiplayer()
    {
        SceneManager.LoadScene(ServerScene);
    }
}
