using UnityEngine;
using TMPro;
using System.Collections;

public class SceneTitleDisplay : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public float fadeDuration = 1f;
    public float displayTime = 3f; // Time fully visible before fade-out

    void Start()
    {
        if (titleText != null)
        {
            StartCoroutine(ShowTitleWithFade());
        }
    }

    IEnumerator ShowTitleWithFade()
    {
        titleText.gameObject.SetActive(true);

        // Fade In
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        // Wait while fully visible
        yield return new WaitForSeconds(displayTime);

        // Fade Out
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (t / fadeDuration));
            SetAlpha(alpha);
            yield return null;
        }

        titleText.gameObject.SetActive(false);
    }

    void SetAlpha(float a)
    {
        Color color = titleText.color;
        color.a = a;
        titleText.color = color;
    }
}
