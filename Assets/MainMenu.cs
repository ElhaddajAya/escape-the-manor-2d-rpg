using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public SceneFader sceneFader; // 👈 Drag your SceneFader GameObject here in Inspector

    public void PlayGame()
    {
        sceneFader.FadeToScene("Main_Scene"); // 👈 Trigger fade instead of direct load
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
