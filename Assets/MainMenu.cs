using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public SceneFader sceneFader; // Reference to the fader
    public string sceneToLoad = "Main_Scene";

    public void PlayGame()
    {
        sceneFader.FadeToScene(sceneToLoad); // Use the fade instead of direct load
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    } 
}
