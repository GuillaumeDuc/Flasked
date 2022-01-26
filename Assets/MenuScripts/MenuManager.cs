using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Button SingleplayerButton;
    public Button MultiplayerButton;

    void Start()
    {
        SingleplayerButton.onClick.AddListener(LoadSingleplayer);
        MultiplayerButton.onClick.AddListener(LoadMultiplayer);
    }

    void LoadSingleplayer()
    {
        SceneManager.LoadScene(Scenes.SingleplayerScene.ToString());
    }

    void LoadMultiplayer()
    {
        SceneManager.LoadScene(Scenes.ServerScene.ToString());
    }
}
