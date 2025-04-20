using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //public string gameSceneName = "Main_Scene";

    public void PlayGame()
    {
        SceneManager.LoadScene("Main_Scene");
    }
}
