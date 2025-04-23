using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;
    public AudioSource fadeSound;
    void Start()
    {
        // Start by fading in
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeIn()
{
    float time = fadeDuration;
    while (time > 0)
    {
        time -= Time.deltaTime;
        float alpha = time / fadeDuration;
        SetAlpha(alpha);
        yield return null;
    }
    SetAlpha(0);
    //fadeImage.gameObject.SetActive(false); 
}


    IEnumerator FadeOut(string sceneName)
{
    // 🔊 Play the sound effect at the start of the fade
    if (fadeSound != null)
        fadeSound.Play();

    float time = 0;
    while (time < fadeDuration)
    {
        time += Time.deltaTime;
        float alpha = time / fadeDuration;
        SetAlpha(alpha);
        yield return null;
    }
    SetAlpha(1);
    SceneManager.LoadScene(sceneName);
}


    void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
