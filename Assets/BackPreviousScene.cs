using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackPreviousScene : MonoBehaviour
{
    public Scenes scene;

    void Start()
    {
        Button backButton = GetComponent<Button>();
        backButton?.onClick.AddListener(LoadScene);
    }

    void LoadScene()
    {
        SceneManager.LoadScene(scene.ToString());
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                LoadScene();
            }
        }
    }
}
