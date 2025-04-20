using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public string gameSceneName = "Main_Scene";

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
